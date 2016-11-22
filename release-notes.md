# Release notes

## 1.1
See [log](https://github.com/CLxS/SMAPI/compare/1.0...master).

For players:
  * Fixed console exiting immediately when some exceptions occur.
  * Fixed an error in 1.0 when mod uses `config.json` but the file doesn't exist.
  * Fixed critical errors being saved to a separate log file.
  * Fixed compatibility with some older mods.<sup>1.1.1, 1.1.2</sup>
  * Fixed race condition where some mods would sometimes crash because the game wasn't ready yet.<sup>1.1.1</sup>
  * Fixed errors in some mod event handlers crashing the game.<sup>1.1.2</sup>
  * Fixed issue where an error in one mod's event handler would cause other mods' handlers to never be called.<sup>1.1.2</sup>

For developers:
  * Added new logging interface:
    * easier to use;
    * supports trace logs (written to the log file, but hidden in the console by default);
    * messages are now listed in order;
    * messages now show which mod logged them;
    * more consistent and intuitive console color scheme.
  * Added optional `MinimumApiVersion` to `manifest.json`.
  * Added emergency interrupt feature for dangerous mods.
  * Fixed deprecation warnings being repeated if the mod can't be identified.<sup>1.1.1</sup>

## 1.0
See [log](https://github.com/CLxS/SMAPI/compare/0.40.1.1-3...1.0).

For players:
  * Added support for Linux and Mac.
  * Added installer to automate adding & removing SMAPI.
  * Added background update check on launch.
  * Fixed missing `steam_appid.txt` file.
  * Fixed some mod UIs disappearing at a non-default zoom level for some users.
  * Removed undocumented support for mods in AppData folder **(breaking change)**.
  * Removed `F2` debug mode.

For mod developers:
  * Added deprecation warnings.
  * Added OS version to log.
  * Added zoom-adjusted mouse position to mouse-changed event arguments.
  * Added SMAPI code documentation.
  * Switched to [semantic versioning](http://semver.org).
  * Fixed mod versions not shown correctly in the log.
  * Fixed misspelled field in `manifest.json` schema.
  * Fixed some events getting wrong data.
  * Simplified log output.

For SMAPI developers:
  * Simplified compiling from source.
  * Formalised release process and added automated build packaging.
  * Removed obsolete and unfinished code.
  * Internal cleanup & refactoring.

## 0.x
* 0.40.1.1 (2016-09-30, [log](https://github.com/CLxS/SMAPI/compare/0.40.0...0.40.1.1-3))
  * Added support for Stardew Valley 1.1.

* 0.40.0 (2016-04-05, [log](https://github.com/CLxS/SMAPI/compare/0.39.7...0.40.0))
  * Fixed an error that ocurred during minigames.

* 0.39.7 (2016-04-04, [log](https://github.com/CLxS/SMAPI/compare/0.39.6...0.39.7))
  * Added 'no check' graphics events that are triggered regardless of game's if checks.

* 0.39.6 (2016-04-01, [log](https://github.com/CLxS/SMAPI/compare/0.39.5...0.39.6))
  * Added game & SMAPI versions to log.
  * Fixed conflict in graphics tick events.
  * Bug fixes.

* 0.39.5 (2016-03-30, [log](https://github.com/CLxS/SMAPI/compare/0.39.4...0.39.5))
* 0.39.4 (2016-03-29, [log](https://github.com/CLxS/SMAPI/compare/0.39.3...0.39.4))
* 0.39.3 (2016-03-28, [log](https://github.com/CLxS/SMAPI/compare/0.39.2...0.39.3))
* 0.39.2 (2016-03-23, [log](https://github.com/CLxS/SMAPI/compare/0.39.1...0.39.2))
* 0.39.1 (2016-03-23, [log](https://github.com/CLxS/SMAPI/compare/0.38.8...0.39.1))
* 0.38.8 (2016-03-23, [log](https://github.com/CLxS/SMAPI/compare/0.38.7...0.38.8))
* 0.38.7 (2016-03-23, [log](https://github.com/CLxS/SMAPI/compare/0.38.6...0.38.7))
* 0.38.6 (2016-03-22, [log](https://github.com/CLxS/SMAPI/compare/0.38.5...0.38.6))
* 0.38.5 (2016-03-22, [log](https://github.com/CLxS/SMAPI/compare/0.38.4...0.38.5))
* 0.38.4 (2016-03-21, [log](https://github.com/CLxS/SMAPI/compare/0.38.3...0.38.4))
* 0.38.3 (2016-03-21, [log](https://github.com/CLxS/SMAPI/compare/0.38.2...0.38.3))
* 0.38.2 (2016-03-21, [log](https://github.com/CLxS/SMAPI/compare/0.38.0...0.38.2))
* 0.38.0 (2016-03-20, [log](https://github.com/CLxS/SMAPI/compare/0.38.1...0.38.0))
* 0.38.1 (2016-03-20, [log](https://github.com/CLxS/SMAPI/compare/0.37.3...0.38.1))
* 0.37.3 (2016-03-08, [log](https://github.com/CLxS/SMAPI/compare/0.37.2...0.37.3))
* 0.37.2 (2016-03-07, [log](https://github.com/CLxS/SMAPI/compare/0.37.1...0.37.2))
* 0.37.1 (2016-03-06, [log](https://github.com/CLxS/SMAPI/compare/0.36...0.37.1))
* 0.36 (2016-03-04, [log](https://github.com/CLxS/SMAPI/compare/0.37...0.36))
* 0.37 (2016-03-04, [log](https://github.com/CLxS/SMAPI/compare/0.35...0.37))
* 0.35 (2016-03-02, [log](https://github.com/CLxS/SMAPI/compare/0.34...0.35))
* 0.34 (2016-03-02, [log](https://github.com/CLxS/SMAPI/compare/0.33...0.34))
* 0.33 (2016-03-02, [log](https://github.com/CLxS/SMAPI/compare/0.32...0.33))
* 0.32 (2016-03-02, [log](https://github.com/CLxS/SMAPI/compare/0.31...0.32))
* 0.31 (2016-03-02, [log](https://github.com/CLxS/SMAPI/compare/0.3...0.31))
* 0.3 (2016-03-01, [log](https://github.com/CLxS/SMAPI/compare/Alpha0.2...0.3))
* 0.2 (2016-02-29, [log](https://github.com/CLxS/SMAPI/compare/Alpha0.1...Alpha0.2)
* 0.1 (2016-02-28)
