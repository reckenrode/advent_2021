// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day9

open Expecto
open FSharpPlus

open Advent2021.Solutions.Day9

[<Tests>]
let tests = testList "Day9.Heightmap" [
    testList "parsing" [
        test "it parses the string" {
            let expectedHeightmap = Ok (
                HeightMap (array2D [
                    [ 1uy; 2uy; 3uy; 4uy; 5uy ];
                    [ 6uy; 7uy; 8uy; 9uy; 0uy ]
                ])
            )
            let heightmap = HeightMap.parse "12345\n67890\n"
            Expect.equal heightmap expectedHeightmap "the height maps match"
        }
    ]
    testList "filter" [
        test "it applies the filter" {
            let expectedOutput = Ok ([ { row = 0; column = 0; value = 1uy } ])
            let output = monad' {
                let! hm = HeightMap.parse "1\n2"
                let result = hm |> HeightMap.filter (fun pt _ -> pt.value = 1uy)
                return List.ofSeq result
            }
            Expect.equal output expectedOutput "the filter results match"
        }
        test "it includes the neighbors" {
            let expectedOutput = Ok ([ { row = 1; column = 1; value = 1uy } ])
            let output = monad' {
                let! hm = HeightMap.parse "111\n111\n111"
                let result = hm |> HeightMap.filter (fun pt neighbors ->
                    match neighbors with
                    | {
                        top = Some top; bottom = Some bottom; left = Some left; right = Some right
                      } ->
                        top.value = 1uy && bottom.value = 1uy && left.value = 1uy
                        && right.value = 1uy && pt.value = 1uy
                    | _ -> false
                )
                return List.ofSeq result
            }
            Expect.equal output expectedOutput "the filter results match"
        }
    ]
    testList "mapBasin" [
        test "it maps the entire basin from the lowest point" {
            let expectedBasin = Ok (Set.ofList [
                { row = 1; column = 2; value = 8uy }
                { row = 1; column = 3; value = 7uy }
                { row = 1; column = 4; value = 8uy }
                { row = 2; column = 1; value = 8uy }
                { row = 2; column = 2; value = 5uy }
                { row = 2; column = 3; value = 6uy }
                { row = 2; column = 4; value = 7uy }
                { row = 2; column = 5; value = 8uy }
                { row = 3; column = 0; value = 8uy }
                { row = 3; column = 1; value = 7uy }
                { row = 3; column = 2; value = 6uy }
                { row = 3; column = 3; value = 7uy }
                { row = 3; column = 4; value = 8uy }
                { row = 4; column = 1; value = 8uy }
            ])
            let basin = monad' {
                let! hm =
                    HeightMap.parse (
                        "2199943210\n" +
                        "3987894921\n" +
                        "9856789892\n" +
                        "8767896789\n" +
                        "9899965678"
                    )
                return HeightMap.mapBasin (2, 2) hm
            }
            Expect.equal basin expectedBasin "the basins match"
        }
    ]
]
