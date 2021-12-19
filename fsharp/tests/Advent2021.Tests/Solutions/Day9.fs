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
            let expectedOutput = Ok ([ { Row = 0; Column = 0; Value = 1uy } ])
            let output = monad' {
                let! hm = HeightMap.parse "1\n2"
                let result = hm |> HeightMap.filter (fun pt _ -> pt.Value = 1uy)
                return List.ofSeq result
            }
            Expect.equal output expectedOutput "the filter results match"
        }
        test "it includes the neighbors" {
            let expectedOutput = Ok ([ { Row = 1; Column = 1; Value = 1uy } ])
            let output = monad' {
                let! hm = HeightMap.parse "111\n111\n111"
                let result = hm |> HeightMap.filter (fun pt neighbors ->
                    match neighbors with
                    | {
                        Top = Some top; Bottom = Some bottom; Left = Some left; Right = Some right
                      } ->
                        top.Value = 1uy && bottom.Value = 1uy && left.Value = 1uy
                        && right.Value = 1uy && pt.Value = 1uy
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
                { Row = 1; Column = 2; Value = 8uy }
                { Row = 1; Column = 3; Value = 7uy }
                { Row = 1; Column = 4; Value = 8uy }
                { Row = 2; Column = 1; Value = 8uy }
                { Row = 2; Column = 2; Value = 5uy }
                { Row = 2; Column = 3; Value = 6uy }
                { Row = 2; Column = 4; Value = 7uy }
                { Row = 2; Column = 5; Value = 8uy }
                { Row = 3; Column = 0; Value = 8uy }
                { Row = 3; Column = 1; Value = 7uy }
                { Row = 3; Column = 2; Value = 6uy }
                { Row = 3; Column = 3; Value = 7uy }
                { Row = 3; Column = 4; Value = 8uy }
                { Row = 4; Column = 1; Value = 8uy }
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
