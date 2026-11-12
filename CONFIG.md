# Loot All Pre-Disaster settings

Settings live in `BepInEx\config\com.dev.survivallog.lootallpredisaster.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen with no restart, or edit the file while the game is closed. A new `Mode` applies from the next click. Each world can keep its own `Mode`: pick it for a new world in Customizable Difficulty, or, while you play, change it for that world alone on the Mods screen. Delete the file to get the defaults back.

## Loot

| Setting | Default | What it does |
| --- | --- | --- |
| `Mode` | `1` | What happens when you click **Loot this store**. `1`: you walk to each container and interaction, and each container's bag window opens so you can take what you want. `2`: you walk to each one and take everything with no window. Items go into your trunk first if you have a car or shopping cart, then into your bag. `3`: you take everything with no window and without walking. Each action still takes its normal time. |

In modes 2 and 3, if only part of a container fits, you get what fits and the run stops. Unload at home, come back and click again to carry on.
