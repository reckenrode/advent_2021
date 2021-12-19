// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day13

type Point = { X: int; Y: int }

module Point =
    let origin = { X = 0; Y = 0 }

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
            let v = vector (pt.X, pt.Y, 1) |> Vector.toCol
            let m = Matrix.matrixProduct t v

            { X = Matrix.get Z Z m
              Y = Matrix.get (S Z) Z m })
        |> Set.union unchanged
        |> Points

    let reflectHorizontal y =
        reflect (matrix ((1, 0, 0), (0, -1, 2 * y), (0, 0, 1))) (fun pt -> pt.Y <= y)

    let reflectVertical x =
        reflect (matrix ((-1, 0, 2 * x), (0, 1, 0), (0, 0, 1))) (fun pt -> pt.X <= x)

    let render (Points pts) =
        let bounds =
            pts
            |> Set.fold
                (fun bound ({ X = x; Y = y }) -> { X = max bound.X x; Y = max bound.Y y })
                Point.origin

        seq { 0 .. bounds.Y }
        |> Seq.map (fun y ->
            seq { 0 .. bounds.X }
            |> Seq.map (fun x ->
                if Set.contains { X = x; Y = y } pts then
                    "#"
                else
                    ".")
            |> String.concat "")
        |> String.concat "\n"

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
                { X = ToInt32 (lhs.String, 10)
                  Y = ToInt32 (rhs.String, 10) })

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
    Input: FileInfo
}

let run (options: Options) (console: IConsole) =
    task {
        use file = options.Input.OpenRead()
        use reader = new StreamReader(file)
        let! input = reader.ReadToEndAsync()

        return
            monad' {
                let! points, flips = Instructions.parse input

                let points =
                    flips
                    |> foldi
                        (fun points n flip ->
                            if n = 1 then
                                let message = $"Points after first flip: {Points.count points}\n"
                                console.Out.Write message
                            Instructions.Flip.apply points flip)
                        points

                console.Out.Write $"Final grid\n==========\n{Points.render points}\n"

                return 0
            }
            |> handleFailure console
    }

let command = Command.create "day13" "Transparent Origami" run
