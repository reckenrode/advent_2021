// SPDX-License-Identifier: GPL-3.0-only

use nom::{
    bytes::complete::tag,
    character::complete::digit1,
    combinator::{map, map_res},
    multi::separated_list1,
    IResult,
};

#[derive(Debug, Default, PartialEq)]
pub(crate) struct Draws(pub(super) Vec<u8>);

impl Draws {
    pub(crate) fn parse(input: &str) -> IResult<&str, Draws> {
        let number = map_res(digit1, |s| u8::from_str_radix(s, 10));
        let mut number_seq = map(separated_list1(tag(","), number), Draws);
        number_seq(input)
    }

    pub(crate) fn iter(&self) -> impl Iterator<Item = &u8> {
        self.0.iter()
    }
}

impl IntoIterator for Draws {
    type Item = u8;

    type IntoIter = <Vec<u8> as IntoIterator>::IntoIter;

    fn into_iter(self) -> Self::IntoIter {
        self.0.into_iter()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parser_when_the_input_is_empty_it_returns_an_error() {
        let result = Draws::parse("");
        assert_eq!(result.is_err(), true)
    }

    #[test]
    fn parser_when_the_input_is_wrong_it_returns_an_error() {
        let result = Draws::parse("a,b,c");
        assert_eq!(result.is_err(), true)
    }

    #[test]
    fn parser_when_the_input_is_a_list_it_returns_the_draws() {
        let expected_result = Ok(("", Draws(vec![1, 2, 3, 4])));
        let result = Draws::parse("1,2,3,4");
        assert_eq!(result, expected_result);
    }

    #[test]
    fn parser_when_the_input_has_more_it_returns_the_rest() {
        let expected_result = Ok(("\n\nbingo boards go here", Draws(vec![1, 2, 3, 4])));
        let result = Draws::parse("1,2,3,4\n\nbingo boards go here");
        assert_eq!(result, expected_result);
    }
}
