// SPDX-License-Identifier: GPL-3.0-only

use std::{
    borrow::Borrow,
    io::{BufRead, Error},
    marker::PhantomData,
};

use anyhow::Result;
use num_bigint::BigUint;
use num_traits::{One, Zero};

#[derive(Debug, PartialEq)]
pub(crate) struct Report<B: Borrow<Vec<I>>, I: Borrow<BigUint>> {
    data: B,
    width: usize,
    bi: PhantomData<I>,
}

impl<B: Borrow<Vec<I>>, I: Borrow<BigUint>> Report<B, I> {
    fn new(data: B) -> Report<B, I> {
        let width = data.borrow().iter().fold(0, |acc, b| {
            std::cmp::max(acc, b.borrow().to_u32_digits().len())
        });
        Report {
            data: data,
            width: width,
            bi: PhantomData::default(),
        }
    }

    pub(crate) fn extrema(&self) -> (usize, usize) {
        let xs = self.data.borrow();
        let ones_threshold = xs.len() / 2 - (1 - xs.len() % 2);
        xs.iter()
            .fold(BigUint::zero(), |lhs, rhs| lhs + rhs.borrow())
            .iter_u32_digits()
            .rev()
            .fold((0usize, 0usize), |(minimum, maximum), digit| {
                let digit = digit as usize;
                (
                    minimum << 1 | (digit <= ones_threshold) as usize,
                    maximum << 1 | (digit > ones_threshold) as usize,
                )
            })
    }

    pub(crate) fn power_consumption(&self) -> usize {
        let (gamma, epsilon) = self.extrema();
        gamma * epsilon
    }

    pub(crate) fn life_support_rating(&self) -> usize {
        let num_bits = self.width;
        let (oxygen, co2) = (0..num_bits).rev().fold(
            (
                self.data.borrow().iter().map(Borrow::borrow).collect(),
                self.data.borrow().iter().map(Borrow::borrow).collect(),
            ),
            |(oxygen, co2): (Vec<&BigUint>, Vec<&BigUint>), idx| {
                (
                    filter_numbers_meeting_criteria(oxygen, idx, false),
                    filter_numbers_meeting_criteria(co2, idx, true),
                )
            },
        );
        if oxygen.len() == 1 && co2.len() == 1 {
            decode(&oxygen[0]) * decode(&co2[0])
        } else {
            panic!("The life support rating did not converge.  Santa is doomed! ☠️ 🎅");
        }
    }
}

fn filter_numbers_meeting_criteria(
    src: Vec<&BigUint>,
    index: usize,
    use_minimum: bool,
) -> Vec<&BigUint> {
    if src.len() > 1 {
        let digits = Report {
            data: &src,
            width: 0,
            bi: PhantomData::default(),
        }
        .extrema();
        let digits = if use_minimum { digits.0 } else { digits.1 };
        let digit = BigUint::from((digits >> index) & 1);
        src.into_iter()
            .filter(|i| *i >> (32 * index) & &BigUint::one() == digit)
            .collect()
    } else {
        src
    }
}

fn decode(i: &BigUint) -> usize {
    i.iter_u32_digits()
        .rev()
        .fold(0, |acc, x| acc << 1 | x as usize)
}

pub(crate) trait ReportExt {
    fn parse(input: impl BufRead) -> Result<Report<Vec<BigUint>, BigUint>, Error>;
}

impl ReportExt for Report<Vec<BigUint>, BigUint> {
    fn parse(input: impl BufRead) -> Result<Report<Vec<BigUint>, BigUint>, Error> {
        let result: Result<Vec<_>, Error> = input
            .lines()
            .map(|s| Ok(BigUint::new(s?.chars().rev().map(to_u32).collect())))
            .collect();
        Ok(Report::new(result?))
    }
}

fn to_u32(ch: char) -> u32 {
    ch as u32 - '0' as u32
}

#[cfg(test)]
mod tests {
    use std::str::FromStr;

    use super::*;

    const INPUT: &str =
        "00100\n11110\n10110\n10111\n10101\n01111\n00111\n11100\n10000\n11001\n00010\n01010\n";

    #[test]
    fn report_parsing_single_number_is_just_that_number() -> Result<()> {
        let expected = Report::new(vec![BigUint::from_str("4294967296")?]);
        let result = Report::parse("10".as_bytes())?;
        assert_eq!(result, expected);
        Ok(())
    }

    #[test]
    fn report_parsing_two_numbers_are_those_number() -> Result<()> {
        let expected = Report::new(vec![
            BigUint::from_str("4294967296")?,
            BigUint::from_str("18446744073709551616")?,
        ]);
        let result = Report::parse("10\n100".as_bytes())?;
        assert_eq!(result, expected);
        Ok(())
    }

    #[test]
    fn report_querying_finds_the_minimum() -> Result<()> {
        let expected_minimum = 0b01001;
        let report = Report::parse(INPUT.as_bytes())?;
        let (minimum, _) = report.extrema();
        assert_eq!(minimum, expected_minimum);
        Ok(())
    }

    #[test]
    fn report_querying_finds_the_maximum() -> Result<()> {
        let expected_maximum = 0b10110;
        let report = Report::parse(INPUT.as_bytes())?;
        let (_, maximum) = report.extrema();
        assert_eq!(maximum, expected_maximum);
        Ok(())
    }

    #[test]
    fn report_querying_works_with_odd_length_inputs() -> Result<()> {
        let expected_extrema = (0b000, 0b111);
        let report = Report::parse("010\n101\n111".as_bytes())?;
        let result = report.extrema();
        assert_eq!(result, expected_extrema);
        Ok(())
    }

    #[test]
    fn report_querying_tie_breakers_go_to_1s() -> Result<()> {
        let expected_extrema = (0b000, 0b111);
        let report = Report::parse("010\n101".as_bytes())?;
        let result = report.extrema();
        assert_eq!(result, expected_extrema);
        Ok(())
    }

    #[test]
    fn example_1_power_consumption_is_198() -> Result<()> {
        let expected_consumption = 198;
        let report = Report::parse(INPUT.as_bytes())?;
        let result = report.power_consumption();
        assert_eq!(result, expected_consumption);
        Ok(())
    }

    #[test]
    fn example_2_life_support_rating_is_230() -> Result<()> {
        let expected_consumption = 230;
        let report = Report::parse(INPUT.as_bytes())?;
        let result = report.life_support_rating();
        assert_eq!(result, expected_consumption);
        Ok(())
    }
}
