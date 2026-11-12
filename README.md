# Loot All Pre-Disaster

Before the disaster, one click searches every container in the store you're in and uses every interaction that gives a daily reward. It only works in stores before the disaster, so it does nothing on expeditions afterwards.

## Features

- **One entry in the Mods menu**: open the inventory and pick **Loot this store** from the Mods dropdown. The entry only shows before the disaster.
- **Every container, every reward**: the run searches every container you haven't searched yet and uses every interaction that still has a daily reward left today. Shop shelves are never touched.
- **Three ways to loot** (`Mode`):
  1. Walk to each container and interaction. Each container's bag window opens as usual and you take what you want. This is the default and the closest to playing by hand.
  2. Walk to each one and take everything at once, with no window. Items go into your trunk first if you own a car or shopping cart, then into your bag.
  3. Take everything at once with no window and no walking. Each action still takes its normal time.
- **Stop any time**: any order you give during a run, such as a click to walk somewhere, stops the run and is carried out right away.
- **Stops when you are full**: in modes 2 and 3, if only part of a container fits, you get what fits, then the run stops and tells you. Go home, unload, come back and click again. Containers you already searched are skipped, and the ones you couldn't empty get emptied first.
- **Click again to carry on**: containers you already searched and interactions that are used up for today are skipped, so a second click only does what is left.
- **Log file**: each click and how each run ended are written to `BepInEx\loot-all-pre-disaster.log`.

Settings are in [CONFIG.md](CONFIG.md). With Mod Manager you can change them in game on its Mods screen, with no restart. Each world can keep its own mode: pick it for a new world in Customizable Difficulty, or change it for that world alone on the Mods screen.

Pairs well with [Full Queue](https://www.nexusmods.com/survivallog/mods/32). Without it, the queue bar only shows the first 5 steps of a run, and with it you see every step. An order you give during a run still stops the run, with or without Full Queue.

## Known issues and limits

- Works only in a store before the disaster. If you click at home or after the disaster, a message tells you why nothing happened.
- Only the floor you are standing on is looted.
- Mode 3 skips the walk, so an interaction that normally needs you to stand at it may look odd while it plays.

## Requirements

- Survival Log 1.1.20719. A game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- **Exiled's Toolkit** 0.19.0 or newer, which draws the Mods menu and the notices. Loot All Pre-Disaster does not load without it.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Install Exiled's Toolkit: download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `LootAllPreDisaster-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-LootAllPreDisaster/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/78), and extract it into the game folder, merging the folders. `LootAllPreDisaster.dll` ends up in `BepInEx\plugins\`.

## Uninstall

Delete `LootAllPreDisaster.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.lootallpredisaster.cfg` and `BepInEx\loot-all-pre-disaster.log`.

## Troubleshooting

Check `BepInEx\LogOutput.log` for `===== Loot All Pre-Disaster 0.2.2 started`. The same lines are in `BepInEx\loot-all-pre-disaster.log`, which survives restarts, along with one line per click and one per run end.

If the Mods menu has no **Loot this store** entry, make sure Exiled's Toolkit 0.19.0 or newer is installed and that the disaster has not happened yet.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\loot-all-pre-disaster.log`.

To reach me directly, message `exiled.dev` on Discord. You can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx, the game's IL2CPP interop assemblies and Exiled's Toolkit, so a game install with BepInEx set up and started once is required. Those assemblies are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/LootAllPreDisaster -c Release -p:GameDir="C:\path\to\Survival Log" -p:ToolkitDll="C:\path\to\ExiledToolkit.dll"
```

`GameDir` is the game folder and `ToolkitDll` is a built `ExiledToolkit.dll`. The defaults in the project file are local paths you will almost certainly need to override. The output DLL is at `src\LootAllPreDisaster\bin\Release\LootAllPreDisaster.dll`.

The unit tests cover the pure rules (what a click loots and in what order, when a run stops, which orders stop it) and need nothing from the game:

```
dotnet test tests/LootAllPreDisaster.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
