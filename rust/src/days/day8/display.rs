// SPDX-License-Identifier: GPL-3.0-only

use std::{collections::HashMap, ops::BitOr};

use anyhow::{anyhow, Result};
use itertools::Itertools;
use nom::{
    bytes::complete::{tag, take_while_m_n},
    character::complete::space1,
    combinator::map_res,
    sequence::{preceded, separated_pair, tuple},
    IResult,
};
use phf::phf_map;

static SEGMENT_MAPPING: phf::Map<u8, u8> = phf_map! {
    b'a' => 0b0000001,
    b'b' => 0b0000010,
    b'c' => 0b0000100,
    b'd' => 0b0001000,
    b'e' => 0b0010000,
    b'f' => 0b0100000,
    b'g' => 0b1000000,
};

fn to_repr(segments: &[u8]) -> u8 {
    segments
        .iter()
        .map(|segment| SEGMENT_MAPPING[segment])
        .fold(0, BitOr::bitor)
}

#[derive(Debug, PartialEq)]
pub(crate) struct Display {
    output: [u8; 4],
}

impl Display {
    fn parser<'a>(input: &'a [u8]) -> IResult<&'a [u8], ([&'a [u8]; 10], [&'a [u8]; 4])> {
        fn from_vec<'a>(
            (digits, output): (Vec<&'a [u8]>, Vec<&'a [u8]>),
        ) -> Result<([&'a [u8]; 10], [&'a [u8]; 4])> {
            Ok((digits.as_slice().try_into()?, output.as_slice().try_into()?))
        }

        fn is_segment(digit: u8) -> bool {
            b'a' <= digit && b'g' >= digit
        }

        fn separated_list_n<'a>(
            n: usize,
            mut sep: impl FnMut(&'a [u8]) -> IResult<&'a [u8], &'a [u8]>,
            mut f: impl FnMut(&'a [u8]) -> IResult<&'a [u8], &'a [u8]>,
        ) -> impl FnMut(&'a [u8]) -> IResult<&'a [u8], Vec<&'a [u8]>> {
            move |input| {
                let (input, head) = {
                    let head = &mut f;
                    head(input)
                }?;
                let mut result = vec![head];
                let mut rest = preceded(&mut sep, &mut f);
                let output = (1..n).fold(Ok(input), |input, _| {
                    let (output, rest) = rest(input?)?;
                    result.push(rest);
                    Ok(output)
                })?;
                Ok((output, result))
            }
        }

        let segments = take_while_m_n(2, 7, is_segment);
        let digits = separated_list_n(10, space1, &segments);
        let output = separated_list_n(4, space1, &segments);
        let mut line = map_res(
            separated_pair(digits, tuple((space1, tag(b"|"), space1)), output),
            from_vec,
        );
        line(input)
    }

    pub(crate) fn parse(input: &str) -> Result<Display> {
        let (_, (digits, output)) =
            Self::parser(input.as_bytes()).map_err(|err| anyhow!("{}", err))?;
        Ok(Display::decode(digits, output))
    }

    fn decode(digits: [&[u8]; 10], output: [&[u8]; 4]) -> Display {
        let mapping: HashMap<_, _> = digits
            .into_iter()
            .sorted_by_key(|key| key.len())
            .fold(
                HashMap::<u8, u8>::with_capacity(10),
                |mut mapping, segments| {
                    let bit_repr = to_repr(segments);
                    let number = match segments {
                        x if x.len() == 2 => 1,
                        x if x.len() == 5
                            && ((bit_repr | 0b10000000) & !mapping[&4]) == !mapping[&4] =>
                        {
                            2
                        }
                        x if x.len() == 5 && (bit_repr & mapping[&1]) == mapping[&1] => 3,
                        x if x.len() == 4 => 4,
                        x if x.len() == 5
                            && (bit_repr & (mapping[&4] ^ mapping[&7]))
                                == (mapping[&4] ^ mapping[&7]) =>
                        {
                            5
                        }
                        x if x.len() == 6
                            && ((bit_repr | 0b10000000) & !mapping[&7]) == !mapping[&7] =>
                        {
                            6
                        }
                        x if x.len() == 3 => 7,
                        x if x.len() == 7 => 8,
                        x if x.len() == 6 && (bit_repr & mapping[&4]) == mapping[&4] => 9,
                        _ => 0,
                    };
                    mapping.insert(number, bit_repr);
                    mapping
                },
            )
            .into_iter()
            .map(|(k, v)| (v, k))
            .collect();
        Display {
            output: {
                let mut mapped_output = [0; 4];
                for (index, number) in output.into_iter().map(to_repr).enumerate() {
                    mapped_output[index] = mapping[&number];
                }
                mapped_output
            },
        }
    }
}

impl std::fmt::Display for Display {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_fmt(format_args!("{}", self.output.iter().join("")))
    }
}

impl From<Display> for u32 {
    fn from(display: Display) -> Self {
        display
            .output
            .into_iter()
            .fold(0, |acc, digit| acc * 10 + digit as u32)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn display_decodes_one_digit() -> Result<()> {
        let expected_string = "1111";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| ab ab ab ab"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_two_digit() -> Result<()> {
        let expected_string = "2222";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| gcdfa gcdfa gcdfa gcdfa"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_three_digit() -> Result<()> {
        let expected_string = "3333";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| fbcad fbcad fbcad fbcad"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_four_digit() -> Result<()> {
        let expected_string = "4444";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| eafb eafb eafb eafb"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_five_digit() -> Result<()> {
        let expected_string = "5555";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| cdfbe cdfbe cdfbe cdfbe"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_six_digit() -> Result<()> {
        let expected_string = "6666";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| cdfgeb cdfgeb cdfgeb cdfgeb"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_seven_digit() -> Result<()> {
        let expected_string = "7777";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| dab dab dab dab"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_eight_digit() -> Result<()> {
        let expected_string = "8888";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| acedgfb acedgfb acedgfb acedgfb"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_nine_digit() -> Result<()> {
        let expected_string = "9999";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| cefabd cefabd cefabd cefabd"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn display_decodes_zero_digit() -> Result<()> {
        let expected_string = "0000";
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| cagedb cagedb cagedb cagedb"
        );
        let display = Display::parse(input)?;
        assert_eq!(format!("{}", display), expected_string);
        Ok(())
    }

    #[test]
    fn it_converts_into_u32() -> Result<()> {
        let expected_number = 9999;
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| cefabd cefabd cefabd cefabd"
        );
        let display = Display::parse(input)?;
        assert_eq!(u32::from(display), expected_number);
        Ok(())
    }
}
