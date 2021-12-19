// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day18

open FSharpPlus

let debug x =
    printfn "{x}"
    x

type Cell =
    { Depth: uint32
      Value: uint64 }

    static member (+) (lhs, rhs) =
        { lhs with Value = lhs.Value + rhs.Value }

    static member (+) (lhs, rhs: uint32) = { lhs with Depth = lhs.Depth + rhs }

    static member (/) (lhs, rhs: uint64) =
        { Depth = lhs.Depth + 1u
          Value = lhs.Value / rhs }

    static member (/^) (lhs, rhs: uint64) =
        let value = lhs.Value / rhs + lhs.Value % 2UL

        { Depth = lhs.Depth + 1u
          Value = value }

    static member Zero = { Depth = zero; Value = zero }

module Cell =
    let depth (c: Cell) = c.Depth
    let value (c: Cell) = c.Value

type SnailfishNumber =
    private
    | SnailfishNumber of list<Cell>

    member this.Reduce () =
        let (SnailfishNumber num) = this

        let rec reduce' visited =
            function
            | lhs :: rhs :: successor :: rest when lhs.Depth = rhs.Depth && lhs.Depth = 4u ->
                List.append
                    (rev visited)
                    (Cell.Zero + (lhs.Depth - 1u)
                    :: successor + rhs :: rest)
                |> reduce' []
            | prior :: lhs :: rhs :: successor :: rest when lhs.Depth = rhs.Depth && lhs.Depth = 4u ->
                List.append
                    (rev visited)
                    (prior + lhs
                    :: Cell.Zero + (lhs.Depth - 1u)
                        :: successor + rhs :: rest)
                |> reduce' []
            | prior :: lhs :: [ rhs ] when lhs.Depth = rhs.Depth && lhs.Depth = 4u ->
                List.append (rev visited) (prior + lhs :: [ Cell.Zero + (lhs.Depth - 1u) ])
                |> reduce' []
            | x :: xs when x.Value >= 10UL ->
                List.append (rev visited) (x / 2UL :: x /^ 2UL :: xs)
                |> reduce' []
            | x :: xs -> reduce' (x :: visited) xs
            | [] -> rev visited

        reduce' [] num |> SnailfishNumber

    static member (+) (SnailfishNumber lhs, SnailfishNumber rhs) =
        match (lhs, rhs) with
        | ([], rhs) -> SnailfishNumber rhs
        | (lhs, []) -> SnailfishNumber lhs
        | (lhs, rhs) ->
            let result =
                List.append lhs rhs
                |> List.map (fun cell -> { cell with Depth = cell.Depth + 1u })
                |> SnailfishNumber

            result.Reduce ()

    static member Zero = SnailfishNumber []

    override this.ToString () =
        let (SnailfishNumber num) = this
        let depths, nums =
            num
            |> map (fun cell -> string cell.Depth, string cell.Value)
            |> unzip

        let depths = String.intercalate " " depths
        let nums = String.intercalate " " nums
        $"\t{depths}\n\t{nums}"

module SnailfishNumber =
    open FParsec

    type private SnailfishNumberAst =
        | Pair of SnailfishNumberAst * SnailfishNumberAst
        | Element of uint64

    let magnitude (SnailfishNumber num) =
        let rec magnitude' depth acc =
            function
            | [] ->
                match acc with
                | [ x ] -> x.Value
                | xs -> magnitude' (depth - 1u) [] (rev xs)
            | [ x ] -> magnitude' depth (x :: acc) []
            | lhs :: rhs :: rest when lhs.Depth = rhs.Depth && lhs.Depth = depth ->
                magnitude'
                    depth
                    ({ Depth = lhs.Depth - 1u
                       Value = 3UL * lhs.Value + 2UL * rhs.Value }
                     :: acc)
                    rest
            | lhs :: rest -> magnitude' depth (lhs :: acc) rest

        magnitude' (List.maxBy Cell.depth num |> Cell.depth) [] num

    let reduce (num: SnailfishNumber) = num.Reduce ()

    let tryParse str =
        let parser =
            let number, numberRef =
                createParserForwardedToRef<SnailfishNumberAst, unit> ()

            let pair =
                between (pchar '[') (pchar ']') (number .>> (pchar ',') .>>. number)
                |>> Pair

            let element = puint64 |>> Element

            numberRef.Value <-
                (attempt element) <?> "element" <|> pair
                <?> "pair"

            pair .>> eof

        let flatten =
            let rec flatten' depth ast =
                seq {
                    match ast with
                    | Element value -> yield { Depth = (depth - 1u); Value = value }
                    | Pair (lhs, rhs) ->
                        yield! flatten' (depth + 1u) lhs
                        yield! flatten' (depth + 1u) rhs
                }

            flatten' 0u

        match run parser str with
        | Success (number, _, _) ->
            flatten number
            |> List.ofSeq
            |> SnailfishNumber
            |> reduce
            |> Some
        | _ -> None

open FSharp.Control

open System.CommandLine
open System.IO

type Options = { Input: FileInfo }

let run (options: Options) (console: IConsole) =
    let rec snailwisePairs =
        function
        | x :: xs ->
            unzip (map (fun y -> (x, y), (y, x)) xs)
            |> uncurry List.append
            |> flip List.append (snailwisePairs xs)

        | [] -> []

    task {
        use file = options.Input.OpenRead ()
        use reader = new StreamReader (file)

        let lines = lines reader

        let! problems =
            lines
            |> AsyncSeq.choose SnailfishNumber.tryParse
            |> AsyncSeq.toListAsync

        let sum = List.sum problems
        let magnitude = SnailfishNumber.magnitude sum

        console.Out.Write $"The magnitude of the sum of the problem is: {magnitude}\n"

        let pairs = snailwisePairs problems

        let largestMagnitude =
            pairs
            |> List.map (uncurry (+) >> SnailfishNumber.magnitude)
            |> List.max

        console.Out.Write $"The largest magnitude of two of the addends: {largestMagnitude}\n"

        return 0
    }

let command = Command.create "day18" "Snailfish" run
