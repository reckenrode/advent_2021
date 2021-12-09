// SPDX-License-Identifier: GPL-3.0-only

use anyhow::{anyhow, Result};
use nom::{
    branch::alt,
    character::complete::{digit1, line_ending, one_of},
    combinator::{eof, map, map_opt},
    multi::{many_m_n, separated_list1},
    sequence::{preceded, terminated},
    IResult,
};

#[derive(Debug, PartialEq)]
pub(crate) struct Heightmap {
    grid: Vec<Vec<u32>>,
    width: usize,
    height: usize,
}

pub(crate) struct Neighbors {
    pub(crate) top: Option<u32>,
    pub(crate) bottom: Option<u32>,
    pub(crate) left: Option<u32>,
    pub(crate) right: Option<u32>,
}

impl Heightmap {
    fn heightmap(input: &str) -> IResult<&str, Heightmap> {
        let (_, first_row) = digit1(input)?;
        let width = first_row.len();
        let row = many_m_n(
            width,
            width,
            map_opt(one_of("0123456789"), |ch| ch.to_digit(10)),
        );
        let mut grid = terminated(
            map(separated_list1(line_ending, row), |grid| Heightmap {
                height: *&grid.len(),
                grid,
                width,
            }),
            alt((eof, preceded(line_ending, eof))),
        );
        grid(input)
    }

    pub(crate) fn parse(input: &str) -> Result<Heightmap> {
        let (_, heightmap) = Self::heightmap(input).map_err(|err| anyhow!("{}", err))?;
        Ok(heightmap)
    }

    pub(crate) fn filter<'a, 'b>(
        &'a self,
        f: impl Fn(u32, Neighbors) -> bool + 'b,
    ) -> impl Iterator<Item = (usize, usize, &'a u32)> + 'a
    where
        'b: 'a,
    {
        self.grid
            .iter()
            .enumerate()
            .flat_map(|(row, values)| {
                values
                    .iter()
                    .enumerate()
                    .map(move |(column, value)| (row, column, value))
            })
            .filter_map(move |result @ (row, column, value)| {
                f(
                    *value,
                    Neighbors {
                        top: if row == 0 {
                            None
                        } else {
                            Some(self.grid[row - 1][column])
                        },
                        bottom: if row == self.height - 1 {
                            None
                        } else {
                            Some(self.grid[row + 1][column])
                        },
                        left: if column == 0 {
                            None
                        } else {
                            Some(self.grid[row][column - 1])
                        },
                        right: if column == self.width - 1 {
                            None
                        } else {
                            Some(self.grid[row][column + 1])
                        },
                    },
                )
                .then(|| result)
            })
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parse_produces_lines_of_numbers() -> Result<()> {
        let expected_output = vec![vec![1, 2, 3, 4, 5], vec![6, 7, 8, 9, 0]];
        let output = Heightmap::parse("12345\n67890\n")?;
        assert_eq!(output.grid, expected_output);
        Ok(())
    }

    #[test]
    fn filter_applies_the_filter() -> Result<()> {
        let expected_output = vec![(0usize, 0usize, &1u32)];
        let heightmap = Heightmap::parse("1\n2")?;
        let output = heightmap.filter(|value, _| value == 1);
        assert_eq!(output.collect::<Vec<_>>(), expected_output);
        Ok(())
    }

    #[test]
    fn filter_provides_neighbors() -> Result<()> {
        let expected_output = vec![(1usize, 1usize, &1u32)];
        let heightmap = Heightmap::parse("111\n111\n111")?;
        let output = heightmap.filter(|value, neighbors| match neighbors {
            Neighbors {
                top: Some(top),
                bottom: Some(bottom),
                left: Some(left),
                right: Some(right),
            } => top == 1 && bottom == 1 && left == 1 && right == 1 && value == 1,
            _ => false,
        });
        assert_eq!(output.collect::<Vec<_>>(), expected_output);
        Ok(())
    }
}
