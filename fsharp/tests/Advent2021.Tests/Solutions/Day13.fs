// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day13

open Expecto

open Advent2021.Solutions.Day13

[<Tests>]
let tests = testList "Day13" [
    testList "horizontal reflections" [
        test "it flips the points up" {
            let expectedPoints = Points.ofList [
                { x = 9; y = 4 }
            ]
            let startingPoints = Points.ofList [
                { x = 9; y = 10 }
            ]
            let line = 7
            let points = Points.reflectHorizontal line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it flips only the points below the line" {
            let expectedPoints = Points.ofList [
                { x = 6; y = 0 }
                { x = 9; y = 0 }
            ]
            let startingPoints = Points.ofList [
                { x = 6; y = 0 }
                { x = 9; y = 14 }
            ]
            let line = 7
            let points = Points.reflectHorizontal line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it merges duplicate points" {
            let expectedPoints = Points.ofList [
                { x = 6; y = 0 }
            ]
            let startingPoints = Points.ofList [
                { x = 6; y = 0 }
                { x = 6; y = 14 }
            ]
            let line = 7
            let points = Points.reflectHorizontal line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }
    ]

    testList "vertical reflections" [
        test "it flips the points left" {
            let expectedPoints = Points.ofList [
                { x = 5; y = 10 }
            ]
            let startingPoints = Points.ofList [
                { x = 9; y = 10 }
            ]
            let line = 7
            let points = Points.reflectVertical line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it flips only the points to the right of the line" {
            let expectedPoints = Points.ofList [
                { x = 6; y = 0 }
                { x = 5; y = 14 }
            ]
            let startingPoints = Points.ofList [
                { x = 6; y = 0 }
                { x = 9; y = 14 }
            ]
            let line = 7
            let points = Points.reflectVertical line startingPoints
            Expect.equal points expectedPoints "the point grids match"
        }

        test "it merges duplicate points" {
            let expectedPoints = Points.ofList [
                { x = 6; y = 0 }
            ]
            let startingPoints = Points.ofList [
                { x = 6; y = 0 }
                { x = 8; y = 0 }
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
                    { x = 6; y = 10 }
                    { x = 0; y = 14 }
                    { x = 9; y = 10 }
                    { x = 0; y = 3 }
                    { x = 10; y = 4 }
                    { x = 4; y = 11 }
                    { x = 6; y = 0 }
                    { x = 6; y = 12 }
                    { x = 4; y = 1 }
                    { x = 0; y = 13 }
                    { x = 10; y = 12 }
                    { x = 3; y = 4 }
                    { x = 3; y = 0 }
                    { x = 8; y = 4 }
                    { x = 1; y = 10 }
                    { x = 2; y = 14 }
                    { x = 8; y = 10 }
                    { x = 9; y = 0 }
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
            let input = Points.ofList [ { x = 0; y = 0 }]
            let output = Points.render input
            Expect.equal output expectedOutput "the outputs match"
        }

        test "it renders empty points as a dot" {
            let expectedOutput = ".#"
            let input = Points.ofList [ { x = 1; y = 0 }]
            let output = Points.render input
            Expect.equal output expectedOutput "the outputs match"
        }
    ]
]
