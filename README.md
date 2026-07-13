# Kebab Chefs! - Menu Limit Remover

A simple [MelonLoader](https://melonwiki.xyz/) mod for **Kebab Chefs! - Restaurant Simulator** that removes the default 10-item cap on your restaurant menu, so you (and your friends, in multiplayer) can put as many dishes on the menu as you want. It also lets you raise (or remove) the default 5-item hand-carry limit, extend the customer queue at the restaurant entrance, and remove the cap on your daily "expected customers" forecast — all from one in-game settings panel.

> ⚠️ Always back up your save files before installing any mod.

## Features

- Raises the maximum number of items allowed on the restaurant menu (default cap is 10 in vanilla).
- Raises the maximum number of items you can hold in your hands (default cap is 5 in vanilla) — set it to unlimited or any custom value.
- Extends the customer queue outside the restaurant so it doesn't cap out at a fixed number of spots.
- Removes the cap on the daily "expected customers" forecast, so it keeps growing instead of flattening out.
- In-game settings panel (press **F7**) to toggle unlimited/custom values for all four of the above, and apply changes without editing config files by hand.
- Works in both singleplayer and multiplayer (Netcode-synced).

## Requirements

- **[MelonLoader](https://melonloader.com/)** — this mod does **not** work standalone, MelonLoader must be installed first.

## Installation

1. **If you have a pirated version of the game** (online repack): copy `winmm.dll` from the repack you downloaded and **back it up somewhere safe before installing MelonLoader** — the MelonLoader installer overwrites or removes the existing `winmm.dll` in the game folder. After MelonLoader is installed, put your backed-up `winmm.dll` back into the game's root directory (the same folder as the `.exe`).
2. **Install MelonLoader** on your game, if you haven't already:
   - Download the MelonLoader installer from [melonloader.com](https://melonloader.com/).
   - Point it at `Kebab Chefs! - Restaurant Simulator.exe`.
   - Launch the game once so MelonLoader can finish first-time setup (this creates the `Mods`, `Plugins`, and `UserData` folders in the game's install directory).
     > **Reminder if you have a pirated version:** at this point, put the `winmm.dll` you backed up in step 1 back into the game's root folder (next to the `.exe`), or the repack's online fix won't work.
3. **Download the mod**: grab the latest `.dll` from the [Releases](https://github.com/PhantomUnk/KebabChefs-UnlimitedMenu/releases) page of this repository.
4. **Drop the `.dll`** into the `Mods` folder inside your game's install directory (the same folder as the game's `.exe`).
5. Launch the game normally. You should see a line in the MelonLoader console confirming the mod loaded.

## Usage

- Press **F7** in-game to open the settings panel.
- For each setting (menu items, hand items, entrance queue, daily customer forecast) you can toggle **Unlimited**, or turn it off and type a custom number.
- Click **Apply** to save your changes.
- **All settings** only apply after you return to the main menu and re-enter the save/session (this is a limitation of how those values are read, not a bug — the panel will remind you).
- Your settings are saved and persist the next time you launch the game — no need to reconfigure them every session.

## Multiplayer note

- **Menu limit**: if only the **host** has the mod installed, only the host will be able to add more than the vanilla 10 dishes — but any dishes added past the limit will still show up on both the host's and the clients' screens. For every player to be able to add dishes beyond the limit themselves, everyone needs the mod installed.
- **Hand limit**: same idea — if only the host has it, only the host can carry more than 5 items. For every player in the session to benefit, everyone should have the mod installed.
- **Queue extension**: only the **host** needs the mod installed — the extended queue works for everyone in the session regardless of who else has the mod.
- **Daily customer forecast**: same as menu/hand limits — everyone in the session needs the mod installed to see the uncapped forecast.

💡 **It is highly recommended that all players install the mod** — this ensures everyone in the session can fully utilize all features (menu limits, held item limits, and daily customer forecasts), while the extended queue will work for everyone even if only the host has the mod installed.

## Troubleshooting

- **If you have a pirated version of the game** (online repack) — MelonLoader removes or overwrites `winmm.dll` during installation, which breaks the repack's online fix. Before installing MelonLoader, copy `winmm.dll` from your repack to a safe backup location. After MelonLoader is set up, copy that backed-up `winmm.dll` back into the game's root folder (next to the `.exe`).
- **Can't download MelonLoader** — try enabling a VPN; the download can be blocked or fail in some regions/networks.
- **Mod doesn't show up in the log at all** — make sure the `.dll` is directly inside the `Mods` folder (not in a subfolder), and that you installed MelonLoader correctly first.
- **F7 doesn't open anything** — make sure the mod loaded correctly (check the log), and that no other tool/mod is already bound to F7.
- **A setting doesn't seem to apply** — remember all four settings only take effect after returning to the main menu and re-entering the save/session; also make sure the right players in the session have the mod installed (see the multiplayer note above).
- **Conflicts with other tools that inject a proxy DLL** (some third-party launch patches or DRM-bypass tools also use a `.dll` with a common name to hook into the game, e.g. `winmm.dll`, `version.dll`, `dinput8.dll`). MelonLoader itself uses `version.dll`. If you use another tool relying on the same filename, only one of them will actually load — rename or restore the appropriate file so both tools use different filenames, if the other tool supports that.
- Logs are located at `MelonLoader/Logs/Latest.log` in the game's install folder — check there first if something isn't working.

## Building from source

This is a [Harmony](https://harmony.pardeike.net/) patch built against the game's Il2Cpp interop assemblies (generated locally by MelonLoader on first launch). To build it yourself:

1. Install [MelonLoader.VSWizard](https://github.com/TrevTV/MelonLoader.VSWizard) for Visual Studio.
2. Create a new "MelonLoader Mod" project and point it at your game's `.exe` — this automatically sets up the correct references.
3. Drop in `Core.cs` and the rest of the source files.
4. Build, then copy the output `.dll` to your `Mods` folder.

## License

MIT — see [LICENSE](LICENSE).

## Disclaimer

This mod only patches values already present in the shipped game code at runtime (via Harmony) — it does not redistribute, decompile for republishing, or modify the original game files on disk. It requires a legitimate, working installation of the game to function.

## Tags

Kebab Chefs mod, Kebab Chefs! - Restaurant Simulator mod, MelonLoader mod, unlimited menu mod, remove menu limit, menu limit remover, restaurant simulator mod, more dishes mod, no 10 item cap, unlimited hand items mod, hand item limit remover, in-game F7 settings menu, Kebab Chefs multiplayer mod, winmm.dll MelonLoader, unlimited queue mod, restaurant queue limit remover, expected customers uncap, daily customer cap remover, remove customer limit, мод Kebab Chefs, мод Kebab Chefs! - Restaurant Simulator, мод MelonLoader, убрать лимит блюд, безлимитное меню, мод на меню ресторана, симулятор ресторана мод, больше блюд в меню, снять ограничение 10 блюд, лимит предметов в руках, безлимит на руки, мод Kebab Chefs мультиплеер, winmm.dll пиратка, лимит очереди ресторана, снять лимит клиентов в день, безлимит клиентов
