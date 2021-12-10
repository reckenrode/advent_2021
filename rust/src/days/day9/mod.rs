// SPDX-License-Identifier: GPL-3.0-only

use std::path::PathBuf;

mod heightmap;

use anyhow::Result;
use clap::Parser;
use itertools::Itertools;

use crate::util::read_input;

use self::heightmap::Heightmap;

#[derive(Parser)]
#[clap(about = "Smoke Basin")]
pub(crate) struct Day9 {
    input: PathBuf,
}

impl Day9 {
    pub(crate) fn run(self) -> Result<()> {
        let input = read_input(self.input)?;
        let heightmap = Heightmap::parse(input.as_str())?;

        let low_points: Vec<_> = heightmap
            .filter(|value, neighbors| {
                [
                    neighbors.top.unwrap_or(0xA),
                    neighbors.bottom.unwrap_or(0xA),
                    neighbors.left.unwrap_or(0xA),
                    neighbors.right.unwrap_or(0xA),
                ]
                .into_iter()
                .all(|neighbor| value < neighbor)
            })
            .collect();

        let risk_levels = low_points.iter().map(|pt| pt.value + 1);
        println!("The sum of the risk levels: {}", risk_levels.sum::<u32>());

        let basins: usize = low_points
            .iter()
            .map(|pt| heightmap.map_basin(pt.row, pt.column).len())
            .sorted_unstable()
            .rev()
            .take(3)
            .product();
        println!("The product of the three largest basins: {}", basins);

        Ok(())
    }
}
