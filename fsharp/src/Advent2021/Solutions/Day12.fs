// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day12

type Graph = Graph of Map<string, Set<string>>

module Graph =
    open System.Text

    open FSharpPlus
    open FParsec

    let ofList = Map.ofList >> Graph

    let parse name stream =
        let ofTuples =
            List.fold (fun map (fromNode, toNode) ->
                let parents =
                    monad' {
                        let! parents = Map.tryFind toNode map
                        return Set.add fromNode parents
                    }
                    |> Option.defaultValue (Set.ofList [fromNode])
                let children =
                    monad' {
                        let! children = Map.tryFind fromNode map
                        return Set.add toNode children
                    }
                    |> Option.defaultValue (Set.ofList [toNode])
                map
                |> Map.add fromNode children
                |> Map.add toNode parents
            ) Map.empty
        let node = manySatisfy isAsciiLetter
        let line = tuple2 node (pstring "-" >>. node)
        let graph = sepBy1 line (newline .>>? followedBy line)
        let eof = attempt eof <|> (newline >>. eof)
        match runParserOnStream (graph .>> eof) () name stream Encoding.UTF8 with
        | Success (graph, _, _) -> Result.Ok (ofTuples graph |> Graph)
        | Failure (error, _, _) -> Result.Error error

    let paths (Graph graph) revisit =
        let addNode node = Set.map (fun predecessor -> $"{predecessor},{node}")
        let isSmallNode = String.forall System.Char.IsLower
        let updateSmallNodes child set = if isSmallNode child then Set.add child set else set

        let rec paths' f start predecessors visitedSmallNodes =
            if start = "end"
            then predecessors
            else
                graph[start]
                |> Set.filter ((<>) "start")
                |> Set.map (fun child ->
                    if Set.contains child visitedSmallNodes
                    then f predecessors visitedSmallNodes child
                    else
                        let visitedSmallNodes = updateSmallNodes child visitedSmallNodes
                        paths' f child (addNode child predecessors) visitedSmallNodes
                )
                |> Set.unionMany

        let smallCaveVisitor _ _ _ = Set.empty
        let smallCaveRevisitor predecessors visitedSmallNodes child =
            paths' smallCaveVisitor child (addNode child predecessors) visitedSmallNodes

        let visitor = if revisit then smallCaveRevisitor else smallCaveVisitor

        paths' visitor "start" (Set.ofList ["start"]) Set.empty
        |> Set.filter (String.endsWith "end")

open System.CommandLine
open System.IO

open FSharpPlus

type Options = {
    input: FileInfo
    print: bool
    allowRevisit: bool
}

let run (options: Options) (console: IConsole) =
    let toStatusCode = function
    | Ok code -> code
    | Error message ->
        console.Error.Write $"Error parsing graph: {message}"
        1
    task {
        return monad' {
            use file = options.input.OpenRead ()
            let! graph = Graph.parse file.Name file

            let paths = Graph.paths graph options.allowRevisit
            console.Out.Write $"There are {Set.count paths} paths\n"
            if options.print then Seq.iter (fun path -> console.Out.Write $"{path}\n") paths

            return 0
        }
        |> toStatusCode
    }

let command =
    let command = Command.create "day12" "Passage Pathing" run
    command.AddOption <| Option<bool> (
        aliases = [| "-p"; "--print" |],
        description = "print the paths"
    )
    command.AddOption <| Option<bool> (
        aliases = [| "-a"; "--allow-revisit" |],
        description = "allow a small cave to be revisited once"
    )
    command
