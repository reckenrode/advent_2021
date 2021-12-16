// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day15

[<Struct>]
type Point = { row: int; column: int; value: uint }

type Graph = private {
    cells: array<uint32>
    rows: int
    columns: int }

module Graph =
    open System
    open System.Collections.Generic

    open FSharpPlus

    let rows { rows = rows } = rows
    let columns { columns = columns } = columns

    let ofList lst =
        monad' {
            let! firstRow = List.tryHead lst
            let rows, columns = List.length lst, List.length firstRow

            if not (List.forall (List.length >> ((=) columns)) lst) then
                Error "the list contains jagged rows, which is not supported"
            else
                let arr =
                    List.collect id lst
                    |> Array.ofList
                Ok { cells = arr; rows = rows; columns = columns }
        }
        |> Option.defaultValue (
            Ok { cells = Array.zeroCreate 0; rows = 0; columns = 0 }
        )

    let grow (xTimes, yTimes) graph =
        let wrap x = 1u + (x - 1u) % 9u
        [ for row in 0 .. rows graph - 1 do
            [ for column in 0 .. columns graph - 1 -> graph.cells[row * rows graph + column] ] ]
        |> List.map
           (List.replicate (xTimes + 1)
            >> List.mapi (fun idx lst -> List.map (((+) (uint32 idx)) >> wrap) lst)
            >> List.collect id)
        |> List.replicate (yTimes + 1)
        |> List.mapi (fun idx lst -> List.map (List.map (((+) (uint32 idx)) >> wrap)) lst)
        |> List.collect id
        |> ofList
        |> Result.get

    let shortestPathCost start goal graph =
        let { cells = cells; rows = rows; columns = columns } = graph

        let idx row column = row * columns + column

        let tryGetNode = function
            | row, column when row >= 0 && column >= 0 && row < rows && column < columns ->
                Array.tryItem (idx row column)
                >> Option.map (fun value -> { row = row; column = column; value = value })
            | _ -> (fun _ -> None)

        let rec shortestPathCost' current (distances: array<uint32>) (visited: array<bool>) (visiting: PriorityQueue<int * int, uint32>) =
            if visited[idx <|| goal] || visiting.Count = 0 then
                distances[idx <|| goal]
            else
                [
                    tryGetNode (current.row + 1, current.column) cells
                    tryGetNode (current.row - 1, current.column) cells
                    tryGetNode (current.row, current.column + 1) cells
                    tryGetNode (current.row, current.column - 1) cells
                ]
                |> List.choose (function
                    | Some pt as opt when not visited[idx pt.row pt.column] -> opt
                    | _ -> None)
                |> List.iter (fun pt ->
                    let currentDistance = distances[idx pt.row pt.column]
                    let distance = distances[idx current.row current.column] + pt.value
                    if distance < currentDistance then
                        distances[idx pt.row pt.column] <- distance
                        visiting.Enqueue ((pt.row, pt.column), distance))
                let current =
                    visiting.Dequeue ()
                    |> flip tryGetNode cells
                    |> Option.get
                shortestPathCost' current distances visited visiting


        let distances = Array.init (rows * columns) (fun index ->
            if index = (idx <|| start) then 0u else UInt32.MaxValue)

        let visiting = PriorityQueue ()
        for row = 0 to rows - 1 do
            for column = 0 to columns - 1 do
                visiting.Enqueue ((row, column), distances[idx row column])

        let visited = Array.create (rows * columns) false

        let start = tryGetNode start cells |> Option.get
        shortestPathCost' start distances visited visiting

open System.CommandLine
open System.IO

open FSharpPlus

type Options = {
    input: FileInfo
}

let run (options: Options) (console: IConsole) =
    task {
        use file = options.input.OpenRead ()
        use reader = new StreamReader (file)
        let! input = reader.ReadToEndAsync ()

        return
            monad' {
                let! graph =
                    input.TrimEnd ()
                    |> String.split [ "\n" ]
                    |> Seq.map (Seq.map (fun ch -> (uint32 ch) - (uint32 '0')) >> List.ofSeq)
                    |> List.ofSeq
                    |> Graph.ofList

                let cavernRisk =
                    graph
                    |> Graph.shortestPathCost (0, 0) (Graph.rows graph - 1, Graph.columns graph - 1)
                console.Out.Write $"The lowest risk of any path out of the cavern: {cavernRisk}\n"

                let graph = Graph.grow (4, 4) graph
                let caveRisk =
                    graph
                    |> Graph.shortestPathCost (0, 0) (499, 499)
                console.Out.Write $"The lowest risk of any path out of the cave: {caveRisk}\n"

                return 0
            }
            |> handleFailure console
    }

let command = Command.create "day15" "Chiton" run
