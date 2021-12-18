// SPDX-License-Identifier: GPL-3.0-only

module Advent2021.Solutions.Day17

[<Struct>]
type Point = { X: int; Y: int }

[<Struct>]
type Velocity = { X: int; Y: int }

module Point =
    open FSharpPlus

    let x (pt: Point) = pt.X
    let y (pt: Point) = pt.Y

    let move (v: Velocity) (pt: Point) =
        { Point.X = v.X + pt.X
          Point.Y = v.Y + pt.Y }

    let origin = { Point.X = zero; Point.Y = zero }

module Velocity =
    let x v = v.X
    let y v = v.Y

    let decay v = { X = v.X - (sign v.X); Y = v.Y - 1 }

[<Struct>]
type Probe = { Position: Point; Velocity: Velocity }

module Probe =
    let position (probe: Probe) = probe.Position
    let velocity (probe: Probe) = probe.Velocity

    let create v =
        { Position = Point.origin
          Velocity = v }

    let step probe =
        { Position = Point.move probe.Velocity probe.Position
          Velocity = Velocity.decay probe.Velocity }

    let rec steps probe =
        seq {
            yield position probe
            yield! steps (step probe)
        }

type Target =
    { Position: Point
      Width: int
      Height: int }

module Target =
    let position t = t.Position
    let width t = t.Width
    let height t = t.Height

    let check pt target =
        let bounds pt length =
            let lower = (position >> pt) target
            let upper = lower + (length target)
            lower, upper

        let xLower, xUpper = bounds Point.x width
        let yLower, yUpper = bounds Point.y height

        let x = Point.x pt
        let y = Point.y pt

        x >= xLower
        && x <= xUpper
        && y >= yLower
        && y <= yUpper

open System.CommandLine
open System.CommandLine.Invocation

open FSharp.Control
open FSharpPlus

type Options =
    { XStart: int
      XEnd: int
      YStart: int
      YEnd: int
      Print: bool }

let run (options: Options) (console: IConsole) =
    task {
        let target =
            { Position =
                { X = options.XStart
                  Y = options.YStart }
              Width = options.XEnd - options.XStart
              Height = options.YEnd - options.YStart }

        // Assume the target area is to the right and down
        let maxX =
            (Target.position >> Point.x) target
            + Target.width target

        let minY =
            (Target.position >> Point.y) target
            - Target.height target

        let velocities =
            seq { 0 .. maxX }
            |> Seq.skipWhile (fun x -> x * (x + 1) < 2 * (Target.position >> Point.x) target)
            |> Seq.collect (fun x ->
                seq { minY .. -minY }
                |> Seq.choose (fun y ->
                    let pts =
                        Probe.create { X = x; Y = y }
                        |> Probe.steps
                        |> Seq.take 1000

                    match Seq.tryFindIndex (flip Target.check target) pts with
                    | Some index ->
                        let maxY =
                            pts
                            |> Seq.take index
                            |> Seq.map Point.y
                            |> Seq.max

                        Some ({ X = x; Y = y }, maxY)
                    | _ -> None))
            |> List.ofSeq

        let v, maxY = List.maxBy snd velocities
        console.Out.Write $"Out of {List.length velocities} starting velocities:\n"
        console.Out.Write $"- ({Velocity.x v},{Velocity.y v}) has highest Y height of {maxY}\n"

        if options.Print then
            console.Out.Write "\nVelocities\n==========\n"

            velocities
            |> List.iter (fun (v, _) -> console.Out.Write $"({Velocity.x v},{Velocity.y v})\n")

        return 0
    }

let command =
    let cmd =
        Command ("day17", "Trick Shot", Handler = CommandHandler.Create run)

    cmd.AddOption
    <| Option<int> (
        aliases = [| "-xs"; "--x-start" |],
        description = "the starting x coordinate of the target area",
        IsRequired = true
    )

    cmd.AddOption
    <| Option<int> (
        aliases = [| "-xe"; "--x-end" |],
        description = "the ending x coordinate of the target area",
        IsRequired = true
    )

    cmd.AddOption
    <| Option<int> (
        aliases = [| "-ys"; "--y-start" |],
        description = "the starting y coordinate of the target area",
        IsRequired = true
    )

    cmd.AddOption
    <| Option<int> (
        aliases = [| "-ye"; "--y-end" |],
        description = "the ending y coordinate of the target area",
        IsRequired = true
    )

    cmd.AddOption
    <| Option<bool> (aliases = [| "-p"; "--print" |], description = "print all velocities")

    cmd
