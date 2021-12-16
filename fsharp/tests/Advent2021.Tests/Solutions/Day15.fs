// SPDX,License,Identifier: GPL,3.0,only

module Advent2021.Tests.Solutions.Day15

open Expecto
open FSharpPlus

open Advent2021.Solutions.Day15

[<Tests>]
let tests = testList "Day15" [
    testList "shortest path cost" [
        test "it includes the source" {
            let expectedCost = 0u
            let input =
                Graph.ofList [
                    [ 42u ]
                ]
                |> Result.get
            let cost = Graph.shortestPathCost (0, 0) (0, 0) input
            Expect.equal cost expectedCost "the cost is minimized"
        }

        test "it finds the cost in a 2x2 graph" {
            let expectedCost = 7u
            let input =
                Graph.ofList [
                    [ 1u; 2u ]
                    [ 3u; 5u ]
                ]
                |> Result.get
            let cost = Graph.shortestPathCost (0, 0) (1, 1) input
            Expect.equal cost expectedCost "the least cost path is found"
        }

        test "it finds the cost of the example graph" {
            let expectedCost = 40u
            let input =
                Graph.ofList [
                    [ 1u; 1u; 6u; 3u; 7u; 5u; 1u; 7u; 4u; 2u ]
                    [ 1u; 3u; 8u; 1u; 3u; 7u; 3u; 6u; 7u; 2u ]
                    [ 2u; 1u; 3u; 6u; 5u; 1u; 1u; 3u; 2u; 8u ]
                    [ 3u; 6u; 9u; 4u; 9u; 3u; 1u; 5u; 6u; 9u ]
                    [ 7u; 4u; 6u; 3u; 4u; 1u; 7u; 1u; 1u; 1u ]
                    [ 1u; 3u; 1u; 9u; 1u; 2u; 8u; 1u; 3u; 7u ]
                    [ 1u; 3u; 5u; 9u; 9u; 1u; 2u; 4u; 2u; 1u ]
                    [ 3u; 1u; 2u; 5u; 4u; 2u; 1u; 6u; 3u; 9u ]
                    [ 1u; 2u; 9u; 3u; 1u; 3u; 8u; 5u; 2u; 1u ]
                    [ 2u; 3u; 1u; 1u; 9u; 4u; 4u; 5u; 8u; 1u ]
                ]
                |> Result.get
            let cost = Graph.shortestPathCost (0, 0) (9, 9) input
            Expect.equal cost expectedCost "the least cost path is found"
        }

        test "it finds the cost in a windy graph" {
            let expectedCost = 22u
            let input =
                Graph.ofList [
                    [ 9u; 1u; 1u; 1u ]
                    [ 9u; 9u; 2u; 1u ]
                    [ 1u; 1u; 3u; 9u ]
                    [ 1u; 9u; 9u; 9u ]
                    [ 1u; 1u; 1u; 9u ]
                ]
                |> Result.get
            let cost = Graph.shortestPathCost (0, 0) (4, 3) input
            Expect.equal cost expectedCost "the least cost path is found"
        }
    ]

    testList "graph resizing" [
        test "it increases the values" {
            let expectedGraph =
                Graph.ofList [
                    [ 1u; 2u; 3u ]
                    [ 2u; 3u; 4u ]
                    [ 3u; 4u; 5u ]
                ]
                |> Result.get
            let graph =
                Graph.ofList [[ 1u ]]
                |> Result.get
                |> Graph.grow (2, 2)

            Expect.equal graph expectedGraph "the graphs match"
        }

        test "it works with multiple lines and columns" {
            let expectedGraph =
                Graph.ofList [
                    [ 1u; 1u; 2u; 2u; 3u; 3u; 4u; 4u; 5u; 5u ]
                    [ 1u; 1u; 2u; 2u; 3u; 3u; 4u; 4u; 5u; 5u ]
                    [ 2u; 2u; 3u; 3u; 4u; 4u; 5u; 5u; 6u; 6u ]
                    [ 2u; 2u; 3u; 3u; 4u; 4u; 5u; 5u; 6u; 6u ]
                    [ 3u; 3u; 4u; 4u; 5u; 5u; 6u; 6u; 7u; 7u ]
                    [ 3u; 3u; 4u; 4u; 5u; 5u; 6u; 6u; 7u; 7u ]
                    [ 4u; 4u; 5u; 5u; 6u; 6u; 7u; 7u; 8u; 8u ]
                    [ 4u; 4u; 5u; 5u; 6u; 6u; 7u; 7u; 8u; 8u ]
                    [ 5u; 5u; 6u; 6u; 7u; 7u; 8u; 8u; 9u; 9u ]
                    [ 5u; 5u; 6u; 6u; 7u; 7u; 8u; 8u; 9u; 9u ]
                ]
                |> Result.get
            let graph =
                Graph.ofList [
                    [ 1u; 1u ]
                    [ 1u; 1u ]
                ]
                |> Result.get
                |> Graph.grow (4, 4)

            Expect.equal graph expectedGraph "the graphs match"
        }

        test "it wraps the risk levels" {
            let expectedGraph =
                Graph.ofList [
                    [ 9u; 1u ]
                    [ 1u; 2u ]
                ]
                |> Result.get
            let graph =
                Graph.ofList [
                    [ 9u ]
                ]
                |> Result.get
                |> Graph.grow (1, 1)

            Expect.equal graph expectedGraph "the graphs match"
        }
    ]
]
