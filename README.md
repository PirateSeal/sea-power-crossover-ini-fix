# Sea Power / CrossOver INI loading workaround — v1.0.0

This BepInEx plugin works around a file-existence problem observed while loading **More Flight Deck** (Workshop `3818105882`) in Sea Power under CrossOver on macOS.

In the affected game process, `System.IO.File.Exists()` reported a nonexistent INI as present. The loader selected that nonexistent User Data path, built an incomplete carrier record, and threw a null reference while enumerating its missing `[AirGroup]` section. This plugin uses Windows' native `GetFileAttributesW()` for `.ini` existence checks in the game process.

**Observed result:** the original error stopped and the game reached the main menu with More Flight Deck and its add-on pack enabled. Carrier operations inside a mission have not been tested. This is a community workaround, with no affiliation to the game, mod, or CrossOver authors. It is not a general fix for every metadata-loading error.

## Tested environment

- Sea Power `0.8.7-261021`, Steam build `26103404`
- Unity `6000.0.84f1`
- CrossOver `26.1`, Steam bottle, Windows 10 reported by the game
- macOS `27.2`, Apple M4 Pro, 48 GiB unified memory
- BepInEx `5.4.23.2`, Anchor Chain `1.1.0`

No native-Windows control test or other CrossOver/game-version combinations have been tested. The evidence points to a Unity/Mono–CrossOver interaction; its precise underlying interop defect has not been established.

## Install

1. Fully quit Sea Power.
2. In Steam, select Sea Power → Properties → Installed Files → Browse. Alternatively, use CrossOver's **Open C: Drive** and navigate to `Program Files (x86)\Steam\steamapps\common\Sea Power`.
3. Confirm BepInEx is already installed and working. This ZIP does not include BepInEx or Anchor Chain.
4. Copy **`BepInEx/plugins/CrossOverIniFileExistsFix.dll`** from this ZIP into the corresponding `BepInEx\plugins` folder inside the Sea Power installation. Create `plugins` if your existing BepInEx installation lacks it.
5. Start Sea Power normally.

The DLL belongs in the game's BepInEx folder, not a Workshop folder. There is no in-game checkbox for this plugin: BepInEx loads it at process startup.

For More Flight Deck, also follow the mod's own instructions: place its add-on pack and the source mods for custom carriers above More Flight Deck. This workaround does not supply missing ships, models, or dependencies.

## Confirm it activated

Open `BepInEx\CrossOverIniFileExistsFix.log` in the game folder. It should contain:

```text
After patch: missing=False existing=True
Compatibility patch active for .ini paths only.
```

The plugin checks a randomly named, nonexistent INI and the existing vanilla Nimitz INI. It does not create the probe file. If the native checks or the post-patch checks fail, it leaves the hook inactive or removes it and records that result. A correct managed result at startup does not exclude the intermittent false-positive behavior observed later during loading.

Check `Player.log` for the original exception and confirm the game actually reaches the menu. A successful self-check alone does not establish that a particular loading error is fixed.

## Remove

Fully quit Sea Power, remove `BepInEx\plugins\CrossOverIniFileExistsFix.dll`, and restart. The optional log can also be removed. No INI restoration is required: this plugin does not edit game or Workshop INIs.

## Scope and limits

- The hook affects **all `.ini` paths checked through `System.IO.File.Exists()` inside the Sea Power process**, including other mods' INIs. Other file extensions use the original method.
- It changes file-existence checks; it does not catch or suppress metadata exceptions and does not fabricate empty air groups.
- It runs only in this Windows game process; it changes no macOS or CrossOver-wide setting.
- On a later game, Unity, BepInEx, or CrossOver update, retest whether it is needed and remains compatible.
- File-system permissions, unusual path forms, and other compatibility-layer behaviors have not been exhaustively tested.

## Source and rebuilding

Source code and a Python build helper are in `source/`. Rebuilding requires Python 3, a .NET SDK, and your own existing game/BepInEx assemblies. No game assemblies or third-party libraries are bundled.

```sh
python3 source/build.py --game "/path/to/Sea Power" --output "CrossOverIniFileExistsFix.dll"
```

On Windows, use `python` and the Windows game path. The helper compiles against the locally installed game and loader; it does not install the result.

## Reporting the issue

See `DIAGNOSTIC-PROMPT.md` for a prompt that gathers evidence before proposing a fix. Include `Player.log`, `BepInEx\LogOutput.log`, this plugin's log, game/CrossOver versions, and the enabled mod order. Logs may contain identifiers or local paths; review them before sharing.

Source code is provided under the MIT license in `LICENSE.txt`.
