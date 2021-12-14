// SPDX,License,Identifier: GPL,3.0,only

module Advent2021.Tests.Solutions.Day14

open Expecto

open Advent2021.Solutions.Day14

[<Tests>]
let tests = testList "Day14" [
    testList "polymerization" [
        test "it inserts the pair after the match" {
            let expectedOutput = "NCN"
            let rules = InsertionRules.ofList [
                [| 'N'; 'N' |], 'C'
            ]
            let output = InsertionRules.apply "NN" rules
            Expect.equal output expectedOutput "the polymerization was applied"
        }

        test "it only considers the original string" {
            let expectedOutput = "NCNBCHB"
            let rules = InsertionRules.ofList [
                [| 'N'; 'N' |], 'C'
                [| 'N'; 'C' |], 'B'
                [| 'C'; 'B' |], 'H'
            ]
            let output = InsertionRules.apply "NNCB" rules
            Expect.equal output expectedOutput "the polymerization was applied"
        }
    ]

    testList "parsing" [
        test "it parses the input" {
            let expectedInsertionRules = InsertionRules.ofList [
                [| 'B'; 'B' |], 'N'
                [| 'B'; 'C' |], 'B'
                [| 'B'; 'H' |], 'H'
                [| 'B'; 'N' |], 'B'
                [| 'C'; 'B' |], 'H'
                [| 'C'; 'C' |], 'N'
                [| 'C'; 'H' |], 'B'
                [| 'C'; 'N' |], 'C'
                [| 'H'; 'B' |], 'C'
                [| 'H'; 'C' |], 'B'
                [| 'H'; 'H' |], 'N'
                [| 'H'; 'N' |], 'C'
                [| 'N'; 'B' |], 'B'
                [| 'N'; 'C' |], 'B'
                [| 'N'; 'H' |], 'C'
                [| 'N'; 'N' |], 'C'
            ]
            let expectedTemplate = "NNCB"
            let expectedOutput = Ok (expectedTemplate, expectedInsertionRules)
            let input =
                "NNCB\n\
                \n\
                CH -> B\n\
                HH -> N\n\
                CB -> H\n\
                NH -> C\n\
                HB -> C\n\
                HC -> B\n\
                HN -> C\n\
                NN -> C\n\
                BH -> H\n\
                NC -> B\n\
                NB -> B\n\
                BN -> B\n\
                BB -> N\n\
                BC -> B\n\
                CC -> N\n\
                CN -> C"
            let output = Parser.parse input
            Expect.equal output expectedOutput "the file was parsed"
        }
    ]
]
