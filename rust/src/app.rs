// SPDX-License-Identifier: GPL-3.0-only

use clap::Parser;

#[derive(Parser)]
#[clap(about, author, version)]
pub(crate) struct App {
    #[clap(subcommand)]
    cmd: crate::days::Command,
}

pub(crate) fn parse_options() -> App {
    App::parse()
}

impl App {
    pub(crate) fn run(self) -> anyhow::Result<()> {
        self.cmd.run()
    }
}
