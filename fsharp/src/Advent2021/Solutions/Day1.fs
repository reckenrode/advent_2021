// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day1

open System.CommandLine
open type System.Convert
open System.IO

open FSharp.Control
open FSharpPlus

let countIncreases windowSize : seq<int> -> int =
    Seq.windowed windowSize
    >> Seq.map Array.sum
    >> Seq.pairwise
    >> Seq.fold (fun acc (previous, current) -> acc + ToInt32 (current > previous)) 0

let rec parse (reader: TextReader) : AsyncSeq<option<int>> =
    asyncSeq {
        let! line = reader.ReadLineAsync () |> Async.AwaitTask

        match Option.ofObj line with
        | Some line ->
            yield Option.ofPair (System.Int32.TryParse line)
            yield! parse reader
        | None -> ()
    }

let toOptionList lst =
    let rec toOptionList' rst =
        function
        | (Some x) :: xs -> toOptionList' (x :: rst) xs
        | None :: _ -> None
        | [] -> Some (List.rev rst)

    toOptionList' [] lst

type Options = { Input: FileInfo }

let run (options: Options) (console: IConsole) =
    task {
        use file = options.Input.OpenRead ()
        use reader = new StreamReader (file)
        let! numbers = parse reader |> AsyncSeq.toListAsync

        match toOptionList numbers with
        | Some numbers ->
            let increases = countIncreases 1 numbers
            console.Out.Write $"Times the depth measurement increased: {increases}\n"
            let increases = countIncreases 3 numbers
            console.Out.Write $"Times the sliding depth measurement increased: {increases}\n"
        | None -> console.Error.Write $"There was a problem reading the file."

        return 0
    }

let command = Command.create "day1" "Sonar Sweep" run
