// SPDX-License-Identifier: GPL-3.0-only

use std::{
    fs::File,
    io::{BufReader, Read},
    path::PathBuf,
};

use anyhow::{anyhow, Result};
use clap::Parser;

use self::{bitmap::Bitmap, command_list::CommandList};

mod bitmap;
mod command_list;

#[derive(Parser)]
#[clap(about = "Hydrothermal venture")]
pub(crate) struct Day5 {
    input: PathBuf,
    #[clap(short, long)]
    ignore_diagonals: bool,
    #[clap(short, long)]
    print_diagram: bool,
}

impl Day5 {
    pub(crate) fn run(self) -> Result<()> {
        let file = File::open(self.input)?;
        let mut reader = BufReader::new(file);

        let mut input: String = String::new();
        reader.read_to_string(&mut input)?;

        let commands = {
            let commands = CommandList::parse(input.as_str()).map_err(|err| anyhow!("{}", err))?;
            if self.ignore_diagonals {
                commands
                    .into_iter()
                    .filter(|((x1, y1), (x2, y2))| x1 != x2 && y1 == y2 || x1 == x2 && y1 != y2)
                    .collect()
            } else {
                commands
            }
        };
        let (max_x, max_y) = commands.required_bounds();

        let mut bitmap = Bitmap::new(max_x, max_y);
        commands.apply_commands(&mut bitmap)?;

        let overlap_count = bitmap.iter().filter(|x| *x > 1).count();

        println!("# points where lines overlap: {}", overlap_count);

        if self.print_diagram {
            println!("{}", bitmap);
        }

        Ok(())
    }
}
