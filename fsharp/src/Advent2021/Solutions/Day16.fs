// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day16

type Payload =
    | Blob
    | Literal of bigint
    | SubPackets of list<Packet>

and [<Struct>] Packet =
    private
        { Version: uint8
          TypeId: uint8
          Payload: Payload }

module Packet =
    open FSharpPlus
    open FParsec

    let ofRaw ((version, typeId), payload) =
        { Version = version
          TypeId = typeId
          Payload = payload }

    let private toBinaryRep str =
        str
        |> Seq.map (function
            | '0' -> "0000"
            | '1' -> "0001"
            | '2' -> "0010"
            | '3' -> "0011"
            | '4' -> "0100"
            | '5' -> "0101"
            | '6' -> "0110"
            | '7' -> "0111"
            | '8' -> "1000"
            | '9' -> "1001"
            | 'A' -> "1010"
            | 'B' -> "1011"
            | 'C' -> "1100"
            | 'D' -> "1101"
            | 'E' -> "1110"
            | 'F' -> "1111"
            | _ -> failwith "invalid hex character") // FIXME: make this return result
        |> String.concat ""

    let private packet =
        let bit ch = ch = '0' || ch = '1'

        let inline ofBits t =
            Seq.map (t >> (flip (-) (t '0')))
            >> Seq.fold (fun num bit -> (num <<< 1) + bit) zero

        let inline ofNibbles lst =
            Seq.fold (fun num nibble -> (num <<< 4) + nibble) zero lst

        let (packet, packetRef) =
            createParserForwardedToRef<Packet, uint16> ()

        let literalValue =
            let literalDigit =
                manyMinMaxSatisfy 4 4 bit <?> "literal nibble" |>> ofBits bigint

            let literalInterstitial = satisfy ((=) '1') >>. literalDigit
            let literalEnding = satisfy ((=) '0') >>. literalDigit

            many literalInterstitial .>>. literalEnding
            |>> (fun (nibbles, lastNibble) -> Seq.append nibbles [lastNibble] |> ofNibbles |> Literal)

        let operator =
            let subPacketsByLength =
                let packetBlob length =
                    let (subPackets, subPacketsRef) = createParserForwardedToRef<list<Packet>, uint16> ()
                    let subPackets' targetColumn =
                        packet
                        .>>. getPosition >>= (fun (packet, position) ->
                            match compare targetColumn position.Column with
                                | -1 -> fail "invalid subpacket format: longer than expected"
                                |  0 -> preturn [packet]
                                |  1 -> preturn packet .>>. subPackets |>> uncurry List.cons
                                | _ -> failwith "`compare` returned an invalid result")

                    getPosition >>= (fun position ->
                        let targetColumn = position.Column + (int64 length)
                        subPacketsRef.Value <- subPackets' targetColumn
                        subPackets <?> "subpacket")
                    |>> SubPackets

                manyMinMaxSatisfy 15 15 bit <?> "size of subpackets" |>> ofBits uint16
                >>= packetBlob

            let subPacketsByCount =
                let subPackets count =
                    let (subPackets, subPacketsRef) =
                        createParserForwardedToRef<list<Packet>, uint16> ()

                    let mutable n = 0us
                    let subPackets' =
                        packet
                        >>= (fun packet ->
                            n <- n + 1us
                            if n = count then
                                preturn [packet]
                            else
                                preturn packet .>>. subPackets |>> uncurry List.cons)

                    subPacketsRef.Value <- subPackets'
                    subPackets <?> "subpacket"
                    |>> SubPackets

                manyMinMaxSatisfy 11 11 bit <?> "subpacket count" |>> ofBits uint16
                >>= subPackets <?> "subpackets"

            (attempt (pchar '0') >>. subPacketsByLength) <|> (pchar '1' >>. subPacketsByCount)

        let version =
            manyMinMaxSatisfy 3 3 bit <?> "packet version" |>> ofBits uint8

        let typeId =
            manyMinMaxSatisfy 3 3 bit <?> "packet type id" |>> ofBits uint8
            >>= (fun n -> setUserState (uint16 n) >>. preturn n)

        let payload =
            userStateSatisfies ((=) 4us) >>. literalValue <?> "literal value"
            <|> operator <?> "operator"

        let header = (version .>>. typeId) <?> "packet header"
        let packet = (header .>>. payload) <?> "packet" |>> ofRaw

        packetRef.Value <- packet
        packet

    let decode str =
        match runParserOnString packet 0us "packet parser" (toBinaryRep str) with
        | Success (packet, _, _) -> Result.Ok packet
        | Failure (message, _, _) -> Result.Error message

    let version packet = packet.Version
    let typeId packet = packet.TypeId
    let payload packet = packet.Payload

    let rec eval =
        function
        | { TypeId = 0uy
            Payload = SubPackets packets } -> List.sumBy eval packets
        | { TypeId = 1uy
            Payload = SubPackets packets } -> List.map eval packets |> List.reduce (*)
        | { TypeId = 2uy
            Payload = SubPackets packets } -> List.map eval packets |> List.min
        | { TypeId = 3uy
            Payload = SubPackets packets } -> List.map eval packets |> List.max
        | { TypeId = 4uy
            Payload = Literal x } -> x
        | { TypeId = 5uy
            Payload = SubPackets [ lhs; rhs ] } -> if eval lhs > eval rhs then 1I else 0I
        | { TypeId = 6uy
            Payload = SubPackets [ lhs; rhs ] } -> if eval lhs < eval rhs then 1I else 0I
        | { TypeId = 7uy
            Payload = SubPackets [ lhs; rhs ] } -> if eval lhs = eval rhs then 1I else 0I
        | p -> failwith $"packet {p} that cannot be evaluated was evaluated (Santa is doomed, probably)"

open System.CommandLine
open System.IO

open FSharpPlus

type Options = {
    Input: FileInfo
}

let run (options: Options) (console: IConsole) =
    let sumVersionNumbers packet =
        let rec versions packet =
            seq {
                yield Packet.version packet |> uint64
                match Packet.payload packet with
                | SubPackets packets ->
                    yield! Seq.collect versions packets
                | _ -> ()
            }
        versions packet
        |> Seq.sum
    task {
        use file = options.Input.OpenRead ()
        use reader = new StreamReader (file)
        let! input = reader.ReadToEndAsync ()
        let input = String.trimWhiteSpaces input

        return
            monad' {
                let! packet = Packet.decode input

                let versionSums = sumVersionNumbers packet
                console.Out.Write $"The sum of the version numbers is {versionSums}.\n"

                let result = Packet.eval packet
                console.Out.Write $"The result of evaluating the packet: {result}\n"

                return 0
            }
            |> handleFailure console
    }

let command = Command.create "day16" "Packet Decoder" run
