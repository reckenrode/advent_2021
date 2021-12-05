// SPDX-License-Identifier: GPL-3.0-only

use std::{fmt::Display, iter::repeat, str};

use anyhow::{ensure, Result};

pub(crate) struct Bitmap {
    width: u16,
    height: u16,
    data: Vec<u8>,
}

impl Bitmap {
    pub(crate) fn new(width: u16, height: u16) -> Bitmap {
        Bitmap {
            width,
            height,
            data: repeat(0).take(width as usize * height as usize).collect(),
        }
    }

    pub(crate) fn width(&self) -> u16 {
        self.width
    }

    pub(crate) fn height(&self) -> u16 {
        self.height
    }

    pub(crate) fn draw(&mut self, x: u16, y: u16) -> Result<()> {
        ensure!(x < self.width() && y < self.height());
        let width = self.width() as usize;
        self.data[x as usize + y as usize * width] += 1;
        Ok(())
    }

    pub(crate) fn iter<'a>(&'a self) -> impl Iterator<Item = u8> + 'a {
        self.data.iter().cloned()
    }

    fn render_span(&self, begin: usize, length: u16, buf: &mut [u8]) {
        let length = length as usize;
        for (idx, pixel) in self.data[begin..(begin + length)]
            .iter()
            .enumerate()
        {
            match pixel {
                0 => buf[idx] = '.' as u8,
                _ => buf[idx] = pixel + ('0' as u8),
            };
        }
    }
}

impl IntoIterator for Bitmap {
    type Item = u8;

    type IntoIter = <Vec<u8> as IntoIterator>::IntoIter;

    fn into_iter(self) -> Self::IntoIter {
        self.data.into_iter()
    }
}

impl Display for Bitmap {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        if self.width() > 0 && self.height() > 0 {
            let mut buffer: Vec<u8> = Vec::with_capacity(self.width() as usize);
            buffer.resize(self.width() as usize, 0);

            // Only valid UTF-8 characters are written to buffer, so create it unchecked to avoid
            // having to deal with propagating the error up.
            self.render_span(0, self.width(), &mut buffer);
            f.write_fmt(format_args!("{}", unsafe {
                str::from_utf8_unchecked(&buffer)
            }))?;

            for row in 1..self.height() {
                let begin = row as usize * self.width() as usize;
                self.render_span(begin, self.width(), &mut buffer);
                f.write_fmt(format_args!("\n{}", unsafe {
                    str::from_utf8_unchecked(&buffer)
                }))?;
            }
        }
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn new_bitmap_has_specified_width_and_height() {
        let expected_size = (640, 480);
        let bitmap = Bitmap::new(640, 480);
        assert_eq!((bitmap.width(), bitmap.height()), expected_size);
    }

    #[test]
    fn bitmap_with_zero_width_is_an_empty_string() {
        let expected_output = "";
        let bitmap = Bitmap::new(0, 3);
        let output = format!("{}", bitmap);
        assert_eq!(output, expected_output);
    }

    #[test]
    fn bitmap_with_zero_height_is_an_empty_string() {
        let expected_output = "";
        let bitmap = Bitmap::new(4, 0);
        let output = format!("{}", bitmap);
        assert_eq!(output, expected_output);
    }

    #[test]
    fn empty_bitmap_is_all_dots() {
        let expected_output = "....\n....\n....";
        let bitmap = Bitmap::new(4, 3);
        let output = format!("{}", bitmap);
        assert_eq!(output, expected_output);
    }

    #[test]
    fn draw_outside_of_width_bounds_returns_an_error() {
        let expected_error = "Condition failed: `x < self.width() && y < self.height()`";
        let mut bitmap = Bitmap::new(4, 3);
        let error = bitmap.draw(5, 0).unwrap_err();
        assert_eq!(error.to_string(), expected_error);
    }

    #[test]
    fn draw_outside_of_height_bounds_returns_an_error() {
        let expected_error = "Condition failed: `x < self.width() && y < self.height()`";
        let mut bitmap = Bitmap::new(4, 3);
        let error = bitmap.draw(0, 5).unwrap_err();
        assert_eq!(error.to_string(), expected_error);
    }

    #[test]
    fn draw_puts_a_one_on_an_empty_space() -> Result<()> {
        let expected_output = ".1..\n....\n....";
        let mut bitmap = Bitmap::new(4, 3);
        bitmap.draw(1, 0)?;
        let output = format!("{}", bitmap);
        assert_eq!(output, expected_output);
        Ok(())
    }

    #[test]
    fn draw_increments_the_value_when_drawn_twice() -> Result<()> {
        let expected_output = ".1..\n.2..\n....";
        let mut bitmap = Bitmap::new(4, 3);
        bitmap.draw(1, 0)?;
        bitmap.draw(1, 1)?;
        bitmap.draw(1, 1)?;
        let output = format!("{}", bitmap);
        assert_eq!(output, expected_output);
        Ok(())
    }

    #[test]
    fn iter_gets_an_iterator_to_the_raw_data() -> Result<()> {
        let expected_output = vec![0, 1, 0, 0];
        let mut bitmap = Bitmap::new(2, 2);
        bitmap.draw(1, 0)?;
        let output: Vec<u8> = bitmap.iter().collect();
        assert_eq!(output, expected_output);
        Ok(())
    }
}
