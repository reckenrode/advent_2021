// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day10

open Expecto

open Advent2021.Solutions.Day10

[<Tests>]
let tests = testList "Day10.Syntax" [
    testList "chunks" [
        test "it recognizes paren chunks" {
            let expected = Ok ()
            let result = Syntax.recognize "()"
            Expect.equal result expected "the chunk was recognized"
        }

        test "it recognizes bracket chunks" {
            let expected = Ok ()
            let result = Syntax.recognize "[]"
            Expect.equal result expected "the chunk was recognized"
        }

        test "it recognizes curly bracket chunks" {
            let expected = Ok ()
            let result = Syntax.recognize "{}"
            Expect.equal result expected "the chunk was recognized"
        }

        test "it recognizes angle bracket chunks" {
            let expected = Ok ()
            let result = Syntax.recognize "<>"
            Expect.equal result expected "the chunk was recognized"
        }

        test "it recognizes nested chunks" {
            let expected = Ok ()
            let result = Syntax.recognize "(())"
            Expect.equal result expected "the chunk was recognized"
        }

        test "it recognizes complex chunks" {
            let expected = Ok ()
            let result = Syntax.recognize "[<>({}){}[([])<>]]"
            Expect.equal result expected "the chunk was recognized"
        }

        test "it autocompletes incomplete lines" {
            let expected = Ok ()
            let result = Syntax.recognize "[({(<(())[]>[[{[]{<()<>>"
            Expect.equal result expected "the line was ignored"
        }
    ]
    testList "invalid chunks" [
        test "it identifies which character was found" {
            let expected = Error '>'
            let result = Syntax.recognize "<{([([[(<>()){}]>(<<{{"
            Expect.equal result expected "it found the unexpected character"
        }
    ]
]
