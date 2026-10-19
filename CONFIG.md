# Loot All Pre-Disaster settings

Settings live in `BepInEx\config\com.dev.survivallog.lootallpredisaster.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen with no restart, or edit the file while the game is closed. A new `Mode` applies from the next click. Delete the file to get the defaults back.

## Loot

| Setting | Default | What it does |
| --- | --- | --- |
| `Mode` | `1` | How a click loots the store. `1`: walk to each container and interaction; each container's bag window opens and you take what you want. `2`: walk to each one and take everything at once with no window, into the trunk first if you own a car or shopping cart, then the bag. `3`: take everything at once with no window and no walking. |

In modes 2 and 3, a container that only part fits gives what fits and stops the run. Unload at home, come back and click again to carry on.
