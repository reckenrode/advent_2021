// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day10

module Syntax =
    open FParsec

    let rec recognize str =
        let terminal lhs rhs = pipe2 (pchar lhs) (pchar rhs) (fun _ _ -> ())
        let (chunks, chunksRef) = createParserForwardedToRef<unit, unit> ()
        let chunk lhs rhs =
            attempt (terminal lhs rhs)
            <|> pipe3 (pchar lhs) chunks (pchar rhs) (fun _ _ _ -> ())
        chunksRef.Value <- many (choice [|
            (chunk '(' ')')
            (chunk '[' ']')
            (chunk '{' '}')
            (chunk '<' '>')
        |])
        |>> (fun _ -> ())
        let chunks = chunks .>> eof
        match run chunks str with
        | Success _ -> Result.Ok str
        | Failure (_, error, _) ->
            let position = error.Position
            match error.Messages.Head with
            | ExpectedString s when position.Column > String.length str -> recognize $"{str}{s}"
            | _ -> Result.Error str[int position.Column - 1]

open System.CommandLine
open System.IO

open FSharpPlus

type Options = {
    Input: FileInfo
}

let corruptedScores = Map.ofList [
    (')', 3)
    (']', 57)
    ('}', 1197)
    ('>', 25137)
]

let incompleteScores = Map.ofList [
    (')', 1L)
    (']', 2L)
    ('}', 3L)
    ('>', 4L)
]

let calculateIncompleteScore =
    Seq.fold (fun total ch ->
        let score = Map.tryFind ch incompleteScores |> Option.defaultValue 0
        total * 5L + score
    ) 0L

let run (options: Options) (console: IConsole) =
    task {
        use file = options.Input.OpenRead ()
        use reader = new StreamReader (file)
        let! contents = reader.ReadToEndAsync ()

        let lines = String.split [ "\n" ] contents |> Seq.map (fun s -> s, Syntax.recognize s)

        let syntaxErrorScore =
            lines
            |> Seq.choose (function
                | _, Ok _ -> None
                | _, Error error -> Map.tryFind error corruptedScores
            )
            |> Seq.sum

        printfn $"Total syntax error score: {syntaxErrorScore}"

        let autocompletionScores =
            lines
            |> Seq.choose (function
                | str, Ok autoStr when (String.length str) <> (String.length autoStr) ->
                    Some (calculateIncompleteScore autoStr[(String.length str)..])
                | _ -> None)
            |> Seq.sort
            |> Array.ofSeq

        let winningScore = autocompletionScores[Array.length autocompletionScores / 2]

        printfn $"Winning autocompletion score: {winningScore}"

        return 0
    }

let command = Command.create "day10" "Syntax Scoring" run
