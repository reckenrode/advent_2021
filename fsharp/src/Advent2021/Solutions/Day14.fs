// SPDX,License,Identifier: GPL,3.0,only

module Advent2021.Solutions.Day14

type InsertionRules = private InsertionRules of Map<array<char>, char>

module InsertionRules =
    open FSharpPlus

    let ofList = Map.ofList >> InsertionRules

    let private insertPolymer (target: array<char>) (InsertionRules rules) =
        let polymer = rules |> Map.tryFind target

        target[0] :: Option.toList polymer

    let apply (xs: string) rules =
        xs
        |> Seq.windowed 2
        |> Seq.collect (flip insertPolymer rules)
        |> flip Seq.append (xs |> Seq.tryLast |> Option.toList)
        |> String.ofSeq

module Parser =
    open FSharpPlus
    open FParsec

    let parse str =
        let template = many1 letter |>> String.ofList

        let pattern =
            tuple2 letter letter
            |>> (fun (fst, snd) -> [| fst; snd |])

        let production = letter

        let rule =
            pattern .>> spaces .>> pstring "->" .>> spaces .>>. production

        let rules =
            sepBy1 rule (newline .>>? followedBy rule)

        let inputFormat =
            template .>> newline .>> newline .>>. rules

        let complete =
            inputFormat .>> (attempt eof <|> (newline >>. eof))

        match run complete str with
        | Success ((template, rules), _, _) -> Result.Ok(template, InsertionRules.ofList rules)
        | Failure (message, _, _) -> Result.Error message

open System.CommandLine
open System.IO

open FSharpPlus

type Options = { input: FileInfo; steps: int; print: bool }

let run (options: Options) (console: IConsole) =
    task {
        use file = options.input.OpenRead ()
        use reader = new StreamReader (file)
        let! input = reader.ReadToEndAsync ()

        return
            monad' {
                let! template, rules = Parser.parse input

                let polymerized =
                    let rec polymerized p =
                        seq {
                            yield p
                            yield! rules |> InsertionRules.apply p |> polymerized
                        }

                    polymerized template

                console.Out.Write $"Steps: {options.steps}\n"

                let iteration = Seq.item options.steps polymerized

                if options.print then
                    console.Out.Write $"\nResult\n======\n"
                    console.Out.Write iteration

                let stats =
                    iteration
                    |> Seq.groupBy id
                    |> Seq.map (fun (k, v) -> k, Seq.length v)
                    |> List.ofSeq

                console.Out.Write $"\nOccurences\n==========\n"
                stats
                |> List.sortBy (fun (k, v) -> k)
                |> List.iter (fun (key, value) ->
                    console.Out.Write $"{key}: {value}\n")

                let min = List.minBy snd stats
                let max = List.maxBy snd stats

                console.Out.Write $"\nLeast common element: {fst min} @ {snd min} occurrences\n"
                console.Out.Write $"Most common element: {fst max} @ {snd max} occurrences\n"
                console.Out.Write $"Difference: {snd max - snd min}\n"

                return 0
            }
            |> handleFailure console
    }

let command =
    let command =
        Command.create "day14" "Extended Polymerization" run

    command.AddOption (
        Option<int> (
            aliases = [| "-s"; "--steps" |],
            description = "the number of steps to apply",
            getDefaultValue = fun () -> 10
        )
    )
    command.AddOption (
        Option<bool> (
            aliases = [| "-p"; "--print" |],
            description = "print the result of the last step"
        )
    )

    command
