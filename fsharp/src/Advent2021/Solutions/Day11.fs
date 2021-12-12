// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day11

type OctopusField = private OctopusField of list<list<uint32>>
    with override this.ToString () =
            let (OctopusField field) = this
            field
            |> List.map (List.map string >> String.concat "")
            |> String.concat "\n"

module OctopusField =
    open type System.Convert
    open System.IO
    open System.Text

    open FSharpPlus
    open FParsec

    let parse (fd: FileInfo) =
        let digit = digit |>> (fun ch -> (uint32 ch) - (uint32 '0'))
        let line = manyTill digit newline
        let file = manyTill line eof
        use stream = fd.OpenRead ()
        match runParserOnStream file () fd.Name stream Encoding.UTF8 with
        | Success (value, _, _) -> Result.Ok value
        | Failure (message, _, _) -> Result.Error message

    let ofList lst =
        List.fold (fun dims row ->
            monad' {
                let! width, height = dims
                match List.length row with
                | w when width = 0 || width = w -> return w, height + 1
                | _ -> return! Result.Error "Input field has jagged edges, which is not supported."
            }) (Result.Ok (0, 0)) lst
        |> Result.map (fun _ -> OctopusField lst)

    let didAllFlash (OctopusField field) =
        List.forall (List.forall ((=) 0u)) field

    let step (OctopusField field) =
        let clamp value = if value >= 10u then 0u else value

        let increment flashed row column value =
            let neighbors = [
                    row + 1, column
                    row - 1, column
                    row, column + 1
                    row, column - 1
                    row + 1, column + 1
                    row - 1, column + 1
                    row + 1, column - 1
                    row - 1, column - 1
                ]
            let neighborFlashes =
                neighbors
                |> List.map (flip Set.contains flashed >> ToUInt32)
                |> List.sum
            let newValue = value + neighborFlashes + ToUInt32 (Set.isEmpty flashed)
            let maybeFlashed =
                if newValue > 9u && not (Set.contains (row, column) flashed)
                then Some (row, column)
                else None
            newValue, maybeFlashed

        let increaseEnergy field flashed =
            field
            |> List.mapi (fun row -> List.mapi (increment flashed row) >> List.unzip)
            |> List.unzip

        let rec step' field flashed pastFlashes =
            let field, newlyFlashed = increaseEnergy field flashed
            let newlyFlashed =
                newlyFlashed
                |> List.map (List.choose id >> Set.ofList)
                |> Set.unionMany
                |> flip Set.difference pastFlashes
            if Set.isEmpty newlyFlashed
            then OctopusField (List.map (List.map clamp) field), pastFlashes
            else step' field newlyFlashed (Set.union pastFlashes newlyFlashed)

        step' field Set.empty Set.empty

open System.CommandLine
open System.CommandLine.Rendering
open System.IO

open FSharpPlus

type Options = {
    Input: FileInfo
    Steps: uint32
    Print: bool
    Bold: bool
}

let run (options: Options) (console: IConsole) =
    let renderField field =
        if options.Print
        then
            let output =
                sprintf $"{field}\n"
                |> if options.Bold
                    then String.replace "0" $"{Ansi.Text.BoldOn}0{Ansi.Text.BoldOff}"
                    else id
            console.Out.Write output

    task {
        match OctopusField.parse options.Input >>= OctopusField.ofList with
        | Error message ->
            console.Error.Write($"Error reading the input file: {message}\n")
            return 1
        | Ok field ->
            let runSteps field n =
                let rec loop field n flashes =
                    if n = 0u
                    then flashes, field
                    else
                        let newField, newFlashes = OctopusField.step field
                        loop newField (n - 1u) (flashes + Set.count newFlashes)
                loop field n 0
            let flashes, finalField = runSteps field options.Steps

            console.Out.Write($"Number of flashes in {options.Steps} steps: {flashes}\n")
            renderField finalField

            let findFirstFlash field =
                let rec loop field n =
                    if OctopusField.didAllFlash field
                    then n, field
                    else loop (OctopusField.step field |> fst) (n + 1)
                loop field 0
            let steps, finalField = findFirstFlash field

            console.Out.Write($"It took {steps} steps to find the first step with all flashes\n")
            renderField finalField

            return 0
    }

let command =
    let command = Command.create "day11" "Dumbo Octopus" run
    command.AddOption <| Option<uint32> (
        aliases = [| "-s"; "--steps" |],
        description = "the number of steps to iterate",
        IsRequired = true
    )
    command.AddOption <| Option<bool> (
        aliases = [| "-p"; "--print" |],
        description = "print the final field"
    )
    command.AddOption <| Option<bool> (
        aliases = [| "-b"; "--bold" |],
        description = "bold the flashes"
    )
    command
