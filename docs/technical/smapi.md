&larr; [README](../README.md)

This file provides more technical documentation about SMAPI. If you only want to use or create
mods, this section isn't relevant to you; see the main README to use or create mods.

This document is about SMAPI itself; see also [mod build package](mod-package.md) and
[web services](web.md).

# Contents
* [Customisation](#customisation)
  * [Configuration file](#configuration-file)
  * [Command-line arguments](#command-line-arguments)
  * [Compile flags](#compile-flags)
* [For SMAPI developers](#for-smapi-developers)
  * [Compiling from source](#compiling-from-source)
  * [Debugging a local build](#debugging-a-local-build)
  * [Preparing a release](#preparing-a-release)
  * [Using a custom Harmony build](#using-a-custom-harmony-build)
* [Release notes](#release-notes)

## Customisation
### Configuration file
You can customise some SMAPI behaviour by editing the `smapi-internal/config.json` file in your
game folder. See documentation in the file for more info.

### Command-line arguments
The SMAPI installer recognises three command-line arguments:

argument | purpose
-------- | -------
`--install` | Preselects the install action, skipping the prompt asking what the user wants to do.
`--uninstall` | Preselects the uninstall action, skipping the prompt asking what the user wants to do.
`--game-path "path"` | Specifies the full path to the folder containing the Stardew Valley executable, skipping automatic detection and any prompt to choose a path. If the path is not valid, the installer displays an error.

SMAPI itself recognises two arguments **on Windows only**, but these are intended for internal use
or testing and may change without warning. On Linux/macOS, see _environment variables_ below.

argument | purpose
-------- | -------
`--no-terminal` | SMAPI won't write anything to the console window. (Messages will still be written to the log file.)
`--mods-path` | The path to search for mods, if not the standard `Mods` folder. This can be a path relative to the game folder (like `--mods-path "Mods (test)"`) or an absolute path.

### Environment variables
The above SMAPI arguments don't work on Linux/macOS due to the way the game launcher works. You can
set temporary environment variables instead. For example:
> SMAPI_MODS_PATH="Mods (multiplayer)" /path/to/StardewValley

environment variable | purpose
-------------------- | -------
`SMAPI_NO_TERMINAL` | Equivalent to `--no-terminal` above.
`SMAPI_MODS_PATH` | Equivalent to `--mods-path` above.

### Compile flags
SMAPI uses a small number of conditional compilation constants, which you can set by editing the
`<DefineConstants>` element in `SMAPI.csproj`. Supported constants:

flag | purpose
---- | -------
`SMAPI_FOR_WINDOWS` | Whether SMAPI is being compiled for Windows; if not set, the code assumes Linux/macOS. Set automatically in `common.targets`.
`SMAPI_FOR_XNA` | Whether SMAPI is being compiled for XNA Framework; if not set, the code assumes MonoGame. Set automatically in `common.targets` with the same value as `SMAPI_FOR_WINDOWS`.

## For SMAPI developers
### Compiling from source
Using an official SMAPI release is recommended for most users, but you can compile from source
directly if needed. There are no special steps (just open the project and compile), but SMAPI often
uses the latest C# syntax. You may need the latest version of your IDE to compile it.

SMAPI uses build configuration derived from the [crossplatform mod config](https://smapi.io/package/readme)
to detect your current OS automatically and load the correct references. Compile output will be
placed in a `bin` folder at the root of the Git repository.

### Debugging a local build
Rebuilding the solution in debug mode will copy the SMAPI files into your game folder. Starting
the `SMAPI` project with debugging from Visual Studio (on macOS or Windows) will launch SMAPI with
the debugger attached, so you can intercept errors and step through the code being executed. That
doesn't work in MonoDevelop on Linux, unfortunately.

### Preparing a release
To prepare a crossplatform SMAPI release, you'll need to compile it on two platforms: Windows and
Linux. The instructions below assume you have Windows 11, but you can adapt them for
a different setup if needed.

#### Initial setup
First-time setup on Windows:
1. [Install Windows Subsystem for Linux (WSL)](https://docs.microsoft.com/en-us/windows/wsl/install).
2. Install the needed software in WSL:
   1. Run `sudo apt update` to update the package list.
   2. Install [the .NET 5 SDK](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu)
      (for Stardew Valley 1.5.5+) or [`mono-complete`](https://www.mono-project.com/download/stable/)
      (for earlier versions).
      _You can run `lsb_release -a` to get the Ubuntu version number._
   3. [Install Steam](https://linuxconfig.org/how-to-install-steam-on-ubuntu-20-04-focal-fossa-linux).
   4. Launch `steam` and install the game like usual.
   5. Download and install your preferred IDE. For the [latest standalone Rider
      version](https://www.jetbrains.com/help/rider/Installation_guide.html#prerequisites):
      ```sh
      wget "<download url here>" -O rider-install.tar.gz
      sudo tar -xzvf rider-install.tar.gz -C /opt
      ln -s "/opt/JetBrains Rider-<version>/bin/rider.sh"
      ./rider.sh
      ```
   3. Clone the SMAPI repo in WSL:
      ```sh
      git clone https://github.com/Pathoschild/SMAPI.git
      ```

To compile SMAPI in WSL:
1. Run `./rider.sh` to open the Rider GUI.
2. Use the GUI to compile the solution.

To launch the game:
1. Open a WSL terminal.
2. Run these commands to start Steam:
   ```sh
   export TERM=xterm
   steam
   ```
3. Launch the game through the Steam UI.

#### Prepare the release
1. Update the version numbers in `build/common.targets`, `Constants`, and the `manifest.json` for
   bundled mods. Make sure you use a [semantic version](https://semver.org). Recommended format:

   build type | format                   | example
   :--------- | :----------------------- | :------
   dev build  | `<version>-alpha.<date>` | `3.0.0-alpha.20171230`
   prerelease | `<version>-beta.<date>`  | `3.0.0-beta.20171230`
   release    | `<version>`              | `3.0.0`
2. In Windows:
   1. Rebuild the solution with the _release_ solution configuration.
   2. Copy the `bin/SMAPI installer` and `bin/SMAPI installer for developers` folders to Linux.
4. In Linux:
   1. Rebuild the solution with the _release_ solution configuration.
   2. Add the `windows-install.*` files from Windows to the `bin/SMAPI installer` and
      `bin/SMAPI installer for developers` folders compiled on Linux.
   3. Rename the folders to `SMAPI <version> installer` and `SMAPI <version> installer for developers`.
   4. Zip the two folders.

### Custom Harmony build
SMAPI uses [a custom build of Harmony](https://github.com/Pathoschild/Harmony#readme), which is
included in the `build` folder. To use a different build, just replace `0Harmony.dll` in that
folder before compiling.

## Release notes
See [release notes](../release-notes.md).
