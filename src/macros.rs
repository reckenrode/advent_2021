// SPDX-License-Identifier: GPL-3.0-only

#[macro_export]
macro_rules! declare_days {
    ( $( $x:ident), * ) => {
        declare_days! $x
    };
    ( $( $x:ident, )* ) => {
        #[derive(clap::Subcommand)]
        enum Command {
            $(
                $x(crate::days::$x),
            )*
        }
        impl App {
            pub(crate) fn run(self) -> anyhow::Result<()> {
                match self.cmd {
                    $(
                        Command::$x(day) => day.run(),
                    )*
                }
            }
        }
    };
}
