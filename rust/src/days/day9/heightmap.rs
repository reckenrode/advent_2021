// SPDX-License-Identifier: GPL-3.0-only

use std::collections::HashSet;

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

#[derive(Debug, Hash, Eq, PartialEq)]
pub(crate) struct Point {
    pub(crate) row: usize,
    pub(crate) column: usize,
    pub(crate) value: u32,
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
    ) -> impl Iterator<Item = Point> + 'a
    where
        'b: 'a,
    {
        self.grid
            .iter()
            .enumerate()
            .flat_map(|(row, values)| {
                values.iter().enumerate().map(move |(column, value)| Point {
                    row,
                    column,
                    value: *value,
                })
            })
            .filter_map(move |result @ Point { row, column, value }| {
                f(
                    value,
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

    fn neighbors<'a>(&'a self, pt: &Point) -> impl Iterator<Item = Point> + 'a {
        [
            (pt.row.wrapping_sub(1), pt.column),
            (pt.row + 1, pt.column),
            (pt.row, pt.column.wrapping_sub(1)),
            (pt.row, pt.column + 1),
        ]
        .into_iter()
        .filter_map(|(row, column)| {
            if row != usize::MAX && row < self.height && column != usize::MAX && column < self.width
            {
                Some(Point {
                    row,
                    column,
                    value: self.grid[row][column],
                })
            } else {
                None
            }
        })
    }

    fn map_basin_impl(&self, pt: &Point, seen: HashSet<Point>) -> HashSet<Point> {
        self.neighbors(pt).fold(seen, |mut seen, other_pt| {
            if !seen.contains(&other_pt) && other_pt.value != 9 && other_pt.value > pt.value {
                seen.insert(Point {
                    row: other_pt.row,
                    column: other_pt.column,
                    value: other_pt.value,
                });
                self.map_basin_impl(&other_pt, seen)
            } else {
                seen
            }
        })
    }

    pub(crate) fn map_basin(&self, row: usize, column: usize) -> HashSet<Point> {
        self.map_basin_impl(
            &Point {
                row,
                column,
                value: self.grid[row][column],
            },
            [Point {
                row,
                column,
                value: self.grid[row][column],
            }]
            .into_iter()
            .collect(),
        )
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
        let expected_output = vec![Point {
            row: 0,
            column: 0,
            value: 1,
        }];
        let heightmap = Heightmap::parse("1\n2")?;
        let output = heightmap.filter(|value, _| value == 1);
        assert_eq!(output.collect::<Vec<_>>(), expected_output);
        Ok(())
    }

    #[test]
    fn filter_provides_neighbors() -> Result<()> {
        let expected_output = vec![
            (Point {
                row: 1,
                column: 1,
                value: 1,
            }),
        ];
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

    #[test]
    fn map_basin_maps_out_the_basin_for_the_low_point() -> Result<()> {
        let expected_basin: HashSet<Point> = [
            Point { row: 1, column: 2, value: 8, },
            Point { row: 1, column: 3, value: 7, },
            Point { row: 1, column: 4, value: 8, },
            Point { row: 2, column: 1, value: 8, },
            Point { row: 2, column: 2, value: 5, },
            Point { row: 2, column: 3, value: 6, },
            Point { row: 2, column: 4, value: 7, },
            Point { row: 2, column: 5, value: 8, },
            Point { row: 3, column: 0, value: 8, },
            Point { row: 3, column: 1, value: 7, },
            Point { row: 3, column: 2, value: 6, },
            Point { row: 3, column: 3, value: 7, },
            Point { row: 3, column: 4, value: 8, },
            Point { row: 4, column: 1, value: 8, },
        ]
        .into_iter()
        .collect();
        let heightmap = Heightmap::parse(concat!(
            "2199943210\n",
            "3987894921\n",
            "9856789892\n",
            "8767896789\n",
            "9899965678"
        ))?;
        let basin = heightmap.map_basin(2, 2);
        assert_eq!(basin, expected_basin);
        Ok(())
    }
}
