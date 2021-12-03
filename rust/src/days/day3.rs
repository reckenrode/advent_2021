// SPDX-License-Identifier: GPL-3.0-only

use std::{
    borrow::Borrow,
    fs::File,
    io::{BufRead, BufReader},
    iter::repeat,
    path::PathBuf,
};

use anyhow::Result;
use clap::Parser;

#[derive(Parser)]
#[clap(about = "Submarine diagnostics")]
pub(crate) struct Day3 {
    input: PathBuf,
}

impl Day3 {
    pub(crate) fn run(self) -> Result<()> {
        let stream = File::open(self.input)?;
        let reader = BufReader::new(stream);
        let report = read_diagnostic_report(reader)?;

        let (gamma, epsilon) = power_consumption(&report);
        println!("Gamma: {}, Epsilon: {}", gamma, epsilon);
        println!("Power Consumption: {}", gamma * epsilon);

        let (oxygen_rating, scrubber_rating) = life_support_ratings(&report);
        println!(
            "Oxygen Rating: {}, CO₂ Scrubber Rating: {}",
            oxygen_rating, scrubber_rating
        );
        println!("Life Support Rating: {}", oxygen_rating * scrubber_rating);

        Ok(())
    }
}

fn read_diagnostic_report(report: impl BufRead) -> Result<Vec<String>, std::io::Error> {
    let report: Result<Vec<String>, std::io::Error> = report.lines().collect();
    report
}

fn decode_diagnostic_report(report: &[impl Borrow<String>]) -> Vec<bool> {
    let one_threshold = report.len() as f64 / 2f64;
    report
        .iter()
        .fold(
            &mut repeat(0usize).take(report[0].borrow().len()).collect(),
            |gamma_raw: &mut Vec<_>, line| {
                let modifiers = line.borrow().chars().map(|ch| (ch == '1') as usize);
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
        .map(|digit| *digit as f64 >= one_threshold)
        .collect()
}

fn power_consumption(report: &[String]) -> (usize, usize) {
    decode_diagnostic_report(report)
        .iter()
        .fold((0, 0), |(gamma, epsilon), digit| {
            (
                gamma << 1 | *digit as usize,
                epsilon << 1 | !*digit as usize,
            )
        })
}

fn life_support_ratings(raw_report: &[String]) -> (usize, usize) {
    let mut oxygen_candidates: Vec<_> = raw_report.into_iter().collect();
    let mut scrubber_candidates: Vec<_> = raw_report.into_iter().collect();
    for index in 0..raw_report[0].len() {
        if oxygen_candidates.len() > 1 {
            let decoded_report = decode_diagnostic_report(&oxygen_candidates);
            oxygen_candidates = filter_candidates(&decoded_report, &oxygen_candidates, index, '1');
        }
        if scrubber_candidates.len() > 1 {
            let decoded_report = decode_diagnostic_report(&scrubber_candidates);
            scrubber_candidates =
                filter_candidates(&decoded_report, &scrubber_candidates, index, '0');
        }
    }
    if oxygen_candidates.len() > 1 || scrubber_candidates.len() > 1 {
        panic!("The life support did not converge.  Santa is doomed.");
    } else {
        (
            usize::from_str_radix(oxygen_candidates[0], 2)
                .expect("the string had non-binary digits"),
            usize::from_str_radix(scrubber_candidates[0], 2)
                .expect("the string had non-binary digits"),
        )
    }
}

fn filter_candidates<'a>(
    decoded_report: &[bool],
    candidates: &[&'a String],
    index: usize,
    ch: char,
) -> Vec<&'a String> {
    candidates
        .into_iter()
        .filter(|str| {
            (str.chars().nth(index).expect("ran out of digits") == ch) == decoded_report[index]
        })
        .cloned()
        .collect()
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
            power_consumption(&read_diagnostic_report(INPUT.as_bytes())?);
        assert_eq!(gamma_rate * epsilon_rate, expected_consumption);
        Ok(())
    }

    #[test]
    fn example_2_oxygen_rating_is_23() -> Result<()> {
        let expected_rating = 23;
        let report = read_diagnostic_report(INPUT.as_bytes())?;
        let (oxygen_rating, _) = life_support_ratings(&report);
        assert_eq!(oxygen_rating, expected_rating);
        Ok(())
    }

    #[test]
    fn example_2_scrubber_rating_is_10() -> Result<()> {
        let expected_rating = 10;
        let report = read_diagnostic_report(INPUT.as_bytes())?;
        let (_, scrubber_rating) = life_support_ratings(&report);
        assert_eq!(scrubber_rating, expected_rating);
        Ok(())
    }
}
