// SPDX-License-Identifier: GPL-3.0-only

use std::{
    fs::File,
    io::{BufRead, BufReader},
    iter::repeat,
    path::PathBuf,
};

use anyhow::Result;
use clap::Parser;

#[derive(Parser)]
#[clap(about = "Submarine power consumption")]
pub(crate) struct Day3 {
    input: PathBuf,
}

impl Day3 {
    pub(crate) fn run(self) -> Result<()> {
        let stream = File::open(self.input)?;
        let reader = BufReader::new(stream);
        let report = decode_diagnostic_report(reader)?;
        let (gamma, epsilon) = power_consumption(&report);
        println!("Gamma: {}, Epsilon: {}", gamma, epsilon);
        println!("Power Consumption: {}", gamma * epsilon);
        Ok(())
    }
}

fn decode_diagnostic_report(report: impl BufRead) -> Result<Vec<bool>> {
    let report = report
        .lines()
        .collect::<Result<Vec<String>, std::io::Error>>()?;
    let one_threshold = report.len() / 2;
    let result = report
        .iter()
        .fold(
            &mut repeat(0usize).take(report[0].len()).collect(),
            |gamma_raw: &mut Vec<_>, line| {
                let modifiers = line.chars().map(|ch| (ch == '1') as usize);
                gamma_raw
                    .iter_mut()
                    .zip(modifiers)
                    .for_each(|(element, modifier)| {
                        *element += modifier;
                    });
                gamma_raw
            },
        )
        .iter()
        .map(|digit| *digit > one_threshold)
        .collect();
    Ok(result)
}

fn power_consumption(report: &[bool]) -> (usize, usize) {
    report.iter().fold((0, 0), |(gamma, epsilon), digit| {
        (gamma << 1 | *digit as usize, epsilon << 1 | !*digit as usize)
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    const INPUT: &str = concat!(
        "00100\n", "11110\n", "10110\n", "10111\n", "10101\n", "01111\n", "00111\n", "11100\n",
        "10000\n", "11001\n", "00010\n", "01010\n",
    );

    #[test]
    fn example_1_power_consumption_is_198() -> Result<()> {
        let expected_consumption = 198;
        let (gamma_rate, epsilon_rate) =
            power_consumption(&decode_diagnostic_report(INPUT.as_bytes())?);
        assert_eq!(gamma_rate * epsilon_rate, expected_consumption);
        Ok(())
    }
}
