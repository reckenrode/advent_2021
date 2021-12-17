// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Tests.Solutions.Day16

open System

open Expecto
open FSharpPlus

open Advent2021.Solutions.Day16

[<Tests>]
let test =
    testList "Day16" [
        testList "packet decoder" [
            test "it decodes the version header" {
                let expectedVersion = Ok 6uy
                let version =
                    Packet.decode "D2FE28"
                    |> Result.map Packet.version
                Expect.equal version expectedVersion "the versions match"
            }

            test "it decodes the type id" {
                let expectedTypeId = Ok 4uy
                let typeId =
                    Packet.decode "D2FE28"
                    |> Result.map Packet.typeId
                Expect.equal typeId expectedTypeId "the type ids match"
            }

            test "it decodes literal packets" {
                let expectedLiteral = Ok (Literal 2021I)
                let literal =
                    Packet.decode "D2FE28"
                    |> Result.map Packet.payload
                Expect.equal literal expectedLiteral "the literals match"
            }

            testList "example packets" [
                let cases = [
                    "D2FE28", Packet.ofRaw ((6uy, 4uy), Literal 2021I)
                    "38006F45291200", Packet.ofRaw ((1uy, 6uy), SubPackets [
                        Packet.ofRaw ((6uy, 4uy), Literal 10I)
                        Packet.ofRaw ((2uy, 4uy), Literal 20I)
                    ])
                    "EE00D40C823060", Packet.ofRaw ((7uy, 3uy), SubPackets [
                        Packet.ofRaw ((2uy, 4uy), Literal 1I)
                        Packet.ofRaw ((4uy, 4uy), Literal 2I)
                        Packet.ofRaw ((1uy, 4uy), Literal 3I)
                    ])
                    "8A004A801A8002F478", Packet.ofRaw ((4uy, 2uy), SubPackets [
                        Packet.ofRaw ((1uy, 2uy), SubPackets [
                            Packet.ofRaw ((5uy, 2uy), SubPackets [
                                Packet.ofRaw ((6uy, 4uy), Literal 15I)
                            ])
                        ])
                    ])
                    "620080001611562C8802118E34", Packet.ofRaw ((3uy, 0uy), SubPackets [
                        Packet.ofRaw ((0uy, 0uy), SubPackets [
                            Packet.ofRaw ((0uy, 4uy), Literal 10I)
                            Packet.ofRaw ((5uy, 4uy), Literal 11I)
                        ])
                        Packet.ofRaw ((1uy, 0uy), SubPackets [
                            Packet.ofRaw ((0uy, 4uy), Literal 12I)
                            Packet.ofRaw ((3uy, 4uy), Literal 13I)
                        ])
                    ])
                    "C0015000016115A2E0802F182340", Packet.ofRaw ((6uy, 0uy), SubPackets [
                        Packet.ofRaw ((0uy, 0uy), SubPackets [
                            Packet.ofRaw ((0uy, 4uy), Literal 10I)
                            Packet.ofRaw ((6uy, 4uy), Literal 11I)
                        ])
                        Packet.ofRaw ((4uy, 0uy), SubPackets [
                            Packet.ofRaw ((7uy, 4uy), Literal 12I)
                            Packet.ofRaw ((0uy, 4uy), Literal 13I)
                        ])
                    ])
                    "A0016C880162017C3686B18A3D4780", Packet.ofRaw ((5uy, 0uy), SubPackets [
                        Packet.ofRaw ((1uy, 0uy), SubPackets [
                            Packet.ofRaw ((3uy, 0uy), SubPackets [
                                Packet.ofRaw ((7uy, 4uy), Literal 6I)
                                Packet.ofRaw ((6uy, 4uy), Literal 6I)
                                Packet.ofRaw ((5uy, 4uy), Literal 12I)
                                Packet.ofRaw ((2uy, 4uy), Literal 15I)
                                Packet.ofRaw ((2uy, 4uy), Literal 15I)
                            ])
                        ])
                    ])
                ]

                yield!
                    cases
                    |> List.map (fun (case, p) -> test $"it decodes the example packet {case}" {
                        let expectedResult = Ok p
                        let result = Packet.decode case
                        Expect.equal result expectedResult "the packet is decoded correctly"
                    })
            ]
        ]
    ]
