// SPDX-License-Identifier: GPL-3.0-only

use anyhow::{anyhow, Result};
use nom::{
    character::complete::{digit1, newline, space0},
    combinator::map_res,
    multi::many_m_n,
    sequence::{preceded, terminated},
    IResult,
};

#[derive(Debug, PartialEq)]
pub(crate) struct Board {
    board: [[u8; 5]; 5],
    marked_rows: [u8; 5],
    marked_columns: [u8; 5],
    winning_mark: Option<u8>,
}

impl Board {
    pub(crate) fn parse(input: &str) -> IResult<&str, Board> {
        let square = map_res(digit1, |s| u8::from_str_radix(s, 10));
        let line = many_m_n(5, 5, preceded(space0, square));
        let mut board = map_res(many_m_n(5, 5, terminated(line, newline)), Board::try_from);
        board(input)
    }

    pub(crate) fn mark(&mut self, value: u8) {
        if self.winning_mark.is_none() {
            'outer: for (r, row) in self.board.iter().enumerate() {
                for (c, square) in row.iter().enumerate() {
                    if *square == value {
                        self.marked_rows[r] |= 1 << c;
                        self.marked_columns[c] |= 1 << r;
                        break 'outer;
                    }
                }
            }
            self.check_and_set_winner(value)
        }
    }

    fn check_and_set_winner(&mut self, value: u8) {
        let winning_column = self
            .marked_columns
            .iter()
            .find(|column| **column == 0b11111);
        let winning_row = self.marked_rows.iter().find(|row| **row == 0b11111);
        if winning_column.is_some() || winning_row.is_some() {
            self.winning_mark = Some(value)
        }
    }

    pub(crate) fn is_winner(&self) -> bool {
        self.winning_mark.is_some()
    }

    pub(crate) fn score(&self) -> Option<u16> {
        let unmarked_rows = self.marked_rows.iter().map(|x| !x);
        let winning_mark = self.winning_mark?;
        let unmarked_tally = unmarked_rows.enumerate().fold(0u16, |sum, (r, row)| {
            sum + [0b00001, 0b00010, 0b00100, 0b01000, 0b10000]
                .into_iter()
                .enumerate()
                .fold(0u16, |sum, (c, column)| {
                    if row & column != 0 {
                        sum + self.board[r as usize][c] as u16
                    } else {
                        sum
                    }
                })
        });
        Some(unmarked_tally * winning_mark as u16)
    }

    pub(super) fn new(board: [[u8; 5]; 5]) -> Board {
        Board {
            board,
            marked_columns: [0; 5],
            marked_rows: [0; 5],
            winning_mark: None,
        }
    }

    #[cfg(test)]
    pub(super) fn new_with_marks(
        board: [[u8; 5]; 5],
        marked_squares: impl IntoIterator<Item = (usize, usize)>,
        winning_mark: Option<u8>,
    ) -> Board {
        let mut board = Board::new(board);
        for (r, c) in marked_squares {
            board.marked_columns[c] |= 1 << r;
            board.marked_rows[r] |= 1 << c;
        }
        board.winning_mark = winning_mark;
        board
    }
}

impl TryFrom<Vec<Vec<u8>>> for Board {
    type Error = anyhow::Error;

    fn try_from(value: Vec<Vec<u8>>) -> Result<Self, Self::Error> {
        if value.len() != 5 {
            Err(anyhow!(
                "Invalid length: expected 5 but got {}",
                value.len()
            ))
        } else {
            let mut board = [[0; 5]; 5];
            for (r, row) in value.into_iter().enumerate() {
                if row.len() != 5 {
                    return Err(anyhow!(
                        "Invalid row length: expected 5 but got {} for row {}",
                        row.len(),
                        r
                    ));
                }
                for (c, square_value) in row.into_iter().enumerate() {
                    board[r][c] = square_value;
                }
            }
            Ok(Board::new(board))
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parser_when_the_input_is_empty_it_returns_an_error() {
        let result = Board::parse("");
        assert_eq!(result.is_err(), true);
    }

    #[test]
    fn parser_when_the_input_is_wrong_it_returns_an_error() {
        let input = "a b c d e\nf g h i j\nk l m n o\np q r s t u\nv w x y z\n";
        let result = Board::parse(input);
        assert_eq!(result.is_err(), true);
    }

    #[test]
    fn parser_when_the_input_has_not_enough_rows() {
        let result = Board::parse("1 2 3 4 5\n6 7 8 9 10");
        assert_eq!(result.is_err(), true);
    }

    #[test]
    fn parser_when_the_input_has_not_squares() {
        let result = Board::parse("1 2 3 4");
        assert_eq!(result.is_err(), true);
    }

    #[test]
    fn parser_when_the_input_has_a_five_rows_with_five_numbers_it_returns_a_board() {
        let expected_result = Ok((
            "",
            Board::new([
                [22, 13, 17, 11, 0],
                [8, 2, 23, 4, 24],
                [21, 9, 14, 16, 7],
                [6, 10, 3, 18, 5],
                [1, 12, 20, 15, 19],
            ]),
        ));
        let input =
            "22 13 17 11  0\n 8  2 23  4 24\n21  9 14 16  7\n 6 10  3 18  5\n 1 12 20 15 19\n";
        let result = Board::parse(input);
        assert_eq!(result, expected_result);
    }

    #[test]
    fn parser_when_the_input_has_more_it_returns_the_rest() {
        let expected_result = Ok((
            "\n14 21 17 24  4\n10 16 15  9 19\n18  8 23 26 20\n22 11 13  6  5\n 2  0 12  3  7\n",
            Board::new([
                [3, 15, 0, 2, 22],
                [9, 18, 13, 17, 5],
                [19, 8, 7, 25, 23],
                [20, 11, 10, 24, 4],
                [14, 21, 16, 12, 6],
            ]),
        ));
        let input = concat!(
            " 3 15  0  2 22\n 9 18 13 17  5\n19  8  7 25 23\n20 11 10 24  4\n14 21 16 12  6\n",
            "\n",
            "14 21 17 24  4\n10 16 15  9 19\n18  8 23 26 20\n22 11 13  6  5\n 2  0 12  3  7\n"
        );
        let result = Board::parse(input);
        assert_eq!(result, expected_result);
    }

    #[test]
    fn board_marking_a_square_sets_the_state_to_marked() {
        let expected_board = Board::new_with_marks(
            [
                [3, 15, 0, 2, 22],
                [9, 18, 13, 17, 5],
                [19, 8, 7, 25, 23],
                [20, 11, 10, 24, 4],
                [14, 21, 16, 12, 6],
            ],
            [(1, 1)],
            None,
        );
        let mut board = Board::new([
            [3, 15, 0, 2, 22],
            [9, 18, 13, 17, 5],
            [19, 8, 7, 25, 23],
            [20, 11, 10, 24, 4],
            [14, 21, 16, 12, 6],
        ]);
        board.mark(18);
        assert_eq!(board, expected_board);
    }

    #[test]
    fn board_with_a_marked_row_is_a_winner() {
        let mut board = Board::new([
            [3, 15, 0, 2, 22],
            [9, 18, 13, 17, 5],
            [19, 8, 7, 25, 23],
            [20, 11, 10, 24, 4],
            [14, 21, 16, 12, 6],
        ]);
        board.mark(0);
        board.mark(2);
        board.mark(3);
        board.mark(15);
        board.mark(22);
        assert_eq!(board.is_winner(), true);
    }

    #[test]
    fn board_with_a_marked_column_is_a_winner() {
        let mut board = Board::new([
            [3, 15, 0, 2, 22],
            [9, 18, 13, 17, 5],
            [19, 8, 7, 25, 23],
            [20, 11, 10, 24, 4],
            [14, 21, 16, 12, 6],
        ]);
        board.mark(25);
        board.mark(24);
        board.mark(17);
        board.mark(12);
        board.mark(2);
        assert_eq!(board.is_winner(), true);
    }

    #[test]
    fn board_without_a_winner_reports_no_score() {
        let expected_score = None;
        let board = Board::new([
            [3, 15, 0, 2, 22],
            [9, 18, 13, 17, 5],
            [19, 8, 7, 25, 23],
            [20, 11, 10, 24, 4],
            [14, 21, 16, 12, 6],
        ]);
        let score = board.score();
        assert_eq!(score, expected_score);
    }

    #[test]
    fn board_with_a_winner_reports_the_score() {
        let expected_score = Some(4512);
        let board = Board::new_with_marks(
            [
                [14, 21, 17, 24, 4],
                [10, 16, 15, 9, 19],
                [18, 8, 23, 26, 20],
                [22, 11, 13, 6, 5],
                [2, 0, 12, 3, 7],
            ],
            [
                (0, 0),
                (0, 1),
                (0, 2),
                (0, 3),
                (0, 4),
                (1, 3),
                (2, 2),
                (3, 1),
                (3, 4),
                (4, 0),
                (4, 1),
                (4, 4),
            ],
            Some(24),
        );
        let score = board.score();
        assert_eq!(score, expected_score);
    }
}
