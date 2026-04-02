using HarmonyLib;
using Portningsbolaget.Utilities;
#if BEPIN
using BepInEx;
#endif

namespace CrossPatcher;

[ContentWarningPlugin("CrossPatcher", "1.0.0", vanillaCompatible: true )]
#if BEPIN
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class CrossPatcher : BaseUnityPlugin {
    private static readonly Harmony Patcher = new(MyPluginInfo.PLUGIN_GUID);
    private void Awake()
    {
        Patcher.PatchAll();
    }
}
#else
public class CrossPatcher {}
#endif

[HarmonyPatch(typeof(Utilities))]
public class UtilitiesPatches
{
    [HarmonyPatch(nameof(Utilities.CreateRandomName), new Type[]
    {
        typeof(string),
        typeof(bool),
        typeof(int),
        typeof(bool)
    })]
    [HarmonyPrefix]
    private static bool CreateRandomName(ref string region, ref bool usingMods, ref int length, ref bool upperCase)
    {
        usingMods = false;
        return true;
    }
}