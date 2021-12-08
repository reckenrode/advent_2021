// SPDX-License-Identifier: GPL-3.0-only

use std::{
    collections::VecDeque,
    fs::File,
    io::{BufRead, BufReader},
    num::NonZeroUsize,
    path::PathBuf,
};

use anyhow::{Error, Result};
use clap::Parser;

#[derive(Parser)]
#[clap(about = "Sonar sweep")]
pub(crate) struct Day1 {
    input: PathBuf,
}

impl Day1 {
    pub(crate) fn run(self) -> Result<()> {
        let file = File::open(self.input)?;
        let reader = BufReader::new(file);
        let lines = parse_lines(reader)?;

        let increases = count_increases(&lines, NonZeroUsize::new(1).unwrap());
        println!("Times the depth measurement increased: {}", increases);

        let increases = count_increases(&lines, NonZeroUsize::new(3).unwrap());
        println!(
            "Times the sliding depth measurement increased: {}",
            increases
        );
        Ok(())
    }
}

pub(crate) fn count_increases<'a>(
    xs: impl IntoIterator<Item = &'a i32> + 'a,
    window_size: NonZeroUsize,
) -> usize {
    fn update_window(window: &mut VecDeque<i32>, value: i32) {
        for x in window.into_iter() {
            *x = *x + value;
        }
    }
    let window_size = window_size.get();
    let mut it = xs.into_iter();
    let mut window = VecDeque::with_capacity(window_size);
    for _ in 0..window_size {
        if let Some(x) = it.next() {
            update_window(&mut window, *x);
            window.push_back(*x);
        }
    }
    let mut count = 0;
    for value in it {
        if let Some(previous) = window.pop_front() {
            update_window(&mut window, *value);
            let current = *window.front().unwrap_or(value);
            count += (current > previous) as usize;
        }
        window.push_back(*value);
    }
    count
}

fn parse_lines(reader: impl BufRead) -> Result<Vec<i32>> {
    reader
        .lines()
        .map(|line| line?.parse().map_err(Error::from))
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    const EXAMPLE_INPUT: [i32; 10] = [199, 200, 208, 210, 200, 207, 240, 269, 260, 263];

    #[test]
    fn parser_parses_the_file() -> Result<()> {
        let expected_output = EXAMPLE_INPUT;
        let input = "199\n200\n208\n210\n200\n207\n240\n269\n260\n263\n";
        let result = parse_lines(input.as_bytes())?;
        assert_eq!(result, expected_output);
        Ok(())
    }

    #[test]
    fn example_1_has_seven_increases() -> Result<()> {
        let expected_increases = 7;
        assert_eq!(
            count_increases(&EXAMPLE_INPUT, NonZeroUsize::new(1).unwrap()),
            expected_increases
        );
        Ok(())
    }

    #[test]
    fn example_2_has_5_increases() -> Result<()> {
        let expected_increases = 5;
        assert_eq!(
            count_increases(&EXAMPLE_INPUT, NonZeroUsize::new(3).unwrap()),
            expected_increases
        );
        Ok(())
    }
}
