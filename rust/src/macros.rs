// SPDX-License-Identifier: GPL-3.0-only

#[macro_export]
macro_rules! declare_days {
    ( $( $x:ident), * ) => {
        declare_days! $x
    };
    ( $( $x:ident, )* ) => {
        paste::paste! {
            $(
                mod [<$x:lower>];
            )*
            #[derive(clap::Subcommand)]
            pub(crate) enum Command {
                $(
                    $x(crate::days::[<$x:lower>]::$x),
                )*
            }
        }
        impl Command {
            pub(crate) fn run(self) -> anyhow::Result<()> {
                match self {
                    $(
                        Command::$x(day) => day.run(),
                    )*
                }
            }
        }
    };
}
