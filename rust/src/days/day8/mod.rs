// SPDX-License-Identifier: GPL-3.0-only

mod display;

use std::path::PathBuf;

use anyhow::Result;
use clap::Parser;
use itertools::Itertools;

use crate::{days::day8::display::Display, util::read_input};

#[derive(Parser)]
#[clap(about = "Seven Segment Search")]
pub(crate) struct Day8 {
    input: PathBuf,
}

impl Day8 {
    pub(crate) fn run(self) -> Result<()> {
        let input = read_input(self.input)?;
        let displays = Self::parse_displays(input.as_str())?;

        let digit_counts: usize = displays
            .iter()
            .map(|display| -> usize {
                format!("{}", display)
                    .as_bytes()
                    .into_iter()
                    .counts()
                    .into_iter()
                    .filter_map(|(digit, count)| {
                        if *digit == b'1' || *digit == b'4' || *digit == b'7' || *digit == b'8' {
                            Some(count)
                        } else {
                            None
                        }
                    })
                    .sum()
            })
            .sum();
        println!("The count of digits [1, 4, 7, 8] on all the displays: {}", digit_counts);

        Ok(())
    }

    fn parse_displays<'a>(input: &'a str) -> Result<Vec<Display<'a>>> {
        input.lines().map(Display::parse).collect()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parses_the_input() -> Result<()> {
        let expected_displays = vec![
            Display::parse(concat!(
                "be cfbegad cbdgef fgaecd cgeb fdcge agebfd fecdb fabcd edb | ",
                "fdgacbe cefdb cefbgd gcbe\n"
            ))?,
            Display::parse(concat!(
                "edbfga begcd cbg gc gcadebf fbgde acbgfd abcde gfcbed gfec | ",
                "fcgedb cgb dgebacf gc"
            ))?,
        ];
        let input = concat!(
            "be cfbegad cbdgef fgaecd cgeb fdcge agebfd fecdb fabcd edb | ",
            "fdgacbe cefdb cefbgd gcbe\n",
            "edbfga begcd cbg gc gcadebf fbgde acbgfd abcde gfcbed gfec | ",
            "fcgedb cgb dgebacf gc\n",
        );
        let display = Day8::parse_displays(input)?;
        assert_eq!(display, expected_displays);
        Ok(())
    }
}
