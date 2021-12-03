My Advent of Code 2021 solutions — written in F# and Rust.

# Requirements

- [.NET 6.0 SDK][1] for your platform;
- [Rust 1.56][2] or newer; and
- (Optionally) [Nix][3] and [direnv][4]


# Building (F#)

After cloning the repository, you will need to run `dotnet tool restore` and `dotnet paket restore`
to download and set up the required dependencies.  If you have `nix` and `direnv`, you can run
`direnv allow` to download and set up everything you need.  In this case, you won’t even need to
install the .NET SDK.  If you only have `nix`, then `nix develop` will get you a developer shell
with everything.

# Building (Rust)

Just use `cargo build`.  Like with F#, `direnv` will set up an environment for you.

# Running

See `Advent2021 --help` for how to run the various solutions.

[1]: https://dotnet.microsoft.com/download/dotnet/6.0
[2]: https://rustup.rs
[3]: https://nixos.org
[4]: https://direnv.net
