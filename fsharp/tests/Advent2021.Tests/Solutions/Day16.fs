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

        testList "packet evaluator" [
            testList "sum" [
                test "it returns the sub-packet’s value as the sum" {
                    let expectedValue = 42I
                    let input = Packet.ofRaw ((0uy, 0uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 42I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "the sums match"
                }

                test "it returns the sub-packets’ values as the sum" {
                    let expectedValue = 42I
                    let input = Packet.ofRaw ((0uy, 0uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "the sums match"
                }
            ]

            testList "product" [
                test "it returns the sub-packet’s value as the product" {
                    let expectedValue = 42I
                    let input = Packet.ofRaw ((0uy, 1uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 42I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "the products match"
                }

                test "it returns the sub-packets’ values as the product" {
                    let expectedValue = 42I
                    let input = Packet.ofRaw ((0uy, 1uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "the products match"
                }
            ]

            testList "minimum" [
                test "it returns the minimum of the sub-packets" {
                    let expectedValue = 2I
                    let input = Packet.ofRaw ((0uy, 2uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "the minimum value was found"
                }
            ]

            testList "maximum" [
                test "it returns the maximum of the sub-packets" {
                    let expectedValue = 21I
                    let input = Packet.ofRaw ((0uy, 3uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "the maximum value was found"
                }
            ]

            testList "greaterr than" [
                test "it returns 1 when the first sub-packet is greater than the second" {
                    let expectedValue = 1I
                    let input = Packet.ofRaw ((0uy, 5uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "21 > 2 is true"
                }

                test "it returns 0 when the first sub-packet is not greater than the second" {
                    let expectedValue = 0I
                    let input = Packet.ofRaw ((0uy, 5uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "2 > 21 is false"
                }
            ]

            testList "less than" [
                test "it returns 1 when the first sub-packet is less than the second" {
                    let expectedValue = 1I
                    let input = Packet.ofRaw ((0uy, 6uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "2 < 21 is true"
                }

                test "it returns 0 when the first sub-packet is not less than the second" {
                    let expectedValue = 0I
                    let input = Packet.ofRaw ((0uy, 6uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "21 < 2 is false"
                }
            ]

            testList "equality" [
                test "it returns 1 when the first sub-packet is equal to the second" {
                    let expectedValue = 1I
                    let input = Packet.ofRaw ((0uy, 7uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "21 = 21 is true"
                }

                test "it returns 0 when the first sub-packet is not equal to the second" {
                    let expectedValue = 0I
                    let input = Packet.ofRaw ((0uy, 7uy), SubPackets [
                        Packet.ofRaw ((0uy, 4uy), Literal 21I)
                        Packet.ofRaw ((0uy, 4uy), Literal 2I)
                    ])
                    let value = Packet.eval input
                    Expect.equal value expectedValue "21 = 2 is false"
                }
            ]

            testList "example packets" [
                let cases = [
                    "C200B40A82", 3I
                    "04005AC33890", 54I
                    "880086C3E88112", 7I
                    "CE00C43D881120", 9I
                    "D8005AC2A8F0", 1I
                    "F600BC2D8F", 0I
                    "9C005AC2F8F0", 0I
                    "9C0141080250320F1802104A08", 1I
                ]

                yield!
                    cases
                    |> List.map (fun (case, p) -> test $"it evaluates the example packet {case}" {
                        let expectedResult = Ok p
                        let result =
                            Packet.decode case
                            |> Result.map Packet.eval
                        Expect.equal result expectedResult "the packet is evaluated correctly"
                    })
            ]
        ]
    ]
