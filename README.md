# Kebab Chefs! - Menu Limit Remover

A simple [MelonLoader](https://melonwiki.xyz/) mod for **Kebab Chefs! - Restaurant Simulator** that removes the default 10-item cap on your restaurant menu, so you (and your friends, in multiplayer) can put as many dishes on the menu as you want.

> ⚠️ Always back up your save files before installing any mod.

## Features

- Raises the maximum number of items allowed on the restaurant menu (default cap is 10 in vanilla).
- Works in both singleplayer and multiplayer (Netcode-synced).

## Requirements

- **[MelonLoader](https://melonloader.com/)** — this mod does **not** work standalone, MelonLoader must be installed first.

## Installation

1. **Install MelonLoader** on your game, if you haven't already:
   - Download the MelonLoader installer from [melonloader.com](https://melonloader.com/).
   - Point it at `Kebab Chefs! - Restaurant Simulator.exe`.
   - Launch the game once so MelonLoader can finish first-time setup (this creates the `Mods`, `Plugins`, and `UserData` folders in the game's install directory).
2. **Download the mod**: grab the latest `.dll` from the [Releases](https://github.com/PhantomUnk/KebabChefs-UnlimitedMenu/releases) page of this repository.
3. **Drop the `.dll`** into the `Mods` folder inside your game's install directory (the same folder as the game's `.exe`).
4. Launch the game normally. You should see a line in the MelonLoader console confirming the mod loaded.

## Multiplayer note

The mod can be installed **only on the host** — then everything will work. If the mod is installed on both the host and the client, it will **also** work.

## Troubleshooting

- **If you have a pirated version of the game** — drop `winmm.dll` from your online repack.
- **Mod doesn't show up in the log at all** — make sure the `.dll` is directly inside the `Mods` folder (not in a subfolder), and that you installed MelonLoader correctly first.
- **Conflicts with other tools that inject a proxy DLL** (some third-party launch patches or DRM-bypass tools also use a `.dll` with a common name to hook into the game, e.g. `winmm.dll`, `version.dll`, `dinput8.dll`). MelonLoader itself uses `version.dll`. If you use another tool relying on the same filename, only one of them will actually load — rename or restore the appropriate file so both tools use different filenames, if the other tool supports that.
- Logs are located at `MelonLoader/Logs/Latest.log` in the game's install folder — check there first if something isn't working.

## Building from source

This is a [Harmony](https://harmony.pardeike.net/) patch built against the game's Il2Cpp interop assemblies (generated locally by MelonLoader on first launch). To build it yourself:

1. Install [MelonLoader.VSWizard](https://github.com/TrevTV/MelonLoader.VSWizard) for Visual Studio.
2. Create a new "MelonLoader Mod" project and point it at your game's `.exe` — this automatically sets up the correct references.
3. Drop in `Core.cs` from this repo.
4. Build, then copy the output `.dll` to your `Mods` folder.

## License

MIT — see [LICENSE](LICENSE).

## Disclaimer

This mod only patches values already present in the shipped game code at runtime (via Harmony) — it does not redistribute, decompile for republishing, or modify the original game files on disk. It requires a legitimate, working installation of the game to function.
