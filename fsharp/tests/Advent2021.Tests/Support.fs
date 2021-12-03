// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Support

open System

open Expecto
open FsCheck
open FSharpx.Text

[<Struct>]
type NonNumericString = NonNumericString of string with
    static member op_Explicit (NonNumericString s) = s

type ArbitraryStrings =
    static member NonNumericString () =
        let lacksNumbers s =
            not (String.IsNullOrEmpty s || String.IsNullOrWhiteSpace s)
            && Regex.tryMatchWithOptions Regex.Options.Singleline "\d" s |> Option.isNone
        Arb.from<string>
        |> Arb.filter lacksNumbers
        |> Arb.convert NonNumericString string

let private config = {
    FsCheckConfig.defaultConfig with arbitrary = [ typeof<ArbitraryStrings> ]
}

let testProperty s = testPropertyWithConfig config s
