// SPDX-License-Identifier: GPL-3.0-only

use std::{cmp::Reverse, ops::Index};

use priority_queue::PriorityQueue;

#[derive(Debug, PartialEq)]
pub struct Point {
    row: usize,
    column: usize,
    value: u8,
}

#[derive(Debug, PartialEq)]
pub struct Graph {
    nodes: Vec<u8>,
    pub(crate) rows: usize,
    pub(crate) columns: usize,
}

impl Graph {
    pub(crate) fn shortest_path_cost(
        &self,
        (start_row, start_column): (usize, usize),
        (goal_row, goal_column): (usize, usize),
    ) -> u64 {
        let idx = |row: usize, column: usize| row * self.columns + column;
        let try_get_node = |row: usize, row_offset: i32, column: usize, column_offset: i32| {
            if row == 0 && row_offset < 0 || column == 0 && column_offset < 0 {
                return None;
            }
            let row = if row_offset < 0 {
                row - row_offset.abs() as usize
            } else {
                row + row_offset as usize
            };
            let column = if column_offset < 0 {
                column - column_offset.abs() as usize
            } else {
                column + column_offset as usize
            };
            if row < self.rows && column < self.columns {
                let value = self.nodes.get(idx(row, column));
                value.map(|v| ((row, column), v))
            } else {
                None
            }
        };

        let mut distances = vec![u64::MAX; self.rows * self.columns];
        distances[idx(start_row, start_column)] = 0;

        let mut visited = vec![false; self.rows * self.columns];

        let mut visiting = PriorityQueue::new();
        for row in 0..self.rows {
            for column in 0..self.columns {
                visiting.push((row, column), Reverse(distances[idx(row, column)]));
            }
        }

        while let Some(((current_row, current_column), cost)) = visiting.pop() {
            if cost == Reverse(u64::MAX) || visited[idx(goal_row, goal_column)] {
                break;
            }
            [
                try_get_node(current_row, 1, current_column, 0),
                try_get_node(current_row, -1, current_column, 0),
                try_get_node(current_row, 0, current_column, 1),
                try_get_node(current_row, 0, current_column, -1),
            ]
            .into_iter()
            .flatten()
            .filter(|((row, column), _)| !visited[idx(*row, *column)])
            .for_each(|((row, column), value)| {
                let current_distance = distances[idx(row, column)];
                let new_distance = distances[idx(current_row, current_column)] + *value as u64;
                if new_distance < current_distance {
                    distances[idx(row, column)] = new_distance;
                    visiting.push((row, column), Reverse(new_distance));
                }
            });
            visited[idx(current_row, current_column)] = true;
        }

        distances[idx(goal_row, goal_column)]
    }

    pub(crate) fn grow(&mut self, row_multiplier: usize, col_multiplier: usize) {
        fn wrap(value: u8) -> u8 {
            1 + (value - 1) % 9
        }

        let (new_rows, new_columns) = (
            self.rows * (1 + row_multiplier),
            self.columns * (1 + col_multiplier),
        );

        let mut nodes: Vec<Vec<u8>> = Vec::with_capacity(new_rows);
        nodes.resize(new_rows, vec![0; new_columns]);

        for row_index in 0..=row_multiplier {
            let row_offset = row_index * self.rows;
            for row in 0..self.rows {
                let offset = row * self.rows;
                for col_index in 0..=col_multiplier {
                    let col_offset = col_index * self.columns;
                    let target_slice: Vec<_> = self.nodes[offset..(offset + self.columns)]
                        .into_iter()
                        .map(|x| wrap(*x + row_index as u8 + col_index as u8))
                        .collect();
                    nodes[row + row_offset][col_offset..(col_offset + self.columns)]
                        .copy_from_slice(&target_slice);
                }
            }
        }

        self.nodes = nodes.into_iter().flatten().collect();
        self.rows = new_rows;
        self.columns = new_columns;
    }
}

impl<const N: usize, const M: usize> From<[[u8; M]; N]> for Graph {
    fn from(value: [[u8; M]; N]) -> Self {
        Graph {
            nodes: value.into_iter().flat_map(|row| row).collect(),
            rows: N,
            columns: M,
        }
    }
}

impl From<Vec<Vec<u8>>> for Graph {
    fn from(value: Vec<Vec<u8>>) -> Self {
        Graph {
            rows: value.len(),
            columns: value[0].len(),
            nodes: value.into_iter().flatten().collect(),
        }
    }
}

impl Index<(usize, usize)> for Graph {
    type Output = u8;

    fn index(&self, (row, column): (usize, usize)) -> &Self::Output {
        &self.nodes[row * self.columns + column]
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn starting_position_has_zero_cost() {
        let expected_cost = 0;
        let input = Graph::from([[42]]);
        let cost = input.shortest_path_cost((0, 0), (0, 0));
        assert_eq!(cost, expected_cost);
    }

    #[test]
    fn finds_the_shortest_path_in_a_simple_graph() {
        let expected_cost = 7;
        let input = Graph::from([[1, 2], [3, 5]]);
        let cost = input.shortest_path_cost((0, 0), (1, 1));
        assert_eq!(cost, expected_cost);
    }

    #[test]
    fn finds_the_path_through_a_twisty_graph() {
        let expected_cost = 22;
        let input = Graph::from([
            [9, 1, 1, 1],
            [9, 9, 2, 1],
            [1, 1, 3, 9],
            [1, 9, 9, 9],
            [1, 1, 1, 9],
        ]);
        let cost = input.shortest_path_cost((0, 0), (4, 3));
        assert_eq!(cost, expected_cost);
    }

    #[test]
    fn finds_the_path_through_the_example_graph() {
        let expected_cost = 40;
        let input = Graph::from([
            [1, 1, 6, 3, 7, 5, 1, 7, 4, 2],
            [1, 3, 8, 1, 3, 7, 3, 6, 7, 2],
            [2, 1, 3, 6, 5, 1, 1, 3, 2, 8],
            [3, 6, 9, 4, 9, 3, 1, 5, 6, 9],
            [7, 4, 6, 3, 4, 1, 7, 1, 1, 1],
            [1, 3, 1, 9, 1, 2, 8, 1, 3, 7],
            [1, 3, 5, 9, 9, 1, 2, 4, 2, 1],
            [3, 1, 2, 5, 4, 2, 1, 6, 3, 9],
            [1, 2, 9, 3, 1, 3, 8, 5, 2, 1],
            [2, 3, 1, 1, 9, 4, 4, 5, 8, 1],
        ]);
        let cost = input.shortest_path_cost((0, 0), (9, 9));
        assert_eq!(cost, expected_cost);
    }

    #[test]
    fn extending_the_graph_increases_costs_by_1() {
        let expected_graph = Graph::from([[1, 2, 3], [2, 3, 4], [3, 4, 5]]);
        let mut graph = Graph::from([[1]]);
        graph.grow(2, 2);
        assert_eq!(graph, expected_graph);
    }

    #[test]
    fn numbers_wrap_around_at_9_when_the_graph_is_extended() {
        let expected_graph = Graph::from([
            [1, 9, 2, 1, 3, 2],
            [8, 1, 9, 2, 1, 3],
            [2, 1, 3, 2, 4, 3],
            [9, 2, 1, 3, 2, 4],
        ]);
        let mut graph = Graph::from([[1, 9], [8, 1]]);
        graph.grow(1, 2);
        assert_eq!(graph, expected_graph);
    }
}
