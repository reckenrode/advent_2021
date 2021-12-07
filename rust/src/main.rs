// SPDX-License-Identifier: GPL-3.0-only

mod app;
mod days;
mod macros;
mod util;

fn main() {
    let app = app::parse_options();
    if let Err(error) = app.run() {
        println!("Something went wrong while solving the problem: {}", error);
    }
}
