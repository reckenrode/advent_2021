// SPDX-License-Identifier: GPL-3.0-only

namespace Advent2021.Solutions

open System
open System.CommandLine
open System.CommandLine.NamingConventionBinder
open System.IO
open System.Reflection
open System.Threading.Tasks

open FSharp.Control
open FSharpPlus
open FSharpx.Text

[<AutoOpen>]
module Support =
    let register (root: RootCommand) =
        let getDayNumber s =
            monad' {
                let! matchResult = Regex.tryMatch @"[A-Za-z](\d+)" s
                let! day = matchResult.GroupValues |> List.tryItem 0
                return! Option.ofPair (Int32.TryParse day)
            }
            |> Option.defaultValue -1

        let getCommand (t: Type) =
            monad' {
                let! props = t.GetProperties () |> Option.ofObj

                let! cmd =
                    props
                    |> Seq.filter (fun (p: PropertyInfo) ->
                        p.Name = "command"
                        && p.PropertyType = typeof<Command>)
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

    let rec lines (reader: TextReader) =
        asyncSeq {
            match! Async.AwaitTask (reader.ReadLineAsync ()) with
            | null -> ()
            | line ->
                yield line
                yield! lines reader
        }

    module Command =
        let create name description (handler: 'a -> IConsole -> Task<int>) =
            let cmd =
                Command (name, description, Handler = CommandHandler.Create handler)

            cmd.AddOption
            <| Option<FileInfo> (
                aliases = [| "-i"; "--input" |],
                description = "the day’s input file",
                IsRequired = true
            )

            cmd
