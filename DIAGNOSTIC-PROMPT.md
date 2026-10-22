# Prompt for investigating the same symptom

Copy this prompt into an assistant with local-file access. Replace the bracketed details.

> I'm running Sea Power [game version] in CrossOver [version] on macOS [version]. I'm using More Flight Deck, Workshop item 3818105882, and its add-on pack, item 3818199045. The game gets stuck at Loading Unit Metadata with a NullReferenceException.
>
> Diagnose my actual installation before changing anything. Read Player.log, BepInEx/LogOutput.log, usersettings.ini, and the installed Workshop metadata. Check the real load order and which DLLs actually loaded: unchecked code mods can still load through Anchor Chain. Verify the mod's current dependency and ordering instructions.
>
> A previously investigated case on Sea Power 0.8.7 / Unity 6000.0.84f1 / CrossOver 26.1 failed in SimpleObjectParameters.InitializeSimpleParameters at IL offset 0x004c4. The game enumerated GetSectionKeyValues("AirGroup") without a null check. Runtime diagnostics identified usn_cvn_nimitz_mfd, an INI resolving to StreamingAssets/user/vessels/usn_cvn_nimitz_mfd.ini even though that file did not exist. File.Exists returned true, but reading the path threw DirectoryNotFoundException. An INI-only BepInEx hook using native GetFileAttributesW allowed that installation to reach the menu.
>
> Treat this as a lead, not a diagnosis of my machine. Check that my exception, affected record, and filesystem behavior actually match. Distinguish real files from cached INIs. If runtime instrumentation is necessary, explain it, keep it targeted, preserve exceptions, and remove it after collecting evidence. Do not suppress the exception or insert an empty AirGroup simply to force loading through.
>
> If a matching workaround is appropriate, back up any files you change, use a reversible installation, and verify both missing-file and existing-file checks. Fully restart and confirm the game reaches the main menu with the intended content enabled; then distinguish loading validation from actual carrier-operation testing. Restore unsuccessful changes. If you cannot verify a fix, produce a bug report separating observations, hypotheses, tests, and remaining uncertainty. Do not send or publish it without my instruction.

## Log locations

Within a typical CrossOver Steam bottle:

- Game: `drive_c/Program Files (x86)/Steam/steamapps/common/Sea Power`
- BepInEx log: `<game>/BepInEx/LogOutput.log`
- Player log: `drive_c/users/crossover/AppData/LocalLow/Triassic Games/Sea Power/Player.log`
- Settings: the same folder as `Player.log`, file `usersettings.ini`
- Workshop content: `drive_c/Program Files (x86)/Steam/steamapps/workshop/content/1286220`

The macOS bottle is usually under `~/Library/Application Support/CrossOver/Bottles/<bottle name>/`. A custom bottle or Steam library may use different paths.
