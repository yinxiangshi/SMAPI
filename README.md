![](https://raw.githubusercontent.com/Gormogon/SMAPI/master/docs/imgs/SMAPI.png)

**SMAPI** is an [open-source](LICENSE) modding API for [Stardew Valley](http://stardewvalley.net/).
It takes care of loading mods into the game context, and exposes events they can use to interact
with the game. It's safely installed alongside the game's executable, and doesn't change any of
your game files.

## For players

* [How to install SMAPI & use mods](http://canimod.com/guides/using-mods#installing-smapi)
* [Support forums](http://community.playstarbound.com/threads/stardew-modding-api-0-40-1-1.108375/)
* [Stardew Valley Discord](https://discord.gg/KCJHWhX) (chat with players and developers)

If you need help, [ask in this forum thread](http://community.playstarbound.com/threads/stardew-modding-api-0-40-1-1.108375)
or [come talk to us on Discord](https://discord.gg/KCJHWhX). Your question will be seen by more
people who can help that way. (Please don't submit issues on GitHub for support questions.)

## For mod developers

* [How to develop mods](http://canimod.com/guides/creating-a-smapi-mod)
* [Release notes](release-notes.md)
* [SMAPI/Farmhand Discord](https://discordapp.com/invite/0t3fh2xhHVc6Vdyx) (chat with SMAPI developers)

## For SMAPI developers
_This section is about compiling SMAPI itself from source. If you don't know what that means, this
section isn't relevant to you; see the previous sections to use or create mods._

### Compiling from source
Using an official SMAPI release is recommended for most users.

If you'd like to compile SMAPI from source, you can do that on any platform using
[Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](http://www.monodevelop.com/).
SMAPI uses build configuration derived from the [crosswiki mod config](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
to detect your current OS automatically and load the correct references. Compile output will be
placed in a `bin` directory at the root of the git repository.

### Debugging a local build
Rebuilding the solution in debug mode will copy the SMAPI files into your game directory. Starting
the `StardewModdingAPI` project with debugging will launch SMAPI with the debugger attached, so you
can intercept errors and step through the code being executed.

### Preparing a release
To prepare a crossplatform SMAPI release, you'll need to compile it on two platforms. See
_[crossplatforming a SMAPI mod](http://canimod.com/guides/crossplatforming-a-smapi-mod#preparing-a-mod-release)_
for the first-time setup. For simplicity, all paths are relative to the root of the repository (the
directory containing `src`).

1. Update the version number in `GlobalAssemblyInfo.cs` and `Constants::Version`. Make sure you use a
   [semantic version](http://semver.org). Recommended format:

   build type | format                            | example
   :--------- | :-------------------------------- | :------
   dev build  | `<version>-alpha-<timestamp>`     | `1.0.0-alpha-201611300500`
   beta       | `<version>-beta<incrementing ID>` | `1.0.0-beta2`
   release    | `<version>`                       | `1.0.0`

2. In Windows:
   1. Rebuild the solution in _Release_ mode.
   2. Rename `bin/Packaged` to `SMAPI-<version>` (e.g. `SMAPI-1.0`).
   2. Transfer the `SMAPI-<version>` directory to Linux or Mac.  
      _This adds the installer executable and Windows files. We'll do the rest in Linux or Mac,
      since we need to set Unix file permissions that Windows won't save._

2. In Linux or Mac:
   1. Rebuild the solution in _Release_ mode.
   2. Copy `bin/Packaged/Mono` into the `SMAPI-<version>` directory.
   3. If you did everything right so far, you should have a directory like this:

      ```
      SMAPI-1.0/
         Mono/
            Mods/*
            Newtonsoft.Json.dll
            StardewModdingAPI
            StardewModdingAPI.exe
            StardewModdingAPI.exe.mdb
            StardewModdingAPI-settings.json
            System.Numerics.dll
            steam_appid.txt
         Windows/
            Mods/*
            StardewModdingAPI.exe
            StardewModdingAPI.pdb
            StardewModdingAPI.xml
            StardewModdingAPI-settings.json
            steam_appid.txt
         install.exe
         readme.txt
      ```
   4. Open a terminal in the `SMAPI-<version>` directory and run `chmod 755 Mono/StardewModdingAPI`.
   5. Copy & paste the `SMAPI-<version>` directory as `SMAPI-<version>-for-developers`.
   6. In the `SMAPI-<version>` directory, delete the following files:
      * `Mono/StardewModdingAPI-settings.json`
      * `Windows/StardewModdingAPI.xml`
      * `Windows/StardewModdingAPI-settings.json`
   7. Compress the two folders into `SMAPI-<version>.zip` and `SMAPI-<version>-for-developers.zip`.