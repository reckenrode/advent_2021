// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day18

open Expecto
open FSharpPlus

open Advent2021.Solutions.Day18

[<Tests>]
let test = testList "Day18" [
    testList "addition" [
        test "it forms a pair" {
            let expectedResult = SnailfishNumber.tryParse "[[1,2],[[3,4],5]]"
            let result =
                monad' {
                    let! lhs = SnailfishNumber.tryParse "[1,2]"
                    let! rhs = SnailfishNumber.tryParse "[[3,4],5]"
                    return lhs + rhs
                }
            Expect.equal result expectedResult "[1,2] + [[3,4],5] = [[1,2],[[3,4],5]]"
        }

        let cases = [
            "[[1,2],[[3,4],5]]"
            "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]"
        ]

        yield!
            cases
            |> List.map (fun input ->
                test $"it returns when adding {input} to zero" {
                    let expectedResult = SnailfishNumber.tryParse input
                    let result1 =
                        monad' {
                            let! input = SnailfishNumber.tryParse input
                            return zero + input
                        }
                    let result2 =
                        monad' {
                            let! input = SnailfishNumber.tryParse input
                            return input + zero
                        }
                    Expect.equal result1 expectedResult $"zero + {input} = {input}"
                    Expect.equal result2 expectedResult $"{input} + zero = {input}"
                })

        test "it reduces the result" {
            let expectedResult = SnailfishNumber.tryParse "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]"
            let result =
                monad' {
                    let! lhs = SnailfishNumber.tryParse "[[[[4,3],4],4],[7,[[8,4],9]]]"
                    let! rhs = SnailfishNumber.tryParse "[1,1]"
                    return lhs + rhs
                }
            Expect.equal result expectedResult
                "[[[[4,3],4],4],[7,[[8,4],9]]] + [1,1] = [[[[0,7],4],[[7,8],[6,0]]],[8,1]]"
        }

        let cases = [
            "[[[0,[4,5]],[0,0]],[[[4,5],[2,6]],[9,5]]]", "[7,[[[3,7],[4,3]],[[6,3],[8,8]]]]", "[[[[4,0],[5,4]],[[7,7],[6,0]]],[[8,[7,7]],[[7,9],[5,0]]]]"
            "[[[[4,0],[5,4]],[[7,7],[6,0]]],[[8,[7,7]],[[7,9],[5,0]]]]", "[[2,[[0,8],[3,4]]],[[[6,7],1],[7,[1,6]]]]", "[[[[6,7],[6,7]],[[7,7],[0,7]]],[[[8,7],[7,7]],[[8,8],[8,0]]]]"
            "[[[[6,7],[6,7]],[[7,7],[0,7]]],[[[8,7],[7,7]],[[8,8],[8,0]]]]", "[[[[2,4],7],[6,[0,5]]],[[[6,8],[2,8]],[[2,1],[4,5]]]]", "[[[[7,0],[7,7]],[[7,7],[7,8]]],[[[7,7],[8,8]],[[7,7],[8,7]]]]"
            "[[[[7,0],[7,7]],[[7,7],[7,8]]],[[[7,7],[8,8]],[[7,7],[8,7]]]]", "[7,[5,[[3,8],[1,4]]]]", "[[[[7,7],[7,8]],[[9,5],[8,7]]],[[[6,8],[0,8]],[[9,9],[9,0]]]]"
            "[[[[7,7],[7,8]],[[9,5],[8,7]]],[[[6,8],[0,8]],[[9,9],[9,0]]]]", "[[2,[2,2]],[8,[8,1]]]", "[[[[6,6],[6,6]],[[6,0],[6,7]]],[[[7,7],[8,9]],[8,[8,1]]]]"
            "[[[[6,6],[6,6]],[[6,0],[6,7]]],[[[7,7],[8,9]],[8,[8,1]]]]", "[2,9]", "[[[[6,6],[7,7]],[[0,7],[7,7]]],[[[5,5],[5,6]],9]]"
            "[[[[6,6],[7,7]],[[0,7],[7,7]]],[[[5,5],[5,6]],9]]", "[1,[[[9,3],9],[[9,0],[0,7]]]]", "[[[[7,8],[6,7]],[[6,8],[0,8]]],[[[7,7],[5,0]],[[5,5],[5,6]]]]"
            "[[[[7,8],[6,7]],[[6,8],[0,8]]],[[[7,7],[5,0]],[[5,5],[5,6]]]]", "[[[5,[7,4]],7],1]", "[[[[7,7],[7,7]],[[8,7],[8,7]]],[[[7,0],[7,7]],9]]"
            "[[[[7,7],[7,7]],[[8,7],[8,7]]],[[[7,0],[7,7]],9]]", "[[[[4,2],2],6],[8,7]]", "[[[[8,7],[7,7]],[[8,6],[7,7]]],[[[0,7],[6,6]],[8,7]]]"
        ]

        yield!
            cases
            |> List.map (fun (lhs, rhs, expectedNumber) ->
                test $"it adds {lhs} to {rhs}" {
                    let expectedResult = SnailfishNumber.tryParse expectedNumber
                    let result =
                        monad' {
                            let! lhs = SnailfishNumber.tryParse lhs
                            let! rhs = SnailfishNumber.tryParse rhs
                            return lhs + rhs
                        }
                    Expect.equal result expectedResult $"{lhs} + {rhs} = {expectedNumber}"
                })
    ]

    testList "magnitude" [
        test "it calculates the magnitude of a pair" {
            let expectedResult = Some 29UL
            let magnitude =
                monad' {
                    let! number = SnailfishNumber.tryParse "[9,1]"
                    return SnailfishNumber.magnitude number
                }
            Expect.equal magnitude expectedResult "magnitude of [9,1] = 29"
        }

        let cases = [
            "[[1,2],[[3,4],5]]", 143UL
            "[[[[0,7],4],[[7,8],[6,0]]],[8,1]]", 1384UL
            "[[[[1,1],[2,2]],[3,3]],[4,4]]", 445UL
            "[[[[3,0],[5,3]],[4,4]],[5,5]]", 791UL
            "[[[[5,0],[7,4]],[5,5]],[6,6]]", 1137UL
            "[[[[8,7],[7,7]],[[8,6],[7,7]]],[[[0,7],[6,6]],[8,7]]]", 3488UL
            "[[[[6,6],[7,6]],[[7,7],[7,0]]],[[[7,7],[7,7]],[[7,8],[9,9]]]]", 4140UL
        ]

        yield!
            cases
            |> List.map (fun (input, expectedMagnitude) ->
                test $"it calculates the magnitude of {input}" {
                    let expectedResult = Some expectedMagnitude
                    let result = monad' {
                        let! number = SnailfishNumber.tryParse input
                        return SnailfishNumber.magnitude number
                    }
                    Expect.equal result expectedResult $"magnitude of {input} = {expectedMagnitude}"
                })
    ]

    testList "numbers are always reduced" [
        let cases = [
            "[[[[[9,8],1],2],3],4]", "[[[[0,9],2],3],4]"
            "[7,[6,[5,[4,[3,2]]]]]", "[7,[6,[5,[7,0]]]]"
            "[[6,[5,[4,[3,2]]]],1]", "[[6,[5,[7,0]]],3]"
            "[[3,[2,[1,[7,3]]]],[6,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]"
            "[[3,[2,[8,0]]],[9,[5,[4,[3,2]]]]]", "[[3,[2,[8,0]]],[9,[5,[7,0]]]]"
        ]

        yield!
            cases
            |> List.map (fun (input, expectedNumber) ->
                test $"it explodes {input} into a reduced form" {
                    let expectedResult = SnailfishNumber.tryParse expectedNumber
                    let result = SnailfishNumber.tryParse input
                    Expect.equal result expectedResult $"{input} → {expectedNumber}"
                })

        let cases = [
            "[10,5]", "[[5,5],5]"
            "[5,11]", "[5,[5,6]]"
        ]

        yield!
            cases
            |> List.map (fun (input, expectedNumber) ->
                test $"it splits {input} into a reduced form" {
                    let expectedResult = SnailfishNumber.tryParse expectedNumber
                    let result = SnailfishNumber.tryParse input
                    Expect.equal result expectedResult $"{input} → {expectedNumber}"
                })
    ]
]
