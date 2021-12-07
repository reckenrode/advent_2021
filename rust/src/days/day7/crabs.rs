// SPDX-License-Identifier: GPL-3.0-only

use std::cmp::Ordering;

use anyhow::anyhow;
use nom::{
    bytes::complete::tag,
    character::complete::digit1,
    combinator::{map, map_res},
    multi::separated_list1,
    IResult,
};

#[derive(Debug, PartialEq)]
pub(crate) struct Positions(Vec<i32>);

impl Positions {
    pub(crate) fn lowest_cost_target_position(&self) -> i32 {
        self.find_cheapest(0)
    }

    fn calculate_candidates(&self, num: i32) -> (i32, i32, i32) {
        (
            self.cost_to_move(num - 1),
            self.cost_to_move(num),
            self.cost_to_move(num + 1),
        )
    }

    fn find_cheapest(&self, start: i32) -> i32 {
        let (left, current, right) = self.calculate_candidates(start);
        match (left.cmp(&current), right.cmp(&current)) {
            (Ordering::Greater, Ordering::Greater) => start,
            (Ordering::Less, _) => self.find_cheapest(start - 1),
            (_, Ordering::Less) => self.find_cheapest(start + 1),
            _ => panic!("The problem fails to converge, which is bad for Santa.  ☠️ 🎅"),
        }
    }

    pub(crate) fn cost_to_move(&self, new_position: i32) -> i32 {
        self.0
            .iter()
            .fold(0, |sum, x| sum + (x - new_position).abs())
    }

    pub(crate) fn parse(input: &str) -> IResult<&str, Positions> {
        let number = map_res(digit1, |src| i32::from_str_radix(src, 10));
        let numbers = separated_list1(tag(","), number);
        map(numbers, Positions::from)(input)
    }
}

impl<const N: usize> From<[i32; N]> for Positions {
    fn from(value: [i32; N]) -> Self {
        Positions(value.into_iter().collect())
    }
}

impl TryFrom<&str> for Positions {
    type Error = anyhow::Error;

    fn try_from(input: &str) -> Result<Self, Self::Error> {
        let (_, result) = Self::parse(input).map_err(|err| anyhow!("{}", err))?;
        Ok(result)
    }
}

impl From<Vec<i32>> for Positions {
    fn from(value: Vec<i32>) -> Self {
        Positions(value)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parser_parses_the_input() {
        let expected_positions = Ok(("", Positions::from([16, 1, 2, 0, 4, 2, 7, 1, 2, 14])));
        let positions = Positions::parse("16,1,2,0,4,2,7,1,2,14");
        assert_eq!(positions, expected_positions);
    }

    #[test]
    fn solver_finds_the_cheapest_position() {
        let expected_position = 2;
        let positions = Positions::from([16, 1, 2, 0, 4, 2, 7, 1, 2, 14]);
        let position = positions.lowest_cost_target_position();
        assert_eq!(position, expected_position);
    }

    #[test]
    fn solver_calculates_the_fuel_cost_to_move() {
        let expected_cost = 37;
        let positions = Positions::from([16, 1, 2, 0, 4, 2, 7, 1, 2, 14]);
        let cost = positions.cost_to_move(2);
        assert_eq!(cost, expected_cost);
    }
}
