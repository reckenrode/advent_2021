// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day2

open System.CommandLine
open System.IO

open FSharpPlus
open FSharpPlus.Data
open FSharpPlus.Lens

type State = {
    distance: int
    depth: int
    aim: int
}

module State =
    let ``default`` = { distance = 0; depth = 0; aim = 0; }
    let inline _distance f p = f p.distance <&> fun d -> { p with distance =d }
    let inline _depth f p = f p.depth <&> fun d -> { p with depth = d }
    let inline _aim f p = f p.aim <&> fun a -> { p with aim = a }

type Program = private Program of State<State, unit>

module Program =
    let private aimlessMapping = Map.ofList [
        ("forward", fun x -> over State._distance ((+) x));
        ("down", fun x -> over State._depth ((+) x));
        ("up", fun x -> over State._depth ((+) -x))
    ]

    let private aimfulMapping = Map.ofList [
        ("forward", fun x state ->
            (over State._distance ((+) x) state) |>
            (over State._depth ((+) (x * view State._aim state)))
        );
        ("down", fun x -> over State._aim ((+) x));
        ("up", fun x -> over State._aim ((+) -x))
    ]

    let private tryParseCommand mapping line =
        monad' {
            let! line = Option.ofObj line
            match trySscanf "%s %d" line with
            | Some (d, x) when mapping |> Map.containsKey d ->
                Ok (State.get |>> ((mapping |> Map.find d) x) >>= State.put)
            | _ ->
                return Error $"invalid input: {line}"
        }

    let private aimlessParser = tryParseCommand aimlessMapping
    let private aimfulParser = tryParseCommand aimfulMapping

    let parse reader useAim =
        let parser = if useAim then aimfulParser else aimlessParser
        let rec parse' (reader: TextReader) xs =
            async {
                let! line = reader.ReadLineAsync () |> Async.AwaitTask
                match parser line with
                | Some (Ok result) ->
                    return! parse' reader (result::xs)
                | Some error ->
                    return error
                | None ->
                    return Ok (xs |> rev |> List.reduce (fun a b -> a *> b))
            }
        task {
            let! result = parse' reader []
            return result |> map Program
        }

    let eval (Program program) = State.exec program State.``default``

type Options = {
    Input: FileInfo
    MultiplyResult: bool
    UseAim: bool
}

let run (options: Options) (console: IConsole) =
    task {
        use stream = options.Input.OpenRead ()
        use reader = new StreamReader (stream)
        let! program = Program.parse reader options.UseAim
        match program with
        | Ok program ->
            let result = Program.eval program
            console.Out.Write $"Distance:\t{result.distance}\nDepth:\t\t{result.depth}\n"
            if options.MultiplyResult
            then console.Out.Write $"Multiplied together: {result.distance * result.depth}\n"
            return 0
        | Error error ->
            console.Error.Write $"Something went wrong while solving the problem: {error}\n"
            return -1
    }

let command =
    let command = Command.create "day2" "Dive!" run
    command.AddOption <| Option<bool> (
        aliases = [| "-m"; "--multiply-result" |],
        description = "multiply the depth and distance together"
    )
    command.AddOption <| Option<bool> (
        aliases = [| "-u"; "--use-aim" |],
        description = "move using the submarine’s aim"
    )
    command
