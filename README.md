![](https://raw.githubusercontent.com/Gormogon/SMAPI/master/docs/imgs/SMAPI.png)

**SMAPI** is an [open-source](LICENSE) modding API for [Stardew Valley](http://stardewvalley.net/).
It takes care of loading mods into the game context, and exposes events they can use to interact
with the game. It's safely installed alongside the game's executable, and doesn't change any of
your game files.

## For players

* [How to install SMAPI & use mods](http://canimod.com/guides/using-mods#installing-smapi)
* [Support forums](http://community.playstarbound.com/threads/stardew-modding-api-0-40-1-1.108375/)
* [Stardew Valley Discord](https://discord.gg/KCJHWhX) (chat with players and developers)

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

### Preparing a release
To prepare a crossplatform SMAPI release, you'll need to compile it on two platforms. See
_[crossplatforming a SMAPI mod](http://canimod.com/guides/crossplatforming-a-smapi-mod#preparing-a-mod-release)_
for the first-time setup.

For simplicity, all paths are relative to the root of the repository (the directory containing `src`).

1. Update the version number in `AssemblyInfo.cs` and `Constants::Version`. Make sure you use a
   [semantic version](http://semver.org). Recommended format:

   build type | format                            | example
   :--------- | :-------------------------------- | :------
   dev build  | `<version>-alpha-<timestamp>`     | `1.0.0-alpha-201611300500`
   beta       | `<version>-beta<incrementing ID>` | `1.0.0-beta2`
   release    | `<version>`                       | `1.0.0`

2. In Windows:
   1. Rebuild the solution in _Release_ mode.
   2. Transfer the `bin/Release/~Package` directory to Linux or Mac.  
      _This adds the installer executable and Windows files. We'll do the rest in Linux or Mac,
      since we need to set Unix file permissions that Windows won't save._

2. In Linux or Mac:
   1. Rebuild the solution in _Release_ mode.
   2. Copy `bin/Release/~Package/Mono` into the package directory you transferred from Windows.
   3. Open a terminal in your package directory and run `chmod 755 Mono/StardewModdingAPI`.
   4. Rename your package directory to `SMAPI-<version>-installer`.
   5. Compress it into a `SMAPI-<version>.zip` file.

If you did everything right, you should have a release file like this:

```
SMAPI-1.0.zip
   SMAPI-1.0-installer/
      Mono/
         Mods/*
         Newtonsoft.Json.dll
         StardewModdingAPI
         StardewModdingAPI.exe
         StardewModdingAPI.exe.mdb
         System.Numerics.dll
         steam_appid.txt
      Windows/
         Mods/*
         StardewModdingAPI.exe
         StardewModdingAPI.pdb
         steam_appid.txt
      install.exe
      readme.txt
```
