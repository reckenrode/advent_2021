// SPDX-License-Identifier: GPL-3.0-only

use std::cmp::{max, max_by_key, min, min_by_key};

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
pub(crate) struct CommandList(Vec<((u16, u16), (u16, u16))>);

impl CommandList {
    pub(crate) fn new() -> Self {
        CommandList(Vec::new())
    }

    pub(crate) fn parse(input: &str) -> Result<CommandList, Err<Error<&str>>> {
        fn coordinate_part(input: &str) -> IResult<&str, u16> {
            map_res(digit1, |src| u16::from_str_radix(src, 10))(input)
        }
        fn point(input: &str) -> IResult<&str, (u16, u16)> {
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
            .map(|(p1, p2)| CommandList::draw_line(*p1, *p2, bitmap))
            .collect()
    }

    pub(crate) fn required_bounds(&self) -> (u16, u16) {
        let xs = self
            .0
            .iter()
            .map(|((x, _), _)| x)
            .chain(self.0.iter().map(|(_, (x, _))| x));
        let ys = self
            .0
            .iter()
            .map(|((_, y), _)| y)
            .chain(self.0.iter().map(|(_, (_, y))| y));
        (*xs.max().unwrap_or(&0) + 1, *ys.max().unwrap_or(&0) + 1)
    }

    fn draw_line((x1, y1): (u16, u16), (x2, y2): (u16, u16), bitmap: &mut Bitmap) -> Result<()> {
        let (x1, y1) = (x1 as i32, y1 as i32);
        let (x2, y2) = (x2 as i32, y2 as i32);
        let ((x1, y1), (x2, y2)) = (
            min_by_key((x1, y1), (x2, y2), |(x, _)| *x),
            max_by_key((x1, y1), (x2, y2), |(x, _)| *x),
        );
        let m = (y2 - y1) as f64 / (x2 - x1) as f64;
        if m.abs() <= 1.0 {
            let y0 = y1 as f64 - m * x1 as f64;
            (x1..=x2)
                .map(|x| bitmap.draw(x as u16, (m * x as f64 + y0).round() as u16))
                .collect()
        } else if m.is_infinite() {
            let (y1, y2) = (min(y1, y2), max(y1, y2));
            (y1..=y2)
                .map(|y| bitmap.draw(x1 as u16, y as u16))
                .collect()
        } else {
            let m = 1.0 / m;
            let x0 = x1 as f64 - m * y1 as f64;
            (y1..=y2)
                .map(|y| bitmap.draw((m * y as f64 + x0).round() as u16, y as u16))
                .collect()
        }
    }
}

impl FromIterator<((u16, u16), (u16, u16))> for CommandList {
    fn from_iter<T: IntoIterator<Item = ((u16, u16), (u16, u16))>>(iter: T) -> Self {
        Vec::from_iter(iter).into()
    }
}

impl IntoIterator for CommandList {
    type Item = ((u16, u16), (u16, u16));

    type IntoIter = <Vec<((u16, u16), (u16, u16))> as IntoIterator>::IntoIter;

    fn into_iter(self) -> Self::IntoIter {
        self.0.into_iter()
    }
}

impl From<Vec<((u16, u16), (u16, u16))>> for CommandList {
    fn from(value: Vec<((u16, u16), (u16, u16))>) -> Self {
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

    #[test]
    fn command_list_with_a_backwards_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = "111\n...\n...";
        let input = CommandList::from(vec![((2, 0), (0, 0))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn command_list_with_a_backwards_vertical_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = "...\n..1\n..1";
        let input = CommandList::from(vec![((2, 2), (2, 1))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn command_list_with_a_downward_45_degree_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = "1..\n.1.\n..1";
        let input = CommandList::from(vec![((0, 0), (2, 2))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn command_list_with_a_upward_45_degree_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = "..1\n.1.\n1..";
        let input = CommandList::from(vec![((0, 2), (2, 0))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn command_list_with_an_accute_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = "1..\n.11\n...";
        let input = CommandList::from(vec![((0, 0), (2, 1))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn command_list_with_an_obtuse_line_draws_it_to_the_bitmap() -> Result<()> {
        let expected_result = "1..\n.1.\n.1.";
        let input = CommandList::from(vec![((0, 0), (1, 2))]);
        let mut bitmap = Bitmap::new(3, 3);
        input.apply_commands(&mut bitmap)?;
        let result = format!("{}", bitmap);
        assert_eq!(result, expected_result);
        Ok(())
    }
}
