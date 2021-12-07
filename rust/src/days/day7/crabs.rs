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
    pub(crate) fn lowest_cost_target_position(&self, crab_engineering: bool) -> (i32, i32) {
        if crab_engineering {
            self.find_cheapest(
                0,
                self.calculate_candidates(|num, new_position| {
                    let delta = (num - new_position).abs();
                    (delta * delta + delta) / 2
                }),
            )
        } else {
            self.find_cheapest(
                0,
                self.calculate_candidates(|num, new_position| (num - new_position).abs()),
            )
        }
    }

    fn calculate_candidates<'a>(
        &'a self,
        f: impl Fn(i32, i32) -> i32 + 'a,
    ) -> impl Fn(i32) -> (i32, i32, i32) + 'a {
        move |num: i32| {
            (
                self.cost_to_move(num - 1, &f),
                self.cost_to_move(num, &f),
                self.cost_to_move(num + 1, &f),
            )
        }
    }

    fn find_cheapest(&self, start: i32, f: impl Fn(i32) -> (i32, i32, i32)) -> (i32, i32) {
        let (left, current, right) = f(start);
        match (left.cmp(&current), right.cmp(&current)) {
            (Ordering::Greater, Ordering::Greater) => (start, current),
            (Ordering::Less, _) => self.find_cheapest(start - 1, f),
            (_, Ordering::Less) => self.find_cheapest(start + 1, f),
            _ => panic!("The problem fails to converge, which is bad for Santa.  ☠️ 🎅"),
        }
    }

    fn cost_to_move(&self, new_position: i32, f: impl Fn(i32, i32) -> i32) -> i32 {
        self.0.iter().fold(0, |sum, x| sum + f(*x, new_position))
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
        let (position, _) = positions.lowest_cost_target_position(false);
        assert_eq!(position, expected_position);
    }

    #[test]
    fn solver_calculates_the_fuel_cost_to_move() {
        let expected_cost = 37;
        let positions = Positions::from([16, 1, 2, 0, 4, 2, 7, 1, 2, 14]);
        let (_, cost) = positions.lowest_cost_target_position(false);
        assert_eq!(cost, expected_cost);
    }

    #[test]
    fn solver_supports_crab_engineering() {
        let expected_result = (5, 168);
        let positions = Positions::from([16, 1, 2, 0, 4, 2, 7, 1, 2, 14]);
        let result = positions.lowest_cost_target_position(true);
        assert_eq!(result, expected_result);
    }
}
