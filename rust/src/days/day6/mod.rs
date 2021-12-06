// SPDX-License-Identifier: GPL-3.0-only

use std::{fs::File, io::BufReader, path::PathBuf};

use anyhow::Result;
use clap::Parser;

use crate::days::day6::fish::Fish;

mod fish;

#[derive(Parser)]
#[clap(about = "Lanternfish")]
pub(crate) struct Day6 {
    input: PathBuf,
    #[clap(short, long)]
    days: usize,
}

impl Day6 {
    pub(crate) fn run(self) -> Result<()> {
        let file = File::open(self.input)?;
        let reader = BufReader::new(file);

        let mut fish = Fish::parse(reader)?;
        (0..self.days).for_each(|_| fish.tick());
        println!("After {} days, there are {} fish.", self.days, fish.count());

        Ok(())
    }
}
