// SPDX-License-Identifier: GPL-3.0-only

use std::cmp::{max, min};

use anyhow::Result;
use nom::{
    branch::alt,
    bytes::complete::tag,
    character::complete::{digit1, space0},
    combinator::{eof, map, map_res},
    error::Error,
    multi::separated_list1,
    sequence::{delimited, separated_pair},
    Err, IResult,
};

use super::bitmap::Bitmap;

#[derive(Debug, PartialEq)]
pub(crate) struct CommandList(Vec<((usize, usize), (usize, usize))>);

impl CommandList {
    pub(crate) fn new() -> Self {
        CommandList(Vec::new())
    }

    pub(crate) fn parse(input: &str) -> Result<CommandList, Err<Error<&str>>> {
        fn coordinate_part(input: &str) -> IResult<&str, usize> {
            map_res(digit1, |src| usize::from_str_radix(src, 10))(input)
        }
        fn point(input: &str) -> IResult<&str, (usize, usize)> {
            separated_pair(coordinate_part, tag(","), coordinate_part)(input)
        }
        let arrow = tag("->");
        let command = separated_pair(point, delimited(space0, arrow, space0), point);
        let mut command_list = alt((
            map(separated_list1(tag("\n"), command), CommandList),
            map(eof, |_| CommandList::new()),
        ));
        let (_, command_list) = command_list(input)?;
        Ok(command_list)
    }

    pub(crate) fn apply_commands(&self, bitmap: &mut Bitmap) -> Result<()> {
        self.0
            .iter()
            .map(|command| match command {
                (p1 @ (x1, _), p2 @ (x2, _)) if x1 == x2 => {
                    CommandList::draw_vertical(*p1, *p2, bitmap)
                }
                (p1 @ (_, y1), p2 @ (_, y2)) if y1 == y2 => {
                    CommandList::draw_horizontal(*p1, *p2, bitmap)
                }
                _ => Ok(()),
            })
            .collect()
    }

    pub(crate) fn required_bounds(&self) -> (usize, usize) {
        let xs = self.0.iter().map(|((x, _), _)| x).chain(self.0.iter().map(|(_, (x, _))| x));
        let ys = self.0.iter().map(|((_, y), _)| y).chain(self.0.iter().map(|(_, (_, y))| y));
        (*xs.max().unwrap_or(&0) + 1, *ys.max().unwrap_or(&0) + 1)
    }

    fn draw_horizontal(
        (x1, y): (usize, usize),
        (x2, _): (usize, usize),
        bitmap: &mut Bitmap,
    ) -> Result<()> {
        let start = min(x1, x2);
        let stop = max(x1, x2);
        (start..=stop).map(|x| bitmap.draw(x, y)).collect()
    }

    fn draw_vertical(
        (x, y1): (usize, usize),
        (_, y2): (usize, usize),
        bitmap: &mut Bitmap,
    ) -> Result<()> {
        let start = min(y1, y2);
        let stop = max(y1, y2);
        (start..=stop).map(|y| bitmap.draw(x, y)).collect()
    }
}

impl From<Vec<((usize, usize), (usize, usize))>> for CommandList {
    fn from(value: Vec<((usize, usize), (usize, usize))>) -> Self {
        CommandList(value)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parse_returns_nothing_with_an_empty_string() -> Result<()> {
        let expected_list = CommandList::new();
        let list = CommandList::parse("")?;
        assert_eq!(list, expected_list);
        Ok(())
    }

    #[test]
    fn parse_with_one_item_returns_a_list_with_one_item() -> Result<()> {
        let expected_list = CommandList::new();
        let list = CommandList::parse("")?;
        assert_eq!(list, expected_list);
        Ok(())
    }

    #[test]
    fn parse_with_multiple_items_returns_a_list_with_those_items() -> Result<()> {
        let expected_list = CommandList::from(vec![((1, 1), (2, 3)), ((5, 8), (13, 21))]);
        let list = CommandList::parse("1,1->2,3\n5,8->13,21")?;
        assert_eq!(list, expected_list);
        Ok(())
    }

    #[test]
    fn parse_rejects_invalid_input() {
        let list = CommandList::parse("C is for cookie, and that’s good enough for me.");
        assert_eq!(list.is_err(), true);
    }

    #[test]
    fn bounds_returns_area_needed_to_draw_all_commands() -> Result<()> {
        let expected_bounds = (640, 480);
        let list = CommandList::parse("639,2->0,2\n43,479->480,43")?;
        let bounds = list.required_bounds();
        assert_eq!(bounds, expected_bounds);
        Ok(())
    }

    #[test]
    fn command_list_with_a_horizontal_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = "111\n...\n...";
        let input = CommandList::from(vec![((0, 0), (2, 0))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn command_list_with_a_vertical_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = ".1.\n.1.\n...";
        let input = CommandList::from(vec![((1, 0), (1, 1))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }
}
