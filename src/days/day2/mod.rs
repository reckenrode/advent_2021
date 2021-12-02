// SPDX-License-Identifier: GPL-3.0-only

use std::{fs::File, io::{BufRead, BufReader}, path::PathBuf};

use anyhow::Result;
use clap::Parser;

use crate::days::day2::command::{Command, State};

mod command;

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
    fn parse_input(reader: impl BufRead, use_aim: bool) -> Result<Program> {
        let result: Result<Vec<_>> = reader
            .lines()
            .filter_map(|line| match line {
                Ok(str) if str.is_empty() => None,
                Ok(str) => Some(Command::parse(&str, use_aim)),
                Err(err) => Some(Err(err.into())),
            })
            .collect();
        Ok(Program(result?))
    }

    pub fn run(self) -> Result<()> {
        let file = File::open(self.input)?;
        let reader = BufReader::new(file);
        let program = Day2::parse_input(reader, self.use_aim)?;
        let result = program.run();
        println!("Position: \t{}\nDepth:\t\t{}", result.position, result.depth);
        if self.multiply_results {
            println!("Multiplied together: {}", result.position * result.depth);
        }
        Ok(())
    }
}

pub(crate) struct Program(Vec<Command>);

impl Program {
    pub(crate) fn run(&self) -> State {
        self.0.iter().fold(State::default(), |mut state, cmd| {
            cmd.apply(&mut state);
            state
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    const EXAMPLE_PROGRAM: &str = "forward 5\ndown 5\nforward 8\nup 3\ndown 8\nforward 2\n";

    #[test]
    fn example_1_has_the_expected_result() -> Result<()> {
        let expected_state = {
            let mut state = State::default();
            state.position = 15;
            state.depth = 10;
            state
        };
        let input = EXAMPLE_PROGRAM;
        let program = Day2::parse_input(input.as_bytes(), false)?;
        let result = program.run();
        assert_eq!(result, expected_state);
        Ok(())
    }

    #[test]
    fn example_2_has_the_expected_result() -> Result<()> {
        let expected_state = State { position: 15, depth: 60, aim: 10 };
        let input = EXAMPLE_PROGRAM;
        let program = Day2::parse_input(input.as_bytes(), true)?;
        let result = program.run();
        assert_eq!(result, expected_state);
        Ok(())
    }
}
