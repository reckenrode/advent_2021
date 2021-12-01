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
        let increases = count_increases(reader)?;
        println!("Times a depth measurement increased: {}", increases);
        Ok(())
    }
}

fn count_increases(reader: impl BufRead) -> Result<i32> {
    let lines: Vec<i32> = reader
        .lines()
        .map(|line| Ok(line?.parse()?))
        .collect::<Result<_>>()?;
    let pairs = lines[..].iter().zip(&lines[1..]);
    let count = pairs.fold(0, |acc, (cur, next)| if next > cur { acc + 1 } else { acc });
    Ok(count)
}

#[cfg(test)]
mod tests {
    use super::*;

    const EXAMPLE_INPUT: &str = "199\n200\n208\n210\n200\n207\n240\n269\n260\n263\n";

    #[test]
    fn example_1_has_seven_increases() -> Result<()> {
        let expected_increases = 7;
        assert_eq!(
            count_increases(EXAMPLE_INPUT.as_bytes())?,
            expected_increases
        );
        Ok(())
    }
}
