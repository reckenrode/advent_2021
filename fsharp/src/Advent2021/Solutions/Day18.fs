// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day18

open FSharpPlus

type Cell =
    { Depth: uint32
      Value: uint64 }

    static member (+)(lhs, rhs) =
        { lhs with Value = lhs.Value + rhs.Value }

    static member (+)(lhs, rhs: uint32) = { lhs with Depth = lhs.Depth + rhs }

    static member (/)(lhs, rhs: uint64) =
        { Depth = lhs.Depth + 1u
          Value = lhs.Value / rhs }

    static member (/^)(lhs, rhs: uint64) =
        let value = lhs.Value / rhs + lhs.Value % 2UL

        { Depth = lhs.Depth + 1u
          Value = value }

    static member Zero = { Depth = zero; Value = zero }

type SnailfishNumber =
    private
    | SnailfishNumber of list<Cell>

    member this.Reduce () =
        let (SnailfishNumber num) = this

        let rec reduce' visited num =
            match num with
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
            | prior :: lhs :: rhs :: [] when lhs.Depth = rhs.Depth && lhs.Depth = 4u ->
                List.append (rev visited) (prior + lhs :: Cell.Zero + (lhs.Depth - 1u) :: [])
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

module SnailfishNumber =
    open FParsec

    type private SnailfishNumberAst =
        | Pair of SnailfishNumberAst * SnailfishNumberAst
        | Element of uint64

    let magnitude (SnailfishNumber num) =
        let rec magnitude' acc =
            function
            | [] ->
                match acc with
                | x :: [] -> x.Value
                | xs -> magnitude' [] (rev xs)
            | x :: [] -> magnitude' (x :: acc) []
            | lhs :: rhs :: rest when lhs.Depth = rhs.Depth ->
                magnitude'
                    ({ Depth = lhs.Depth - 1u
                       Value = 3UL * lhs.Value + 2UL * rhs.Value }
                     :: acc)
                    rest
            | lhs :: rest -> magnitude' (lhs :: acc) rest

        magnitude' [] num

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

type Options = {
    Input: FileInfo
}

let run (options: Options) (console: IConsole) =
    task {
        use file = options.Input.OpenRead ()
        use reader = new StreamReader (file)

        let lines = lines reader

        let problems =
            lines
            |> AsyncSeq.choose SnailfishNumber.tryParse

        let! sum = AsyncSeq.sum problems
        let magnitude = SnailfishNumber.magnitude sum

        console.Out.Write $"The magnitude of the sum of the problem is: {magnitude}\n"

        return 0
    }

let command = Command.create "day18" "Snailfish" run
