// SPDX-License-Identifier: GPL-3.0-only

use std::{
    fs::File,
    io::{BufRead, BufReader},
    path::PathBuf,
};

use anyhow::Result;
use clap::Parser;

#[derive(Parser)]
#[clap(about = "Santa is doing something")]
pub(crate) struct Day1 {
    input: PathBuf,
}

impl Day1 {
    pub(crate) fn run(self) -> Result<()> {
        let file = File::open(self.input)?;
        let reader = BufReader::new(file);
        let lines = parse_lines(reader)?;

        let increases = count_increases(&lines);
        println!("Times a depth measurement increased: {}", increases);

        let increases = count_sliding_window_increases(&lines);
        println!(
            "Times the sliding window measurement increased: {}",
            increases
        );
        Ok(())
    }
}

fn parse_lines(reader: impl BufRead) -> Result<Vec<i32>> {
    reader.lines().map(|line| Ok(line?.parse()?)).collect()
}

fn count_increases(lines: &[i32]) -> i32 {
    let pairs = lines[..].iter().zip(&lines[1..]);
    pairs.fold(0, |acc, (cur, next)| if next > cur { acc + 1 } else { acc })
}

fn count_sliding_window_increases(lines: &[i32]) -> i32 {
    let windowed_values: Vec<i32> = lines[..]
        .iter()
        .zip(&lines[1..])
        .zip(&lines[2..])
        .map(|((x, y), z)| x + y + z)
        .collect();
    let pairs = windowed_values[..].iter().zip(&windowed_values[1..]);
    pairs.fold(0, |acc, (cur, next)| if next > cur { acc + 1 } else { acc })
}

#[cfg(test)]
mod tests {
    use super::*;

    const EXAMPLE_INPUT: [i32; 10] = [199, 200, 208, 210, 200, 207, 240, 269, 260, 263];

    #[test]
    fn example_1_has_seven_increases() -> Result<()> {
        let expected_increases = 7;
        assert_eq!(count_increases(&EXAMPLE_INPUT), expected_increases);
        Ok(())
    }

    #[test]
    fn example_2_has_5_increases() -> Result<()> {
        let expected_increases = 5;
        assert_eq!(
            count_sliding_window_increases(&EXAMPLE_INPUT),
            expected_increases
        );
        Ok(())
    }
}
