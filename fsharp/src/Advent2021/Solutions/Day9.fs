// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day9

open System.CommandLine
open System.IO
open System.Text

open FParsec

type HeightMap = HeightMap of uint8 [,]

type Point = { Row: int; Column: int; Value: uint8 }

type Neighbors =
    { Top: option<Point>
      Bottom: option<Point>
      Left: option<Point>
      Right: option<Point> }

module HeightMap =
    let heightMap =
        let row =
            many1 (digit |>> (fun ch -> (uint8 ch) - (uint8 '0')))

        let heightMap =
            sepBy1 row (newline .>>? followedBy digit)
            |>> (array2D >> HeightMap)

        heightMap .>> (newline >>? eof <|> eof)

    let parse s =
        match run heightMap s with
        | Success (heightmap, _, _) -> Result.Ok heightmap
        | Failure (message, _, _) -> Result.Error message

    let parseFile (f: FileInfo) =
        use stream = f.OpenRead ()

        match runParserOnStream heightMap () f.Name stream Encoding.UTF8 with
        | Success (heightmap, _, _) -> Result.Ok heightmap
        | Failure (message, _, _) -> Result.Error message

    let width (HeightMap hm) = hm.GetLength 1
    let height (HeightMap hm) = hm.GetLength 0

    let filter f ((HeightMap hm) as self) =
        { 0 .. height self - 1 }
        |> Seq.collect (fun row ->
            { 0 .. width self - 1 }
            |> Seq.map (fun column -> row, column))
        |> Seq.map (fun (row, column) ->
            { Row = row
              Column = column
              Value = hm[row, column] })
        |> Seq.filter (fun pt ->
            let row, column = pt.Row, pt.Column

            let neighbors =
                { Top =
                    if row = 0 then
                        None
                    else
                        Some (
                            { Row = row - 1
                              Column = column
                              Value = hm[row - 1, column] }
                        )
                  Bottom =
                    if row = height self - 1 then
                        None
                    else
                        Some (
                            { Row = row + 1
                              Column = column
                              Value = hm[row + 1, column] }
                        )
                  Left =
                    if column = 0 then
                        None
                    else
                        Some (
                            { Row = row
                              Column = column - 1
                              Value = hm[row, column - 1] }
                        )
                  Right =
                    if column = width self - 1 then
                        None
                    else
                        Some (
                            { Row = row
                              Column = column + 1
                              Value = hm[row, column + 1] }
                        ) }

            f pt neighbors)

    let mapBasin (row, column) ((HeightMap hm) as self) =
        let rec mapBasin' pt seen =
            let neighbors =
                let row, column = pt.Row, pt.Column

                seq {
                    (row - 1, column)
                    (row + 1, column)
                    (row, column - 1)
                    (row, column + 1)
                }
                |> Seq.filter (fun (row, column) ->
                    row >= 0
                    && row < height self
                    && column >= 0
                    && column < width self)
                |> Seq.map (fun (row, column) ->
                    { Row = row
                      Column = column
                      Value = hm[row, column] })

            neighbors
            |> Seq.fold
                (fun seen otherPt ->
                    if not (Set.contains otherPt seen)
                       && otherPt.Value <> 9uy
                       && otherPt.Value > pt.Value then
                        mapBasin' otherPt (Set.add otherPt seen)
                    else
                        seen)
                seen

        let point =
            { Row = row
              Column = column
              Value = hm[row, column] }

        mapBasin' point (Set.ofList [ point ])

type Options = { Input: FileInfo }

open FSharpPlus

let private orDefault value opt =
    monad' {
        let! pt = opt
        return pt.Value
    }
    |> Option.defaultValue value

let run (options: Options) (console: IConsole) =
    task {
        let result =
            monad' {
                let! heightmap = HeightMap.parseFile options.Input

                let lowestPoints =
                    heightmap
                    |> HeightMap.filter (fun pt neighbors ->
                        [ neighbors.Top |> orDefault 0xAuy
                          neighbors.Bottom |> orDefault 0xAuy
                          neighbors.Left |> orDefault 0xAuy
                          neighbors.Right |> orDefault 0xAuy ]
                        |> List.forall (fun neighbor -> pt.Value < neighbor))

                let riskLevels =
                    lowestPoints
                    |> Seq.sumBy (fun pt -> uint32 (pt.Value + 1uy))

                printfn $"The sum of the risk levels: {riskLevels}"

                let basins =
                    lowestPoints
                    |> Seq.map (fun pt ->
                        HeightMap.mapBasin (pt.Row, pt.Column) heightmap
                        |> Set.count)
                    |> Seq.sortDescending
                    |> Seq.take 3
                    |> Seq.reduce (*)

                printfn $"The product of the three largest basins: {basins}"

                return 0
            }

        match result with
        | Result.Ok result -> return result
        | Result.Error message ->
            eprintfn $"{message}"
            return 1
    }

let command = Command.create "day9" "Smoke Basin" run
