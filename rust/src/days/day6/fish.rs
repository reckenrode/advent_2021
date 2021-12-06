// SPDX-License-Identifier: GPL-3.0-only

use std::{io::BufRead, mem::swap};

use anyhow::Result;
use itertools::Itertools;

const DEFAULT_TIMER: usize = 6;
const NEW_FISH_DELAY: usize = 2;
const ARRAY_SIZE: usize = DEFAULT_TIMER + NEW_FISH_DELAY + 2;

#[derive(Debug, PartialEq)]
pub(crate) struct Fish {
    buf: [u128; ARRAY_SIZE],
}

impl Fish {
    fn new(buf: [u128; ARRAY_SIZE]) -> Self {
        Fish { buf }
    }

    pub(crate) fn parse(mut reader: impl BufRead) -> Result<Self> {
        let mut buf = String::new();
        reader.read_line(&mut buf)?;

        let mut result = [0; ARRAY_SIZE];
        buf.trim_end()
            .split(',')
            .map(|src| Ok(usize::from_str_radix(src, 10)?))
            .collect::<Result<Vec<_>>>()?
            .into_iter()
            .counts()
            .into_iter()
            .for_each(|(key, value)| result[key] = value as u128);
        Ok(Self::new(result))
    }

    pub(crate) fn count(&self) -> u128 {
        self.buf.iter().sum()
    }

    pub(crate) fn tick(&mut self) {
        let mut buf = [0; ARRAY_SIZE];
        for (idx, count) in self.buf.iter().enumerate() {
            if idx == 0 {
                buf[DEFAULT_TIMER] = *count;
                buf[DEFAULT_TIMER + NEW_FISH_DELAY] = *count;
            } else {
                buf[idx - 1] += count;
            }
        }
        swap(&mut buf, &mut self.buf);
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn read_input_parses_the_fish() -> Result<()> {
        let expected_result = Fish::new([0, 1, 1, 2, 1, 0, 0, 0, 0, 0]);
        let result = Fish::parse("3,4,3,1,2".as_bytes())?;
        assert_eq!(result, expected_result);
        Ok(())
    }

    #[test]
    fn tick_decreases_the_timer_of_the_fish_by_one() {
        let expected_fish = Fish::new([0, 0, 0, 0, 1, 0, 0, 0, 0, 0]);
        let mut fish = Fish::new([0, 0, 0, 0, 0, 1, 0, 0, 0, 0]);
        fish.tick();
        assert_eq!(fish, expected_fish);
    }

    #[test]
    fn tick_at_zero_resets_the_timer_of_the_fish_to_the_limit() {
        let expected_fish = 1;
        let mut fish = Fish::new([1, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        fish.tick();
        assert_eq!(fish.buf[DEFAULT_TIMER], expected_fish);
    }

    #[test]
    fn tick_at_zero_produces_an_additional_fish() {
        let expected_fish = Fish::new([0, 0, 0, 0, 0, 0, 1, 0, 1, 0]);
        let mut fish = Fish::new([1, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        fish.tick();
        assert_eq!(fish, expected_fish);
    }

    #[test]
    fn tick_new_fish_does_not_produce_another_fish() {
        let expected_fish = Fish::new([0, 0, 0, 0, 0, 1, 0, 1, 0, 0]);
        let mut fish = Fish::new([0, 0, 0, 0, 0, 0, 1, 0, 1, 0]);
        fish.tick();
        assert_eq!(fish, expected_fish);
    }

    #[test]
    fn count_returns_total_number_of_fish() {
        let expected_count = 88;
        let fish = Fish::new([1, 1, 2, 3, 5, 8, 13, 21, 34, 0]);
        let count = fish.count();
        assert_eq!(count, expected_count);
    }
}
