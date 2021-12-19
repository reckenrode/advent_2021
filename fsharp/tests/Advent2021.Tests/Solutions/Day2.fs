// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day2

open System.IO

open Expecto
open FSharpPlus

open Advent2021.Solutions.Day2

[<Tests>]
let tests = testList "Day2" [
    let inputStream () =
        new StringReader "forward 5\ndown 5\nforward 8\nup 3\ndown 8\nforward 2\n"

    let example number expected useAim =
        testTask $"example {number} has distance {expected.Distance} and depth {expected.Depth}" {
            use input = inputStream ()
            let! program = Program.parse input useAim
            Expect.isOk program "program parsed okay"
            let result = Program.eval (Result.get program)
            Expect.equal result expected "the product matches"
        }

    example 1 { Distance = 15; Depth = 10; Aim = 0 } false
    example 2 { Distance = 15; Depth = 60; Aim = 10 } true
]
