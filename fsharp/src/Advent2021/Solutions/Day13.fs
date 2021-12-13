// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day13

type Point = { x: int; y: int }

type Points = Points of Set<Point>

module Points =
    open FSharpPlus.Data
    open FSharpPlus.TypeLevel

    let count (Points pts) = Set.count pts

    let ofList = Set.ofList >> Points

    let private reflect t f (Points pts) =
        let unchanged, reflected = Set.partition f pts

        reflected
        |> Set.map (fun pt ->
            let v = vector (pt.x, pt.y, 1) |> Vector.toCol
            let m = Matrix.matrixProduct t v

            { x = Matrix.get Z Z m
              y = Matrix.get (S Z) Z m })
        |> Set.union unchanged
        |> Points

    let reflectHorizontal y =
        reflect (matrix ((1, 0, 0), (0, -1, 2 * y), (0, 0, 1))) (fun pt -> pt.y <= y)

    let reflectVertical x =
        reflect (matrix ((-1, 0, 2 * x), (0, 1, 0), (0, 0, 1))) (fun pt -> pt.x <= x)

module Instructions =
    open type System.Convert

    open FParsec

    type Flip =
        | Vertical of int
        | Horizontal of int

    module Flip =
        let apply pts = function
        | Vertical x -> Points.reflectVertical x pts
        | Horizontal y -> Points.reflectHorizontal y pts

    let parse input =
        let number =
            numberLiteral NumberLiteralOptions.DefaultInteger

        let newlineThen a = (newline .>>? followedBy a)

        let point =
            number "x" .>> pstring "," .>>. number "y"
            |>> (fun (lhs, rhs) ->
                { x = ToInt32 (lhs.String, 10)
                  y = ToInt32 (rhs.String, 10) })

        let points = sepBy1 point (newlineThen point) |>> Points.ofList

        let foldAxis = (choiceL [ pstring "x="; pstring "y=" ] "fold axis")
        let flip =
            pstring "fold along " >>. foldAxis .>>. number "line"
            |>> (function
            | ("x=", x) -> Vertical (ToInt32 (x.String, 10))
            | ("y=", y) -> Horizontal (ToInt32 (y.String, 10))
            | _ -> failwith "unexpected axis: this should not happen")

        let flips = sepBy1 flip (newlineThen flip)

        let instructions =
            points .>> newline .>> newline .>>. flips

        let complete =
            instructions .>> (attempt eof <|> (newline >>. eof))

        match run complete input with
        | Success (instructions, _, _) -> Result.Ok instructions
        | Failure (message, _, _) -> Result.Error message

open System.CommandLine
open System.IO

open FSharpPlus

type Options = {
    input: FileInfo
}

let run (options: Options) (console: IConsole) =
    let handleFailure =
        function
        | Ok code -> code
        | Error message ->
            console.Error.Write $"Error parsing file: {message}"
            1

    task {
        use file = options.input.OpenRead()
        use reader = new StreamReader(file)
        let! input = reader.ReadToEndAsync()

        return
            monad' {
                let! points, flips = Instructions.parse input

                let points =
                    flips
                    |> foldi
                        (fun points n flip ->
                            console.Out.Write $"Points after flip #{n}: {Points.count points}\n"
                            Instructions.Flip.apply points flip)
                        points

                console.Out.Write $"Points after the final flip: {Points.count points}\n"

                return 0
            }
            |> handleFailure
    }

let command = Command.create "day13" "Transparent Origami" run
