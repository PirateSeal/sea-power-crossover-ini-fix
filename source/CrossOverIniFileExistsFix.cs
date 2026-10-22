using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using HarmonyLib;

[BepInPlugin("local.seapower.crossover-ini-fileexists", "CrossOver INI File Exists Fix", "1.0.0")]
public sealed class CrossOverIniFileExistsFix : BaseUnityPlugin
{
    private Harmony harmony;
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFileAttributesW(string path);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern uint GetLastError();

    private void Awake()
    {
        string log = Path.Combine(Paths.BepInExRootPath, "CrossOverIniFileExistsFix.log");
        string missing = Path.Combine(Paths.GameRootPath, "Sea Power_Data", "StreamingAssets", "user", "__missing_probe_" + Guid.NewGuid().ToString("N") + ".ini");
        bool managedMissing = File.Exists(missing);
        uint nativeMissing = GetFileAttributesW(missing);
        uint missingError = GetLastError();
        string existing = Path.Combine(Paths.GameRootPath, "Sea Power_Data", "StreamingAssets", "original", "vessels", "usn_cvn_nimitz.ini");
        uint nativeExisting = GetFileAttributesW(existing);
        bool checksValid = nativeMissing == uint.MaxValue && nativeExisting != uint.MaxValue && (nativeExisting & 0x10) == 0;
        File.WriteAllText(log, "Missing probe: managed=" + managedMissing + " native_attributes=0x" + nativeMissing.ToString("X8") + " native_error=" + missingError + "\n" +
                              "Existing base Nimitz: native_attributes=0x" + nativeExisting.ToString("X8") + "\n");
        if (!checksValid)
        {
            File.AppendAllText(log, "Compatibility patch not activated: native checks failed.\n");
            return;
        }
        harmony = new Harmony("local.seapower.crossover-ini-fileexists");
        harmony.Patch(AccessTools.Method(typeof(File), "Exists", new Type[]{typeof(string)}),
                      prefix: new HarmonyMethod(typeof(CrossOverIniFileExistsFix).GetMethod("CheckIni", BindingFlags.Static | BindingFlags.Public)));
        bool missingAfter = File.Exists(missing);
        bool existingAfter = File.Exists(existing);
        File.AppendAllText(log, "After patch: missing=" + missingAfter + " existing=" + existingAfter + "\n");
        if (missingAfter || !existingAfter)
        {
            harmony.UnpatchSelf();
            File.AppendAllText(log, "Compatibility patch removed: self-check failed.\n");
            return;
        }
        File.AppendAllText(log, "Compatibility patch active for .ini paths only.\n");
        Logger.LogInfo("INI existence checks now use native Windows file attributes; missing/existing self-checks passed.");
    }

    public static bool CheckIni(string path, ref bool __result)
    {
        if (path == null || !path.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)) return true;
        try
        {
            uint attributes = GetFileAttributesW(Path.GetFullPath(path));
            __result = attributes != uint.MaxValue && (attributes & 0x10) == 0;
        }
        catch (ArgumentException) { __result = false; }
        catch (IOException) { __result = false; }
        catch (NotSupportedException) { __result = false; }
        catch (System.Security.SecurityException) { __result = false; }
        catch (UnauthorizedAccessException) { __result = false; }
        return false;
    }

    private void OnDestroy() { if (harmony != null) harmony.UnpatchSelf(); }
}
