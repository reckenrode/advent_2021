// SPDX-License-Identifier: GPL-3.0-only

use std::{collections::HashMap, io::BufRead};

use anyhow::{anyhow, Result};
use nom::{
    character::complete::{alpha1, digit1, space1},
    combinator::{map_opt, map_res},
    sequence::separated_pair,
};

#[derive(Default, Debug, PartialEq)]
pub(crate) struct State {
    pub(crate) position: i32,
    pub(crate) depth: i32,
    pub(crate) aim: i32,
}

pub(crate) struct Program(Vec<Box<dyn Fn(&mut State)>>);

impl Program {
    fn command_mapping(
        use_aim: bool,
    ) -> HashMap<&'static str, Box<dyn Fn(u16) -> Box<dyn Fn(&mut State)>>> {
        if use_aim {
            HashMap::from([
                (
                    "forward",
                    Box::new(|number: u16| {
                        Box::new(move |state: &mut State| {
                            state.position += number as i32;
                            state.depth += number as i32 * state.aim;
                        }) as Box<dyn Fn(&mut State)>
                    }) as Box<dyn Fn(u16) -> Box<dyn Fn(&mut State)>>,
                ),
                (
                    "down",
                    Box::new(|number: u16| {
                        Box::new(move |state: &mut State| state.aim += number as i32)
                    }),
                ),
                (
                    "up",
                    Box::new(|number: u16| {
                        Box::new(move |state: &mut State| state.aim -= number as i32)
                    }),
                ),
            ])
        } else {
            HashMap::from([
                (
                    "forward",
                    Box::new(|number: u16| {
                        Box::new(move |state: &mut State| state.position += number as i32)
                            as Box<dyn Fn(&mut State)>
                    }) as Box<dyn Fn(u16) -> Box<dyn Fn(&mut State)>>,
                ),
                (
                    "down",
                    Box::new(|number: u16| {
                        Box::new(move |state: &mut State| state.depth += number as i32)
                    }),
                ),
                (
                    "up",
                    Box::new(|number: u16| {
                        Box::new(move |state: &mut State| state.depth -= number as i32)
                    }),
                ),
            ])
        }
    }

    pub(crate) fn parse(reader: impl BufRead, use_aim: bool) -> Result<Program> {
        let cmds = Program::command_mapping(use_aim);

        let raw_program: Result<Vec<_>> = reader
            .lines()
            .map(|line| {
                let line = line?;

                let mut parser = {
                    fn distance(input: &str) -> nom::IResult<&str, u16> {
                        map_res(digit1, |s: &str| s.parse())(input)
                    }
                    let command = map_opt(alpha1, |str| cmds.contains_key(str).then(|| str));
                    separated_pair(command, space1, distance)
                };

                let command = {
                    let (_, (command, value)) =
                        parser(line.as_ref()).map_err(|e| anyhow!("{}", e))?;
                    cmds[command](value)
                };

                Ok(command)
            })
            .collect();

        raw_program.map(Program)
    }

    pub(crate) fn run(&self) -> State {
        self.0.iter().fold(State::default(), |mut state, command| {
            command(&mut state);
            state
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    const EXAMPLE_PROGRAM: &str = "forward 5\ndown 5\nforward 8\nup 3\ndown 8\nforward 2\n";

    #[test]
    fn example_1_has_the_expected_result() -> Result<()> {
        let expected_state = {
            let mut state = State::default();
            state.position = 15;
            state.depth = 10;
            state
        };
        let input = EXAMPLE_PROGRAM;
        let program = Program::parse(input.as_bytes(), false)?;
        let result = program.run();
        assert_eq!(result, expected_state);
        Ok(())
    }

    #[test]
    fn example_2_has_the_expected_result() -> Result<()> {
        let expected_state = State {
            position: 15,
            depth: 60,
            aim: 10,
        };
        let input = EXAMPLE_PROGRAM;
        let program = Program::parse(input.as_bytes(), true)?;
        let result = program.run();
        assert_eq!(result, expected_state);
        Ok(())
    }
}
