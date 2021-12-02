// SPDX-License-Identifier: GPL-3.0-only

use anyhow::{anyhow, Result};
use nom::{
    character::complete::{alpha1, digit1, space1},
    combinator::map_res,
    sequence::separated_pair,
};

#[derive(Default, Debug, PartialEq)]
pub(crate) struct State {
    pub(crate) position: i32,
    pub(crate) depth: i32,
    pub(crate) aim: i32,
}

pub(crate) struct Command(&'static str, Box<dyn Fn(&mut State)>);

enum CommandToken {
    Forward,
    Down,
    Up,
}

impl Command {
    pub(crate) fn parse(str: &str, use_aim: bool) -> Result<Command> {
        fn distance(input: &str) -> nom::IResult<&str, u16> {
            map_res(digit1, |s: &str| s.parse())(input)
        }
        fn command(input: &str) -> nom::IResult<&str, CommandToken> {
            map_res(alpha1, CommandToken::try_from)(input)
        }
        let command = {
            let mut command_parser = separated_pair(command, space1, distance);
            let (_, command) = command_parser(str.as_ref()).map_err(|e| anyhow!("{}", e))?;
            command
        };
        let command = if use_aim {
            match command {
                (CommandToken::Forward, number) => Command(
                    "forward",
                    Box::new(move |s| {
                        s.position += number as i32;
                        s.depth += number as i32 * s.aim;
                    }),
                ),
                (CommandToken::Down, number) => {
                    Command("down", Box::new(move |s| s.aim += number as i32))
                }
                (CommandToken::Up, number) => {
                    Command("up", Box::new(move |s| s.aim -= number as i32))
                }
            }
        } else {
            match command {
                (CommandToken::Forward, number) => {
                    Command("forward", Box::new(move |s| s.position += number as i32))
                }
                (CommandToken::Down, number) => {
                    Command("down", Box::new(move |s| s.depth += number as i32))
                }
                (CommandToken::Up, number) => {
                    Command("up", Box::new(move |s| s.depth -= number as i32))
                }
            }
        };
        Ok(command)
    }

    pub(crate) fn apply(&self, state: &mut State) {
        self.1(state)
    }
}

impl TryFrom<&str> for CommandToken {
    type Error = anyhow::Error;

    fn try_from(value: &str) -> Result<Self, Self::Error> {
        match value {
            "forward" => Ok(CommandToken::Forward),
            "down" => Ok(CommandToken::Down),
            "up" => Ok(CommandToken::Up),
            _ => Err(anyhow!("Invalid Token: {}", value)),
        }
    }
}
