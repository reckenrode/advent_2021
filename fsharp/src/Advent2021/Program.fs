// SPDX-License-Identifier: GPL-3.0-only

open Advent2021

[<EntryPoint>]
let main args =
    let awaiter = (Cli.run args).GetAwaiter ()
    awaiter.GetResult ()
