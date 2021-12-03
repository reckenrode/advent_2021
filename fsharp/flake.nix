{
  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
  inputs.utils.url = "github:gytis-ivaskevicius/flake-utils-plus/v1.3.0";

  outputs = inputs@{ self, utils, ... }:
    utils.lib.mkFlake {
      inherit self inputs;

      outputsBuilder = channels:
        let
	  inherit (channels) nixpkgs;
	in
        rec {
          devShell = nixpkgs.mkShell {
  	    nativeBuildInputs = [ nixpkgs.dotnetCorePackages.sdk_6_0 ];
  	  };
        };
    };
}
