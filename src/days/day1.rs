// SPDX-License-Identifier: GPL-3.0-only

use std::{error::Error, path::PathBuf};

use clap::Parser;

#[derive(Parser)]
#[clap(about = "Santa is doing something")]
pub(crate) struct Day1 {
    input: PathBuf,
}

impl Day1 {
    pub(crate) fn run(self) -> Result<(), Box<dyn Error>> {
        println!("Santa does some stuff");
        Ok(())
    }
}
