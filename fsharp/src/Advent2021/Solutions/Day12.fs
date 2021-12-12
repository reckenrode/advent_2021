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

    let paths (Graph graph) =
        let addNode node = Set.map (fun predecessor -> $"{predecessor},{node}")
        let isSmallNode = String.forall System.Char.IsLower
        let hasEnd = String.endsWith "end"
        let rec paths' start predecessors visitedSmallNodes =
            let children = Set.filter (flip Set.contains visitedSmallNodes >> not) graph[start]
            if start = "end" || Set.isEmpty children
            then predecessors
            else
                children
                |> Set.map (fun child ->
                    let visitedSmallNodes =
                        if isSmallNode child
                        then Set.add child visitedSmallNodes
                        else visitedSmallNodes
                    paths' child (addNode child predecessors) visitedSmallNodes
                )
                |> Set.unionMany
        let initialSets = Set.ofList ["start"]
        paths' "start" initialSets initialSets
        |> Set.filter hasEnd

open System.CommandLine
open System.IO

open FSharpPlus

type Options = {
    input: FileInfo
    print: bool
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

            let paths = Graph.paths graph
            console.Out.Write $"There are {Set.count paths} paths visiting small caves only once\n"
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
    command
