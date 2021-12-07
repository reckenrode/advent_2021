// SPDX-License-Identifier: GPL-3.0-only

use anyhow::Result;
use std::{
    fs::File,
    io::{BufReader, Read},
    path::Path,
};

pub(crate) fn read_input(path: impl AsRef<Path>) -> Result<String> {
    let inner = File::open(path.as_ref())?;
    let mut reader = BufReader::new(inner);
    let mut buf = String::new();
    reader.read_to_string(&mut buf)?;
    Ok(buf)
}
