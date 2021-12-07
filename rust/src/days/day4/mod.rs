// SPDX-License-Identifier: GPL-3.0-only

use std::{
    fs::File,
    io::{BufReader, Read},
    path::PathBuf,
};

use anyhow::{anyhow, Result};
use clap::Parser;

use crate::util::read_input;

use self::bingo::{Board, Game};

mod bingo;

#[derive(Parser)]
#[clap(about = "Giant squid")]
pub(crate) struct Day4 {
    input: PathBuf,
}

impl Day4 {
    pub(crate) fn run(self) -> Result<()> {
        let input = read_input(self.input)?;
        let mut game = Game::parse(input.as_str())?;
        game.mark_draws();

        let no_winner_error = || anyhow!("Expected a winner but none was found.");
        let winners: Vec<&Board> = game.winners().collect();

        let my_score = winners
            .first()
            .ok_or_else(&no_winner_error)?
            .score()
            .ok_or_else(&no_winner_error)?;

        let squid_score = winners
            .last()
            .ok_or_else(&no_winner_error)?
            .score()
            .ok_or_else(&no_winner_error)?;

        println!("The score if Santa wins is: {}", my_score);
        println!("The score if Santa lets the squid wins is: {}", squid_score);

        Ok(())
    }
}
