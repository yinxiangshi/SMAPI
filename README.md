![](docs/imgs/SMAPI.png)

## Contents
* [What is SMAPI?](#what-is-smapi)
* **[For players](#for-players)**
* **[For mod developers](#for-mod-developers)**
* [For SMAPI developers](#for-smapi-developers)
  * [Compiling from source](#compiling-from-source)
  * [Debugging a local build](#debugging-a-local-build)
  * [Preparing a release](#preparing-a-release)
* [Advanced usage](#advanced-usage)
  * [Configuration file](#configuration-file)
  * [Command-line arguments](#command-line-arguments)

## What is SMAPI?
**SMAPI** is an [open-source](LICENSE) modding API for [Stardew Valley](http://stardewvalley.net/)
that lets you play the game with mods. It's safely installed alongside the game's executable, and
doesn't change any of your game files. It serves five main purposes:

1. **Load mods into the game.**  
   _SMAPI loads mods when the game is starting up so they can interact with it. (Code mods aren't
   possible without SMAPI to load them.)_

2. **Provide APIs and events for mods.**  
   _SMAPI provides low-level APIs and events which let mods interact with the game in ways they
   otherwise couldn't._

3. **Rewrite mods for crossplatform compatibility.**  
   _SMAPI rewrites mods' compiled code before loading them so they work on Linux/Mac/Windows
   without the mods needing to handle differences between the Linux/Mac and Windows versions of the
   game._

4. **Rewrite mods to update them.**  
   _SMAPI detects when a mod accesses part of the game that changed in a recent update which
   affects many mods, and rewrites the mod so it's compatible._

5. **Intercept errors.**  
   _SMAPI intercepts errors that happen in the game, displays the error details in the console
   window, and in most cases automatically recovers the game. This prevents mods from accidentally
   crashing the game, and makes it possible to troubleshoot errors in the game itself that would
   otherwise show a generic 'program has stopped working' type of message._

## For players
* [How to install SMAPI & use mods](http://canimod.com/guides/using-mods#installing-smapi)
* [Release notes](release-notes.md#release-notes)
* Need help? Come [chat on Discord](https://discord.gg/KCJHWhX) or [post in the support forums](http://community.playstarbound.com/threads/smapi-stardew-modding-api.108375/).  
  _Please don't submit issues on GitHub for support questions._

## For mod developers
* [How to develop mods](http://canimod.com/guides/creating-a-smapi-mod)
* [How to update mods](http://canimod.com/guides/updating-a-smapi-mod)
* [Release notes](release-notes.md#release-notes)
* [Chat on Discord](https://discord.gg/KCJHWhX) with SMAPI developers and other modders

## For SMAPI developers
_This section is about compiling SMAPI itself from source. If you don't know what that means, this
section isn't relevant to you; see the previous sections to use or create mods._

### Compiling from source
Using an official SMAPI release is recommended for most users.

If you'd like to compile SMAPI from source, you can do that on any platform using
[Visual Studio](https://www.visualstudio.com/vs/community/) or [MonoDevelop](http://www.monodevelop.com/).
SMAPI uses build configuration derived from the [crosswiki mod config](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme)
to detect your current OS automatically and load the correct references. Compile output will be
placed in a `bin` folder at the root of the git repository.

### Debugging a local build
Rebuilding the solution in debug mode will copy the SMAPI files into your game folder. Starting
the `StardewModdingAPI` project with debugging will launch SMAPI with the debugger attached, so you
can intercept errors and step through the code being executed.

### Preparing a release
To prepare a crossplatform SMAPI release, you'll need to compile it on two platforms. See
_[crossplatforming a SMAPI mod](http://canimod.com/guides/crossplatforming-a-smapi-mod#preparing-a-mod-release)_
for the first-time setup. For simplicity, all paths are relative to the root of the repository (the
folder containing `src`).

1. Update the version number in `GlobalAssemblyInfo.cs` and `Constants::Version`. Make sure you use a
   [semantic version](http://semver.org). Recommended format:

   build type | format                            | example
   :--------- | :-------------------------------- | :------
   dev build  | `<version>-alpha.<timestamp>`     | `1.0.0-alpha.20171230`
   beta       | `<version>-beta.<incrementing ID>`| `1.0.0-beta`, `1.0.0-beta.2`, …
   release    | `<version>`                       | `1.0.0`

2. In Windows:
   1. Rebuild the solution in _Release_ mode.
   2. Rename `bin/Packaged` to `SMAPI <version>` (e.g. `SMAPI 1.6`).
   2. Transfer the `SMAPI <version>` folder to Linux or Mac.  
      _This adds the installer executable and Windows files. We'll do the rest in Linux or Mac,
      since we need to set Unix file permissions that Windows won't save._

2. In Linux or Mac:
   1. Rebuild the solution in _Release_ mode.
   2. Copy `bin/internal/Packaged/Mono` into the `SMAPI <version>` folder.
   3. If you did everything right so far, you should have a folder like this:

      ```
      SMAPI-1.x/
         install.exe
         readme.txt
         internal/
            Mono/
               Mods/*
               Mono.Cecil.dll
               Newtonsoft.Json.dll
               StardewModdingAPI
               StardewModdingAPI.AssemblyRewriters.dll
               StardewModdingAPI.config.json
               StardewModdingAPI.exe
               StardewModdingAPI.exe.mdb
               steam_appid.txt
               System.Numerics.dll
               System.Runtime.Caching.dll
            Windows/
               Mods/*
               Mono.Cecil.dll
               Newtonsoft.Json.dll
               StardewModdingAPI.AssemblyRewriters.dll
               StardewModdingAPI.config.json
               StardewModdingAPI.exe
               StardewModdingAPI.pdb
               StardewModdingAPI.xml
               steam_appid.txt
      ```
   4. Open a terminal in the `SMAPI <version>` folder and run `chmod 755 internal/Mono/StardewModdingAPI`.
   5. Copy & paste the `SMAPI <version>` folder as `SMAPI <version> for developers`.
   6. In the `SMAPI <version>` folder...
      * edit `internal/Mono/StardewModdingAPI.config.json` and
	    `internal/Windows/StardewModdingAPI.config.json` to disable developer mode;
      * delete `internal/Windows/StardewModdingAPI.xml`.
   7. Compress the two folders into `SMAPI <version>.zip` and `SMAPI <version> for developers.zip`.

## Advanced usage
### Configuration file
You can customise the SMAPI behaviour by editing the `StardewModdingAPI.config.json` file in your
game folder. It contains these fields:

field | purpose
----- | -------
`DeveloperMode` | Default `false` (except in _SMAPI for developers_ releases). Whether to enable features intended for mod developers. Currently this only makes `TRACE`-level messages appear in the console.
`CheckForUpdates` | Default `true`. Whether SMAPI should check for a newer version when you load the game. If a new version is available, a small message will appear in the console. This doesn't affect the load time even if your connection is offline or slow, because it happens in the background.
`ModCompatibility` | A list of mod versions SMAPI should consider compatible or broken regardless of whether it detects incompatible code. Each record can be set to `AssumeCompatible` or `AssumeBroken`. Changing this field is not recommended and may destabilise your game.

### Command-line arguments
SMAPI recognises the following command-line arguments. These are intended for internal use or
testing and may change without warning.

argument | purpose
-------- | -------
`--log path "path"` | The relative or absolute path of the log file SMAPI should write.
`--no-terminal` | SMAPI won't write anything to the console window. (Messages will still be written to the log file.)
