// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day13

open Expecto

open Advent2021.Solutions.Day13

[<Tests>]
let tests = testList "Day13" [
    testList "horizontal reflections" [
        test "it flips the points up" {
            let expectedPoints = Points.ofList [
                { X = 9; Y = 4 }
            ]
            let startingPoints = Points.ofList [
                { X = 9; Y = 10 }
            ]
            let line = 7
            let points = Points.reflectHorizontal line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it flips only the points below the line" {
            let expectedPoints = Points.ofList [
                { X = 6; Y = 0 }
                { X = 9; Y = 0 }
            ]
            let startingPoints = Points.ofList [
                { X = 6; Y = 0 }
                { X = 9; Y = 14 }
            ]
            let line = 7
            let points = Points.reflectHorizontal line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it merges duplicate points" {
            let expectedPoints = Points.ofList [
                { X = 6; Y = 0 }
            ]
            let startingPoints = Points.ofList [
                { X = 6; Y = 0 }
                { X = 6; Y = 14 }
            ]
            let line = 7
            let points = Points.reflectHorizontal line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }
    ]

    testList "vertical reflections" [
        test "it flips the points left" {
            let expectedPoints = Points.ofList [
                { X = 5; Y = 10 }
            ]
            let startingPoints = Points.ofList [
                { X = 9; Y = 10 }
            ]
            let line = 7
            let points = Points.reflectVertical line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it flips only the points to the right of the line" {
            let expectedPoints = Points.ofList [
                { X = 6; Y = 0 }
                { X = 5; Y = 14 }
            ]
            let startingPoints = Points.ofList [
                { X = 6; Y = 0 }
                { X = 9; Y = 14 }
            ]
            let line = 7
            let points = Points.reflectVertical line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it merges duplicate points" {
            let expectedPoints = Points.ofList [
                { X = 6; Y = 0 }
            ]
            let startingPoints = Points.ofList [
                { X = 6; Y = 0 }
                { X = 8; Y = 0 }
            ]
            let line = 7
            let points = Points.reflectVertical line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }
    ]

    testList "parsing" [
        test "it parses the instructions" {
            let expectedInstructions = Ok (
                Points.ofList [
                    { X = 6; Y = 10 }
                    { X = 0; Y = 14 }
                    { X = 9; Y = 10 }
                    { X = 0; Y = 3 }
                    { X = 10; Y = 4 }
                    { X = 4; Y = 11 }
                    { X = 6; Y = 0 }
                    { X = 6; Y = 12 }
                    { X = 4; Y = 1 }
                    { X = 0; Y = 13 }
                    { X = 10; Y = 12 }
                    { X = 3; Y = 4 }
                    { X = 3; Y = 0 }
                    { X = 8; Y = 4 }
                    { X = 1; Y = 10 }
                    { X = 2; Y = 14 }
                    { X = 8; Y = 10 }
                    { X = 9; Y = 0 }
                ],
                [
                    Instructions.Horizontal 7
                    Instructions.Vertical 5
                ]
            )
            let input =
                "6,10\n\
                0,14\n\
                9,10\n\
                0,3\n\
                10,4\n\
                4,11\n\
                6,0\n\
                6,12\n\
                4,1\n\
                0,13\n\
                10,12\n\
                3,4\n\
                3,0\n\
                8,4\n\
                1,10\n\
                2,14\n\
                8,10\n\
                9,0\n\
                \n\
                fold along y=7\n\
                fold along x=5\n"
            let instructions = Instructions.parse input
            Expect.equal instructions expectedInstructions "the instructions parsed correctly"
        }
    ]

    testList "rendering" [
        test "it renders a point as a hash" {
            let expectedOutput = "#"
            let input = Points.ofList [ { X = 0; Y = 0 }]
            let output = Points.render input
            Expect.equal output expectedOutput "the outputs match"
        }

        test "it renders empty points as a dot" {
            let expectedOutput = ".#"
            let input = Points.ofList [ { X = 1; Y = 0 }]
            let output = Points.render input
            Expect.equal output expectedOutput "the outputs match"
        }
    ]
]
