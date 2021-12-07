// SPDX-License-Identifier: GPL-3.0-only

mod crabs;

use std::path::PathBuf;

use anyhow::Result;
use clap::Parser;

use crate::util::read_input;

use self::crabs::Positions;

#[derive(Parser)]
#[clap(about = "Lanternfish")]
pub(crate) struct Day7 {
    input: PathBuf,
    #[clap(short, long)]
    crab_engineering: bool,
}

impl Day7 {
    pub(crate) fn run(self) -> Result<()> {
        let input = read_input(self.input)?;
        let positions = Positions::try_from(input.as_str())?;
        let (new_position, fuel_cost) =
            positions.lowest_cost_target_position(self.crab_engineering);
        println!("Fuel cost for move to {}: {}", new_position, fuel_cost);
        Ok(())
    }
}
