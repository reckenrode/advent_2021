// SPDX-License-Identifier: GPL-3.0-only

use std::{
    collections::HashSet,
    mem::{swap, take},
};

use anyhow::{anyhow, Result};
use nom::{
    character::complete::newline,
    combinator::map,
    multi::{many1, separated_list1},
    sequence::separated_pair,
};

use super::{board::Board, draws::Draws};

#[derive(Debug, PartialEq)]
pub(crate) struct Game {
    draws: Draws,
    boards: Vec<Board>,
    winners: Vec<usize>,
    winner_bitmap: HashSet<usize>,
}

impl Game {
    pub(crate) fn parse(input: &str) -> Result<Game> {
        let draws = Draws::parse;
        let boards = separated_list1(newline, Board::parse);
        let mut game = map(
            separated_pair(draws, many1(newline), boards),
            |(draws, boards)| Game::new(draws, boards),
        );
        let (_, result) = game(input).map_err(|e| anyhow!("{}", e))?;
        Ok(result)
    }

    fn new(draws: Draws, boards: Vec<Board>) -> Game {
        Game {
            draws,
            boards,
            winners: Vec::new(),
            winner_bitmap: HashSet::new(),
        }
    }

    pub(crate) fn mark(&mut self, value: u8) {
        for (idx, board) in self.boards.iter_mut().enumerate() {
            board.mark(value);
            if board.is_winner() && !self.winner_bitmap.contains(&idx) {
                self.winners.push(idx);
                self.winner_bitmap.insert(idx);
            }
        }
    }

    pub(crate) fn mark_draws(&mut self) {
        let mut draws = take(&mut self.draws);
        draws.iter().for_each(|draw| self.mark(*draw));
        swap(&mut draws, &mut self.draws);
    }

    pub(crate) fn winners(&self) -> impl Iterator<Item = &Board> {
        self.winners.iter().map(|x| &self.boards[*x])
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parser_when_the_input_is_empty_it_returns_an_error() {
        let result = Game::parse("");
        assert_eq!(result.is_err(), true)
    }

    #[test]
    fn parser_when_the_input_is_wrong_it_returns_an_error() {
        let result = Game::parse("a,b,c");
        assert_eq!(result.is_err(), true)
    }

    #[test]
    fn parser_when_the_input_is_complete_it_returns_a_game() -> Result<()> {
        let expected_result = Game::new(
            Draws(vec![
                7, 4, 9, 5, 11, 17, 23, 2, 0, 14, 21, 24, 10, 16, 13, 6, 15, 25, 12, 22, 18, 20, 8,
                19, 3, 26, 1,
            ]),
            vec![
                Board::new([
                    [22, 13, 17, 11, 0],
                    [8, 2, 23, 4, 24],
                    [21, 9, 14, 16, 7],
                    [6, 10, 3, 18, 5],
                    [1, 12, 20, 15, 19],
                ]),
                Board::new([
                    [3, 15, 0, 2, 22],
                    [9, 18, 13, 17, 5],
                    [19, 8, 7, 25, 23],
                    [20, 11, 10, 24, 4],
                    [14, 21, 16, 12, 6],
                ]),
                Board::new([
                    [14, 21, 17, 24, 4],
                    [10, 16, 15, 9, 19],
                    [18, 8, 23, 26, 20],
                    [22, 11, 13, 6, 5],
                    [2, 0, 12, 3, 7],
                ]),
            ],
        );
        let input = concat!(
            "7,4,9,5,11,17,23,2,0,14,21,24,10,16,13,6,15,25,12,22,18,20,8,19,3,26,1\n",
            "\n",
            "22 13 17 11  0\n",
            " 8  2 23  4 24\n",
            "21  9 14 16  7\n",
            " 6 10  3 18  5\n",
            " 1 12 20 15 19\n",
            "\n",
            " 3 15  0  2 22\n",
            " 9 18 13 17  5\n",
            "19  8  7 25 23\n",
            "20 11 10 24  4\n",
            "14 21 16 12  6\n",
            "\n",
            "14 21 17 24  4\n",
            "10 16 15  9 19\n",
            "18  8 23 26 20\n",
            "22 11 13  6  5\n",
            " 2  0 12  3  7\n",
        );
        let result = Game::parse(input)?;
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn game_marking_a_square_marks_all_boards_with_that_square() {
        let expected_game = Game::new(
            Draws(vec![
                7, 4, 9, 5, 11, 17, 23, 2, 0, 14, 21, 24, 10, 16, 13, 6, 15, 25, 12, 22, 18, 20, 8,
                19, 3, 26, 1,
            ]),
            vec![
                Board::new_with_marks(
                    [
                        [22, 13, 17, 11, 0],
                        [8, 2, 23, 4, 24],
                        [21, 9, 14, 16, 7],
                        [6, 10, 3, 18, 5],
                        [1, 12, 20, 15, 19],
                    ],
                    [(0, 0)],
                    None,
                ),
                Board::new_with_marks(
                    [
                        [3, 15, 0, 2, 22],
                        [9, 18, 13, 17, 5],
                        [19, 8, 7, 25, 23],
                        [20, 11, 10, 24, 4],
                        [14, 21, 16, 12, 6],
                    ],
                    [(0, 4)],
                    None,
                ),
                Board::new_with_marks(
                    [
                        [14, 21, 17, 24, 4],
                        [10, 16, 15, 9, 19],
                        [18, 8, 23, 26, 20],
                        [22, 11, 13, 6, 5],
                        [2, 0, 12, 3, 7],
                    ],
                    [(3, 0)],
                    None,
                ),
            ],
        );
        let mut game = Game::new(
            Draws(vec![
                7, 4, 9, 5, 11, 17, 23, 2, 0, 14, 21, 24, 10, 16, 13, 6, 15, 25, 12, 22, 18, 20, 8,
                19, 3, 26, 1,
            ]),
            vec![
                Board::new([
                    [22, 13, 17, 11, 0],
                    [8, 2, 23, 4, 24],
                    [21, 9, 14, 16, 7],
                    [6, 10, 3, 18, 5],
                    [1, 12, 20, 15, 19],
                ]),
                Board::new([
                    [3, 15, 0, 2, 22],
                    [9, 18, 13, 17, 5],
                    [19, 8, 7, 25, 23],
                    [20, 11, 10, 24, 4],
                    [14, 21, 16, 12, 6],
                ]),
                Board::new([
                    [14, 21, 17, 24, 4],
                    [10, 16, 15, 9, 19],
                    [18, 8, 23, 26, 20],
                    [22, 11, 13, 6, 5],
                    [2, 0, 12, 3, 7],
                ]),
            ],
        );
        game.mark(22);
        assert_eq!(game, expected_game);
    }

    #[test]
    fn game_when_there_are_no_winning_boards_returns_empty() {
        let game = Game::new(
            Draws(vec![
                7, 4, 9, 5, 11, 17, 23, 2, 0, 14, 21, 24, 10, 16, 13, 6, 15, 25, 12, 22, 18, 20, 8,
                19, 3, 26, 1,
            ]),
            vec![
                Board::new([
                    [22, 13, 17, 11, 0],
                    [8, 2, 23, 4, 24],
                    [21, 9, 14, 16, 7],
                    [6, 10, 3, 18, 5],
                    [1, 12, 20, 15, 19],
                ]),
                Board::new([
                    [3, 15, 0, 2, 22],
                    [9, 18, 13, 17, 5],
                    [19, 8, 7, 25, 23],
                    [20, 11, 10, 24, 4],
                    [14, 21, 16, 12, 6],
                ]),
                Board::new([
                    [14, 21, 17, 24, 4],
                    [10, 16, 15, 9, 19],
                    [18, 8, 23, 26, 20],
                    [22, 11, 13, 6, 5],
                    [2, 0, 12, 3, 7],
                ]),
            ],
        );
        let mut result = game.winners();
        assert_eq!(result.next(), None);
    }

    #[test]
    fn game_when_there_are_winning_boards_returns_the_winners() {
        let expected_winner = Board::new_with_marks(
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
        let mut game = Game::new(
            Draws(vec![7, 4, 9, 5, 11, 17, 23, 2, 0, 14, 21, 24]),
            vec![
                Board::new([
                    [22, 13, 17, 11, 0],
                    [8, 2, 23, 4, 24],
                    [21, 9, 14, 16, 7],
                    [6, 10, 3, 18, 5],
                    [1, 12, 20, 15, 19],
                ]),
                Board::new([
                    [3, 15, 0, 2, 22],
                    [9, 18, 13, 17, 5],
                    [19, 8, 7, 25, 23],
                    [20, 11, 10, 24, 4],
                    [14, 21, 16, 12, 6],
                ]),
                Board::new([
                    [14, 21, 17, 24, 4],
                    [10, 16, 15, 9, 19],
                    [18, 8, 23, 26, 20],
                    [22, 11, 13, 6, 5],
                    [2, 0, 12, 3, 7],
                ]),
            ],
        );
        game.mark_draws();
        let result: Vec<&Board> = game.winners().collect();
        assert_eq!(result, vec![&expected_winner]);
    }

    #[test]
    fn game_when_there_are_more_winning_boards_keeps_going_then_returns_that_winner() {
        let expected_wining_scores = vec![4512, 2192, 1924];
        let mut game = Game::new(
            Draws(vec![
                7, 4, 9, 5, 11, 17, 23, 2, 0, 14, 21, 24, 10, 16, 13, 6, 15, 25, 12, 22, 18, 20, 8,
                19, 3, 26, 1,
            ]),
            vec![
                Board::new([
                    [22, 13, 17, 11, 0],
                    [8, 2, 23, 4, 24],
                    [21, 9, 14, 16, 7],
                    [6, 10, 3, 18, 5],
                    [1, 12, 20, 15, 19],
                ]),
                Board::new([
                    [3, 15, 0, 2, 22],
                    [9, 18, 13, 17, 5],
                    [19, 8, 7, 25, 23],
                    [20, 11, 10, 24, 4],
                    [14, 21, 16, 12, 6],
                ]),
                Board::new([
                    [14, 21, 17, 24, 4],
                    [10, 16, 15, 9, 19],
                    [18, 8, 23, 26, 20],
                    [22, 11, 13, 6, 5],
                    [2, 0, 12, 3, 7],
                ]),
            ],
        );
        game.mark_draws();
        let winning_scores: Option<Vec<u16>> = game.winners().map(Board::score).collect();
        assert_eq!(winning_scores.unwrap(), expected_wining_scores);
    }
}
