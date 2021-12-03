// SPDX-License-Identifier: GPL-3.0-only

use std::{fs::File, io::BufReader, path::PathBuf};

use anyhow::Result;
use clap::Parser;

use crate::days::day2::program::Program;

mod program;

#[derive(Parser)]
#[clap(about = "Submarine movement")]
pub(crate) struct Day2 {
    input: PathBuf,
    #[clap(short, long)]
    multiply_results: bool,
    #[clap(short, long)]
    use_aim: bool,
}

impl Day2 {
    pub fn run(self) -> Result<()> {
        let file = File::open(self.input)?;
        let reader = BufReader::new(file);
        let program = Program::parse(reader, self.use_aim)?;
        let result = program.run();
        println!(
            "Position: \t{}\nDepth:\t\t{}",
            result.position, result.depth
        );
        if self.multiply_results {
            println!("Multiplied together: {}", result.position * result.depth);
        }
        Ok(())
    }
}
