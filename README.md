# Kebab Chefs! - Menu Limit Remover

A simple [MelonLoader](https://melonwiki.xyz/) mod for **Kebab Chefs! - Restaurant Simulator** that removes the default 10-item cap on your restaurant menu, so you (and your friends, in multiplayer) can put as many dishes on the menu as you want. It also lets you raise (or remove) the default 5-item hand-carry limit, and comes with an in-game settings panel to tweak both on the fly.

> ⚠️ Always back up your save files before installing any mod.

## Features

- Raises the maximum number of items allowed on the restaurant menu (default cap is 10 in vanilla).
- Raises the maximum number of items you can hold in your hands (default cap is 5 in vanilla) — set it to unlimited or any custom value.
- In-game settings panel (press **F7**) to toggle unlimited/custom values for both limits and apply changes without editing config files by hand.
- Works in both singleplayer and multiplayer (Netcode-synced).

## Requirements

- **[MelonLoader](https://melonloader.com/)** — this mod does **not** work standalone, MelonLoader must be installed first.

## Installation

1. **If you have a pirated version of the game** (online repack): copy `winmm.dll` from the repack you downloaded and **back it up somewhere safe before installing MelonLoader** — the MelonLoader installer overwrites or removes the existing `winmm.dll` in the game folder. After MelonLoader is installed, put your backed-up `winmm.dll` back into the game's root directory (the same folder as the `.exe`).
2. **Install MelonLoader** on your game, if you haven't already:
   - Download the MelonLoader installer from [melonloader.com](https://melonloader.com/).
   - Point it at `Kebab Chefs! - Restaurant Simulator.exe`.
   - Launch the game once so MelonLoader can finish first-time setup (this creates the `Mods`, `Plugins`, and `UserData` folders in the game's install directory).
3. **Download the mod**: grab the latest `.dll` from the [Releases](https://github.com/PhantomUnk/KebabChefs-UnlimitedMenu/releases) page of this repository.
4. **Drop the `.dll`** into the `Mods` folder inside your game's install directory (the same folder as the game's `.exe`).
5. Launch the game normally. You should see a line in the MelonLoader console confirming the mod loaded.

## Usage

- Press **F7** in-game to open the settings panel.
- For each limit (menu / hand items) you can toggle **Unlimited**, or turn it off and type a custom number.
- Click **Apply** to save your changes.
- **The menu item limit** applies immediately.
- **The hand item limit** only applies after you return to the main menu and re-enter the save/session (this is a limitation of how that value is read, not a bug — the panel will remind you).

## Multiplayer note

- **Menu limit**: only needs to be installed on the **host** — then everything works for everyone. If it's installed on both host and client, it also works.
- **Hand limit**: for this to work correctly, **every player in the session should have the mod installed** (this hasn't been extensively tested host-only, so install it on all clients to be safe).

## Troubleshooting

- **If you have a pirated version of the game** (online repack) — MelonLoader removes or overwrites `winmm.dll` during installation, which breaks the repack's online fix. Before installing MelonLoader, copy `winmm.dll` from your repack to a safe backup location. After MelonLoader is set up, copy that backed-up `winmm.dll` back into the game's root folder (next to the `.exe`).
- **Mod doesn't show up in the log at all** — make sure the `.dll` is directly inside the `Mods` folder (not in a subfolder), and that you installed MelonLoader correctly first.
- **F7 doesn't open anything** — make sure the mod loaded correctly (check the log), and that no other tool/mod is already bound to F7.
- **Hand limit doesn't seem to apply** — remember it only takes effect after returning to the main menu and re-entering; also make sure all players in the session have the mod installed.
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

## Tags

Kebab Chefs mod, Kebab Chefs! - Restaurant Simulator mod, MelonLoader mod, unlimited menu mod, remove menu limit, menu limit remover, restaurant simulator mod, more dishes mod, no 10 item cap, unlimited hand items mod, hand item limit remover, in-game F7 settings menu, Kebab Chefs multiplayer mod, winmm.dll MelonLoader, мод Kebab Chefs, мод Kebab Chefs! - Restaurant Simulator, мод MelonLoader, убрать лимит блюд, безлимитное меню, мод на меню ресторана, симулятор ресторана мод, больше блюд в меню, снять ограничение 10 блюд, лимит предметов в руках, безлимит на руки, мод Kebab Chefs мультиплеер, winmm.dll пиратка
