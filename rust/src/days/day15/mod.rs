// SPDX-License-Identifier: GPL-3.0-only

mod graph;

use std::path::PathBuf;

use anyhow::Result;
use clap::Parser;

use crate::util::read_input;

use self::graph::Graph;

#[derive(Parser)]
#[clap(about = "Chiton")]
pub(crate) struct Day15 {
    input: PathBuf,
}

impl Day15 {
    pub(crate) fn run(self) -> Result<()> {
        let input = read_input(self.input)?;
        let input: Vec<Vec<u8>> = input
            .lines()
            .map(|s| s.chars().map(|ch| ch.to_digit(10).unwrap() as u8).collect())
            .collect();
        let mut input = Graph::from(input);

        let cavern_risk = input.shortest_path_cost((0, 0), (input.rows - 1, input.columns - 1));
        println!("The lowest cavern risk cost: {}", cavern_risk);

        input.grow(4, 4);
        let cave_risk = input.shortest_path_cost((0, 0), (input.rows - 1, input.columns - 1));
        println!("The lowest cave risk cost: {}", cave_risk);

        Ok(())
    }
}
