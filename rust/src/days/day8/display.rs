// SPDX-License-Identifier: GPL-3.0-only

use anyhow::{anyhow, Result};
use nom::{
    bytes::complete::{tag, take_while_m_n},
    character::complete::space1,
    combinator::map_res,
    sequence::{preceded, separated_pair, tuple},
    IResult,
};

#[derive(Debug, PartialEq)]
pub(crate) struct Display<'a> {
    digits: [&'a [u8]; 10],
    output: [&'a [u8]; 4],
}

impl<'a> Display<'a> {
    fn parser(input: &'a [u8]) -> IResult<&'a [u8], Self> {
        fn from_vec<'a>((digits, output): (Vec<&'a [u8]>, Vec<&'a [u8]>)) -> Result<Display<'a>> {
            Ok(Display {
                digits: digits.as_slice().try_into()?,
                output: output.as_slice().try_into()?,
            })
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

    pub(crate) fn parse(input: &'a str) -> Result<Display<'a>> {
        let (_, display) = Self::parser(input.as_bytes()).map_err(|err| anyhow!("{}", err))?;
        Ok(display)
    }

    fn decode_digit(&self, digit: &[u8]) -> u8 {
        match digit {
            x if x.len() == 2 => b'1',
            x if x.len() == 3 => b'7',
            x if x.len() == 4 => b'4',
            x if x.len() == 7 => b'8',
            _ => b'?',
        }
    }
}

impl<'a> std::fmt::Display for Display<'a> {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let digits = unsafe {
            // safety: `Display::decode` digit returns only valid UTF-8 characters, so the check
            //         can be omitted.
            String::from_utf8_unchecked(
                self.output
                    .iter()
                    .map(|digit| self.decode_digit(digit))
                    .collect(),
            )
        };
        f.write_fmt(format_args!("{}", digits))
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parses_the_input_into_a_display() -> Result<()> {
        let expected_display = Display {
            digits: [
                b"acedgfb", b"cdfbe", b"gcdfa", b"fbcad", b"dab", b"cefabd", b"cdfgeb", b"eafb",
                b"cagedb", b"ab",
            ],
            output: [b"cdfeb", b"fcadb", b"cdfeb", b"cdbaf"],
        };
        let input = concat!(
            "acedgfb cdfbe gcdfa fbcad dab cefabd cdfgeb eafb cagedb ab ",
            "| cdfeb fcadb cdfeb cdbaf"
        );
        let display = Display::parse(input)?;
        assert_eq!(display, expected_display);
        Ok(())
    }

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
}
