// SPDX-License-Identifier: GPL-3.0-only

use std::{fs::File, io::BufReader, path::PathBuf};

use anyhow::Result;
use clap::Parser;

use crate::days::day3::report::{Report, ReportExt};

mod report;

#[derive(Parser)]
#[clap(about = "Submarine diagnostics")]
pub(crate) struct Day3 {
    input: PathBuf,
}

impl Day3 {
    pub(crate) fn run(self) -> Result<()> {
        let stream = File::open(self.input)?;
        let reader = BufReader::new(stream);
        let report = Report::parse(reader)?;

        let (gamma, epsilon) = report.extrema();
        let power_consumption = report.power_consumption();
        println!("Gamma: {}, Epsilon: {}", gamma, epsilon);
        println!("Power Consumption: {}", power_consumption);

        let life_support_rating = report.life_support_rating();
        println!("Life Support Rating: {}", life_support_rating);

        Ok(())
    }
}
