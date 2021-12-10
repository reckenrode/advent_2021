// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day1

open Expecto

open Advent2021.Solutions.Day1

[<Tests>]
let tests = testList "Day1" [
    testList "examples" [
        test "example 1 has seven increasing measurements" {
            let expectedMeasurements = 7
            let input = [199; 200; 208; 210; 200; 207; 240; 269; 260; 263]
            let result = countIncreases 1 input
            Expect.equal result expectedMeasurements "the measurements match"
        }
        test "example 2 has five increasing measurements" {
            let expectedMeasurements = 5
            let input = [199; 200; 208; 210; 200; 207; 240; 269; 260; 263]
            let result = countIncreases 3 input
            Expect.equal result expectedMeasurements "the measurements match"
        }
    ]
]
