# Loot All Pre-Disaster

Before the disaster, one click searches every container and uses every reward interaction in the store you are in. It is meant for the days before the disaster, not for expeditions afterwards.

## Features

- **One entry in the Mods menu**: open the inventory, pick **Loot this store** from the Mods dropdown. The entry only shows before the disaster.
- **Every container, every reward**: each container that has not been searched yet is searched, and each interaction that gives a daily reward (and still has some left today) is used. Shop shelves are never touched.
- **Three ways to loot** (`Mode`):
  1. Walk to each container and interaction. Each container's bag window opens as usual and you take what you want. This is the default and the closest to playing by hand.
  2. Walk to each one and take everything at once, with no window. Items go into your trunk first if you own a car or shopping cart, then into your bag.
  3. Take everything at once with no window and no walking.
- **Normal time or instant** (`ActionTime`): `Runs` keeps every search and interaction at its normal length. `Instant` ends them at once, so no game time passes; you still get everything they give. In mode 3 with `Instant`, you do not move and the clock does not move.
- **Stops when you are full**: in modes 2 and 3, a container that only part fits gives you what fits, then the run stops and tells you. Go home, unload, come back and click again: containers already searched are skipped, and the ones you could not empty are emptied first.
- **Click again to carry on**: anything already searched, or any interaction used up for today, is skipped, so a second click only does what is left.
- **Log file**: each click and how the run ended are written to `BepInEx\loot-all-pre-disaster.log`.

Settings are in [CONFIG.md](CONFIG.md). With the Mod Manager they change in game on its Mods screen, no restart needed.

Pairs well with [Full Queue](https://www.nexusmods.com/survivallog/mods/32). Before the disaster the game keeps only one order, so any order you give during a run throws the rest of the run away. With Full Queue your order joins the end of the queue instead, and the queue bar shows every waiting step of the run, not just the first 5.

## Known issues and limits

- Works only in a store before the disaster. At home, or after the disaster, the click tells you why it did nothing.
- Only the floor you are standing on is looted.
- Without Full Queue, giving any order during a run (moving, using something) ends the run there. Click again to carry on.
- Mode 3 skips the walk: an interaction that needs you to stand at it may look odd while it plays.

## Requirements

- Survival Log 1.1.18902. Built and tested on this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- **Exiled's Toolkit** 0.5.0 or newer, which draws the Mods menu and the notices. Loot All Pre-Disaster does not load without it.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Install Exiled's Toolkit: download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from Nexus Mods, and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Copy `LootAllPreDisaster.dll` into the `BepInEx\plugins` folder inside the game folder.

## Uninstall

Delete `LootAllPreDisaster.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.lootallpredisaster.cfg` and `BepInEx\loot-all-pre-disaster.log`.

## Troubleshooting

Check `BepInEx\LogOutput.log` for `===== Loot All Pre-Disaster 0.1.0 started`. The same lines are in `BepInEx\loot-all-pre-disaster.log`, which survives restarts, along with one line per click and one per run end.

If the Mods menu has no **Loot this store** entry, make sure Exiled's Toolkit 0.5.0 or newer is installed and that the disaster has not happened yet.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\loot-all-pre-disaster.log`.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx, the game's IL2CPP interop assemblies and Exiled's Toolkit, so a game install with BepInEx set up and started once is required. Those assemblies are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/LootAllPreDisaster -c Release -p:GameDir="C:\path\to\Survival Log" -p:ToolkitDll="C:\path\to\ExiledToolkit.dll"
```

`GameDir` is the game folder and `ToolkitDll` is a built `ExiledToolkit.dll`. The defaults in the project file are local paths you will almost certainly need to override. The output DLL is at `src\LootAllPreDisaster\bin\Release\LootAllPreDisaster.dll`.

The unit tests cover the pure rules (what a click loots and in what order, when a run stops, how instant actions are stepped) and need nothing from the game:

```
dotnet test tests/LootAllPreDisaster.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
