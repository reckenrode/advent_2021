// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day17

open Expecto

open Advent2021.Solutions.Day17

[<Tests>]
let tests =
    testList "Day17" [
        testList "Probe" [
            testList "when it has an x velocity" [
                test "it moves by the velocity" {
                    let expectedPosition = { Point.X = 1; Point.Y = 0 }
                    let probe =
                        Probe.create { X = 1; Y = 0 }
                        |> Probe.step
                    Expect.equal (Probe.position probe) expectedPosition "the probe moved to (1, 0)"
                }

                test "it loses velocity due to drag" {
                    let expectedVelocity = { X = 0; Y = -1 }
                    let probe =
                        Probe.create { X = 1; Y = 0 }
                        |> Probe.step
                    Expect.equal (Probe.velocity probe) expectedVelocity "the x-velocity is 0"
                }

                test "it loses velocity due to drag (negative case)" {
                    let expectedVelocity = { X = 0; Y = -1 }
                    let probe =
                        Probe.create { X = -1; Y = 0 }
                        |> Probe.step
                    Expect.equal (Probe.velocity probe) expectedVelocity "the x-velocity is 0"
                }

                test "it remains the same at zero" {
                    let expectedVelocity = { X = 0; Y = -1 }
                    let probe =
                        Probe.create { X = 0; Y = 0 }
                        |> Probe.step
                    Expect.equal (Probe.velocity probe) expectedVelocity "the x-velocity is 0"
                }
            ]

            testList "when it has a y velocity" [
                test "it moves by the velocity" {
                    let expectedPosition = { Point.X = 0; Point.Y = 5 }
                    let probe =
                        Probe.create { X = 0; Y = 5 }
                        |> Probe.step
                    Expect.equal (Probe.position probe) expectedPosition "the probe moved to (0, 5)"
                }

                test "it loses velocity due to gravity when positive" {
                    let expectedVelocity = { X = 0; Y = 4 }
                    let probe =
                        Probe.create { X = 0; Y = 5 }
                        |> Probe.step
                    Expect.equal (Probe.velocity probe) expectedVelocity "the y-velocity is 4"
                }

                test "it gains velocity due to gravity when negative" {
                    let expectedVelocity = { X = 0; Y = -6 }
                    let probe =
                        Probe.create { X = 0; Y = -5 }
                        |> Probe.step
                    Expect.equal (Probe.velocity probe) expectedVelocity "the y-velocity is -6"
                }
            ]
        ]

        testList "Target" [
            test "it returns true when the probe is in it" {
                let pt = { Point.X = 0; Point.Y = 0 }
                let area =
                    { Position = { X = -1; Y = -1 }
                      Width = 3;
                      Height = 3 }
                let result = Target.check pt area
                Expect.equal result true "the probe is in the area"
            }

            test "it returns false when the probe is not in it" {
                let pt = { Point.X = 5; Point.Y = 5 }
                let area =
                    { Position = { X = -1; Y = -1 }
                      Width = 3;
                      Height = 3 }
                let result = Target.check pt area
                Expect.equal result false "the probe is not in the area"
            }
        ]

        test "it steps in sequence" {
            let expectedSequence = [
                { Point.X = 0; Point.Y = 0 }
                { Point.X = 7; Point.Y = 2 }
                { Point.X = 13; Point.Y = 3 }
                { Point.X = 18; Point.Y = 3 }
                { Point.X = 22; Point.Y = 2 }
                { Point.X = 25; Point.Y = 0 }
                { Point.X = 27; Point.Y = -3 }
                { Point.X = 28; Point.Y = -7 }
            ]
            let probe = Probe.create { X = 7; Y = 2 }
            let sequence =
                Probe.steps probe
                |> Seq.take 8
                |> List.ofSeq
            Expect.equal sequence expectedSequence "the probe moved as expected"
        }
    ]
