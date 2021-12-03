// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Cli

open System.CommandLine
open System.CommandLine.Builder
open System.CommandLine.Parsing
open System.Reflection

open Advent2021.Solutions.Support

let private builder =
    let assembly = Assembly.GetEntryAssembly ()
    let attribute = assembly.GetCustomAttribute typeof<AssemblyDescriptionAttribute>
    let description = (attribute :?> AssemblyDescriptionAttribute).Description
    let root = RootCommand (description = description, Name = "Advent2021")
    register root

let run (args: array<string>) =
    task {
        let builder = (CommandLineBuilder builder).UseDefaults ()
        let parser = builder.Build ()
        return! parser.InvokeAsync args
    }
