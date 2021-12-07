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
}

impl Day7 {
    pub(crate) fn run(self) -> Result<()> {
        let input = read_input(self.input)?;
        let positions = Positions::try_from(input.as_str())?;
        let new_position = positions.lowest_cost_target_position();
        let fuel_cost = positions.cost_to_move(new_position);
        println!("Fuel cost for move to {}: {}", new_position, fuel_cost);
        Ok(())
    }
}
