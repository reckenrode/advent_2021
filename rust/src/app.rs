// SPDX-License-Identifier: GPL-3.0-only

use clap::Parser;

use crate::declare_days;

#[derive(Parser)]
#[clap(about, author, version)]
pub(crate) struct App {
    #[clap(subcommand)]
    cmd: Command,
}

declare_days! [
    Day1,
    Day2,
    Day3,
    Day4,
    Day5,
    Day6,
    Day7,
];

pub(crate) fn parse_options() -> App {
    App::parse()
}
