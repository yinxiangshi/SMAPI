<<<<<<< HEAD
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
* [Intro & FAQs](http://stardewvalleywiki.com/Modding:Player_FAQs)
* [Installing SMAPI](http://stardewvalleywiki.com/Modding:Installing_SMAPI)
* [Release notes](release-notes.md#release-notes)
* Need help? Come [chat on Discord](https://discord.gg/KCJHWhX) or [post in the support forums](http://community.playstarbound.com/threads/smapi-stardew-modding-api.108375/).  
  _Please don't submit issues on GitHub for support questions._

## For mod developers
* [Modding documentation](http://stardewvalleywiki.com/Modding:Index)
* [Release notes](release-notes.md#release-notes)
* [Chat on Discord](https://discord.gg/KCJHWhX) with SMAPI developers and other modders

## For SMAPI developers
_This section is about compiling SMAPI itself from source. If you don't know what that means, this
section isn't relevant to you; see the previous sections to use or create mods._

### Compiling from source
Using an official SMAPI release is recommended for most users.

SMAPI uses some C# 7 code, so you'll need at least
[Visual Studio 2017](https://www.visualstudio.com/vs/community/) on Windows,
[MonoDevelop 7.0](http://www.monodevelop.com/) on Linux,
[Visual Studio 2017 for Mac](https://www.visualstudio.com/vs/visual-studio-mac/), or an equivalent
IDE to compile it. It uses build configuration derived from the
[crossplatform mod config](https://github.com/Pathoschild/Stardew.ModBuildConfig#readme) to detect
your current OS automatically and load the correct references. Compile output will be placed in a
`bin` folder at the root of the git repository.

### Debugging a local build
Rebuilding the solution in debug mode will copy the SMAPI files into your game folder. Starting
the `StardewModdingAPI` project with debugging from Visual Studio (on Mac or Windows) will launch
SMAPI with the debugger attached, so you can intercept errors and step through the code being
executed. This doesn't work in MonoDevelop on Linux, unfortunately.

### Preparing a release
To prepare a crossplatform SMAPI release, you'll need to compile it on two platforms. See
[crossplatforming info](http://stardewvalleywiki.com/Modding:Creating_a_SMAPI_mod#Test_on_all_platforms)
on the wiki for the first-time setup.

1. Update the version number in `GlobalAssemblyInfo.cs` and `Constants::Version`. Make sure you use a
   [semantic version](http://semver.org). Recommended format:

   build type | format                            | example
   :--------- | :-------------------------------- | :------
   dev build  | `<version>-alpha.<timestamp>`     | `2.0-alpha.20171230`
   prerelease | `<version>-prerelease.<ID>`       | `2.0-prerelease.2`
   release    | `<version>`                       | `2.0`

2. In Windows:
   1. Rebuild the solution in _Release_ mode.
   2. Rename `bin/Packaged` to `SMAPI <version>` (e.g. `SMAPI 2.0`).
   2. Transfer the `SMAPI <version>` folder to Linux or Mac.  
      _This adds the installer executable and Windows files. We'll do the rest in Linux or Mac,
      since we need to set Unix file permissions that Windows won't save._

2. In Linux or Mac:
   1. Rebuild the solution in _Release_ mode.
   2. Copy `bin/internal/Packaged/Mono` into the `SMAPI <version>` folder.
   3. If you did everything right so far, you should have a folder like this:

      ```
      SMAPI-2.x/
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
               StardewModdingAPI.pdb
               StardewModdingAPI.xml
               steam_appid.txt
               System.Numerics.dll
               System.Runtime.Caching.dll
               System.ValueTuple.dll
            Windows/
               Mods/*
               Mono.Cecil.dll
               Newtonsoft.Json.dll
               StardewModdingAPI.AssemblyRewriters.dll
               StardewModdingAPI.config.json
               StardewModdingAPI.exe
               StardewModdingAPI.pdb
               StardewModdingAPI.xml
               System.ValueTuple.dll
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
game folder.

Basic fields:

field             | purpose
----------------- | -------
`DeveloperMode`   | Default `false` (except in _SMAPI for developers_ releases). Whether to enable features intended for mod developers (mainly more detailed console logging).
`CheckForUpdates` | Default `true`. Whether SMAPI should check for a newer version when you load the game. If a new version is available, a small message will appear in the console. This doesn't affect the load time even if your connection is offline or slow, because it happens in the background.
`VerboseLogging`  | Default `false`. Whether SMAPI should log more information about the game context.
`ModData`         | Internal metadata about SMAPI mods. Changing this isn't recommended and may destabilise your game. See documentation in the file.

### Command-line arguments
The SMAPI installer recognises three command-line arguments:

argument | purpose
-------- | -------
`--install` | Preselects the install action, skipping the prompt asking what the user wants to do.
`--uninstall` | Preselects the uninstall action, skipping the prompt asking what the user wants to do.
`--game-path "path"` | Specifies the full path to the folder containing the Stardew Valley executable, skipping automatic detection and any prompt to choose a path. If the path is not valid, the installer displays an error.

SMAPI itself recognises two arguments, but these are intended for internal use or testing and may
change without warning.

argument | purpose
-------- | -------
`--log-path "path"` | The relative or absolute path of the log file SMAPI should write.
`--no-terminal` | SMAPI won't write anything to the console window. (Messages will still be written to the log file.)

### Compile flags
SMAPI uses a small number of conditional compilation constants, which you can set by editing the
`<DefineConstants>` element in `StardewModdingAPI.csproj`. Supported constants:

flag | purpose
---- | -------
`SMAPI_FOR_WINDOWS` | Indicates that SMAPI is being compiled on Windows for players on Windows. Set automatically in `crossplatform.targets`.

=======
﻿**Stardew.ModBuildConfig** is an open-source NuGet package which automates the build configuration
for [Stardew Valley](http://stardewvalley.net/) [SMAPI](https://github.com/Pathoschild/SMAPI) mods.

The package...

* lets you write your mod once, and compile it on any computer. It detects the current platform
  (Linux, Mac, or Windows) and game install path, and injects the right references automatically.
* configures Visual Studio so you can debug into the mod code when the game is running (_Windows
  only_).
* packages the mod automatically into the game's mod folder when you build the code (_optional_).

## Contents
* [Install](#install)
* [Simplify mod development](#simplify-mod-development)
* [Troubleshoot](#troubleshoot)
* [Versions](#versions)

## Install
**When creating a new mod:**

1. Create an empty library project.
2. Reference the [`Pathoschild.Stardew.ModBuildConfig` NuGet package](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig).
3. [Write your code](http://canimod.com/guides/creating-a-smapi-mod).
4. Compile on any platform.

**When migrating an existing mod:**

1. Remove any project references to `Microsoft.Xna.*`, `MonoGame`, Stardew Valley,
   `StardewModdingAPI`, and `xTile`.
2. Reference the [`Pathoschild.Stardew.ModBuildConfig` NuGet package](https://www.nuget.org/packages/Pathoschild.Stardew.ModBuildConfig).
3. Compile on any platform.

## Simplify mod development
### Package your mod into the game folder automatically
You can copy your mod files into the `Mods` folder automatically each time you build, so you don't
need to do it manually:

1. Edit your mod's `.csproj` file.
2. Add this block above the first `</PropertyGroup>` line:

     ```xml
     <DeployModFolderName>$(MSBuildProjectName)</DeployModFolderName>
     ```

That's it! Each time you build, the files in `<game path>\Mods\<mod name>` will be updated with
your `manifest.json`, build output, and any `i18n` files.

Notes:
* To add custom files, just [add them to the build output](https://stackoverflow.com/a/10828462/262123).
* To customise the folder name, just replace `$(MSBuildProjectName)` with the folder name you want.
* If your project references another mod, make sure the reference is [_not_ marked 'copy local'](https://msdn.microsoft.com/en-us/library/t1zz5y8c(v=vs.100).aspx).

### Debug into the mod code (Windows-only)
Stepping into your mod code when the game is running is straightforward, since this package injects
the configuration automatically. To do it:

1. [Package your mod into the game folder automatically](#package-your-mod-into-the-game-folder-automatically).
2. Launch the project with debugging in Visual Studio or MonoDevelop.

This will deploy your mod files into the game folder, launch SMAPI, and attach a debugger
automatically. Now you can step through your code, set breakpoints, etc.

### Create release zips automatically (Windows-only)
You can create the mod package automatically when you build:

1. Edit your mod's `.csproj` file.
2. Add this block above the first `</PropertyGroup>` line:

     ```xml
     <DeployModZipTo>$(SolutionDir)\_releases</DeployModZipTo>
     ```

That's it! Each time you build, the mod files will be zipped into `_releases\<mod name>.zip`. (You
can change the value to save the zips somewhere else.)

## Troubleshoot
### "Failed to find the game install path"
That error means the package couldn't figure out where the game is installed. You need to specify
the game location yourself. There's two ways to do that:

* **Option 1: set the path globally.**  
  _This will apply to every project that uses version 1.5+ of package._

  1. Get the full folder path containing the Stardew Valley executable.
  2. Create this file path:
  
     platform  | path
     --------- | ----
     Linux/Mac | `~/stardewvalley.targets`
     Windows   | `%USERPROFILE%\stardewvalley.targets`

  3. Save the file with this content:

     ```xml
     <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
        <PropertyGroup>
          <GamePath>PATH_HERE</GamePath>
        </PropertyGroup>
     </Project>
     ```

  4. Replace `PATH_HERE` with your custom game install path.

* **Option 2: set the path in the project file.**  
  _(You'll need to do it for every project that uses the package.)_
  1. Get the folder path containing the Stardew Valley `.exe` file.
  2. Add this to your `.csproj` file under the `<Project` line:

     ```xml
     <PropertyGroup>
       <GamePath>PATH_HERE</GamePath>
     </PropertyGroup>
     ```

  3. Replace `PATH_HERE` with your custom game install path.

The configuration will check your custom path first, then fall back to the default paths (so it'll
still compile on a different computer).

## Versions
See [release notes](release-notes.md).
>>>>>>> mod-build-config/develop
