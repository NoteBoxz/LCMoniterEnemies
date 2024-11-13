using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace LCMoniterEnemies
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LCMoniterEnemies : BaseUnityPlugin
    {
        public static LCMoniterEnemies Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        internal static ConfigEntry<float> UpdateInterval { get; set; } = null!;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            UpdateInterval = Config.Bind("Settings", "Update Interval (seconds)", 1.0f, "Interval in seconds to update the enemy counts.");

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
            Harmony.PatchAll();

            Logger.LogDebug("MoniterEnemies patched!");
        }

        internal static void Unpatch()
        {
            Harmony?.UnpatchSelf();

            Logger.LogDebug("MoniterEnemies unpatched!  Harmony unloaded.");
        }
    }
}
