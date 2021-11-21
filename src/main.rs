// SPDX-License-Identifier: GPL-3.0-only

use std::error::Error;

mod app;
mod days;
mod macros;

fn main() -> Result<(), Box<dyn Error>> {
    let app = app::parse_options();
    app.run()
}
