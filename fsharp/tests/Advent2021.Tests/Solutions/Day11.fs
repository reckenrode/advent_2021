// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day11

open Expecto
open FSharpPlus

open Advent2021.Solutions.Day11

[<Tests>]
let tests = testList "OctopusField" [
    testList "step" [
        test "it increases the energy level of each octopus by 1" {
            let expectedField = Result.get (OctopusField.ofList [
                [2u; 2u; 2u; 2u]
                [2u; 2u; 2u; 2u]
                [2u; 2u; 2u; 2u]
                [2u; 2u; 2u; 2u]
            ])
            let inputField = Result.get (OctopusField.ofList [
                [1u; 1u; 1u; 1u]
                [1u; 1u; 1u; 1u]
                [1u; 1u; 1u; 1u]
                [1u; 1u; 1u; 1u]
            ])
            let resultField, _ = OctopusField.step inputField
            Expect.equal resultField expectedField "the energy levels increased"
        }

        testList "energy level greater than 9" [
            test "it does not reset energy levels just at 9" {
                let expectedField = Result.get (OctopusField.ofList [
                    [9u]
                ])
                let inputField = Result.get (OctopusField.ofList [
                    [8u]
                ])
                let resultField, _ = OctopusField.step inputField
                Expect.equal resultField expectedField "the energy levels increased"
            }

            test "it resets the energy level to 0" {
                let expectedField = Result.get (OctopusField.ofList [
                    [0u]
                ])
                let inputField = Result.get (OctopusField.ofList [
                    [9u]
                ])
                let resultField, _ = OctopusField.step inputField
                Expect.equal resultField expectedField "the energy levels increased"
            }

            test "it causes adjacent energy levels to increase by 1" {
                let expectedField = Result.get (OctopusField.ofList [
                    [0u; 3u; 2u; 2u]
                    [3u; 3u; 2u; 2u]
                    [2u; 2u; 2u; 2u]
                    [2u; 2u; 2u; 2u]
                ])
                let inputField = Result.get (OctopusField.ofList [
                    [9u; 1u; 1u; 1u]
                    [1u; 1u; 1u; 1u]
                    [1u; 1u; 1u; 1u]
                    [1u; 1u; 1u; 1u]
                ])
                let resultField, _ = OctopusField.step inputField
                Expect.equal resultField expectedField "the energy levels increased"
            }

            test "it causes a chain reaction of increases" {
                let expectedField = Result.get (OctopusField.ofList [
                    [3u; 4u; 5u; 4u; 3u]
                    [4u; 0u; 0u; 0u; 4u]
                    [5u; 0u; 0u; 0u; 5u]
                    [4u; 0u; 0u; 0u; 4u]
                    [3u; 4u; 5u; 4u; 3u]
                ])
                let inputField = Result.get (OctopusField.ofList [
                    [1u; 1u; 1u; 1u; 1u]
                    [1u; 9u; 9u; 9u; 1u]
                    [1u; 9u; 1u; 9u; 1u]
                    [1u; 9u; 9u; 9u; 1u]
                    [1u; 1u; 1u; 1u; 1u]
                ])
                let resultField, _ = OctopusField.step inputField
                Expect.equal resultField expectedField "the energy levels increased"
            }

            test "it returns the fields that flashed" {
                let expectedFlashes = Set.ofList [
                    1, 1; 1, 2; 1, 3;
                    2, 1; 2, 2; 2, 3;
                    3, 1; 3, 2; 3, 3
                ]
                let inputField = Result.get (OctopusField.ofList [
                    [1u; 1u; 1u; 1u; 1u]
                    [1u; 9u; 9u; 9u; 1u]
                    [1u; 9u; 1u; 9u; 1u]
                    [1u; 9u; 9u; 9u; 1u]
                    [1u; 1u; 1u; 1u; 1u]
                ])
                let _, flashes = OctopusField.step inputField
                Expect.equal flashes expectedFlashes "the fields flashed"
            }
        ]
    ]

    testList "didAllFlash" [
        test "it is true when all energy levels are 0" {
            let inputField = Result.get (OctopusField.ofList [
                [0u; 0u; 0u; 0u]
                [0u; 0u; 0u; 0u]
                [0u; 0u; 0u; 0u]
                [0u; 0u; 0u; 0u]
            ])
            let output = OctopusField.didAllFlash inputField
            Expect.isTrue output "they flashed"
        }
    ]
]
