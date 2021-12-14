// SPDX-License-Identifier: GPL-3.0-only

namespace Advent2021.Solutions

open System
open System.CommandLine
open System.CommandLine.Invocation
open System.IO
open System.Reflection
open System.Threading.Tasks

open FSharpx.Option
open FSharpx.Text

[<AutoOpen>]
module Support =
    let register (root: RootCommand) =
        let getDayNumber s =
            maybe {
                let! matchResult = Regex.tryMatch @"[A-Za-z](\d+)" s
                let! day = matchResult.GroupValues |> List.tryItem 0
                return! ofBoolAndValue (Int32.TryParse day)
            }
            |> Option.defaultValue -1

        let getCommand (t: Type) =
            maybe {
                let! props = t.GetProperties () |> Option.ofObj
                let! cmd =
                    props
                    |> Seq.filter (fun p -> p.Name = "command" && p.PropertyType = typeof<Command>)
                    |> Seq.tryExactlyOne
                return downcast cmd.GetValue null
            }

        let assembly = Assembly.GetExecutingAssembly ()
        let commands =
            assembly.GetTypes ()
            |> Array.choose getCommand
            |> Array.sortBy (fun (cmd: Command) -> getDayNumber cmd.Name)
        commands |> Array.iter root.AddCommand
        root

    let handleFailure (console: IConsole) =
        function
        | Ok code -> code
        | Error message ->
            console.Error.Write $"Error parsing file: {message}"
            1

    module Command =
        let create name description (handler: 'a -> IConsole -> Task<int>) =
            let cmd = Command (
                name,
                description,
                Handler = CommandHandler.Create handler
            )
            cmd.AddOption <| Option<FileInfo> (
                aliases = [| "-i"; "--input" |],
                description = "the day’s input file",
                IsRequired = true
            )
            cmd
