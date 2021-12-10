// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day10

module Syntax =
    open FParsec

    let recognize str =
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
        | Success _ -> Result.Ok ()
        | Failure (msg, error, _) ->
            let position = error.Position
            if position.Column > String.length str
            then Result.Ok ()
            else Result.Error str[int position.Column - 1]

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
    (')', 1)
    (']', 2)
    ('}', 3)
    ('>', 4)
]

let run (options: Options) (console: IConsole) =
    task {
        use file = options.Input.OpenRead ()
        use reader = new StreamReader (file)
        let! contents = reader.ReadToEndAsync ()

        let lines = String.split [ "\n" ] contents

        let syntaxErrorScore =
            lines
            |> Seq.map Syntax.recognize
            |> Seq.choose (function
                | Ok _ -> None
                | Error error -> Map.tryFind error corruptedScores
            )
            |> Seq.sum

        printfn $"Total syntax error score: {syntaxErrorScore}"

        return 0
    }

let command = Command.create "day10" "Syntax Scoring" run
