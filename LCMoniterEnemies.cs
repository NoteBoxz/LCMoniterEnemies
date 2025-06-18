using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LCMoniterEnemies
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LCMoniterEnemies : BaseUnityPlugin
    {
        public static LCMoniterEnemies Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        internal static ConfigEntry<string> BlackList{ get; set; } = null!;
        internal static ConfigEntry<float> TargetYoffset { get; set; } = null!;
        internal static ConfigEntry<float> TargetXoffset { get; set; } = null!;
        internal static ConfigEntry<float> TargetZoffset { get; set; } = null!;

        internal static ConfigEntry<bool> AutoSwitchOnEnemyDeath { get; set; } = null!;
        public static List<string> GetParsedAttackBlacklist()
        {
            if (string.IsNullOrEmpty(BlackList.Value))
            {
                return new List<string>();
            }
            return BlackList.Value.Split(',').ToList();
        }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            BlackList = Config.Bind("Settings", "Enemy Blacklist", "", "The list of enemy names that wont be monitored (separated by commas, no spaces in between) (item1,item2,item3...)");
            TargetYoffset = Config.Bind("Settings", "Camera Target Y Offset", 0f, "The Y (Vertical) Offset of the Enemy's target.");
            TargetXoffset = Config.Bind("Settings", "Camera Target X Offset", 0f, "The X (Horizontal) Offset of the Enemy's target.");
            TargetZoffset = Config.Bind("Settings", "Camera Target Z Offset", 0f, "The Z (Depth) Offset of the Enemy's target.");
            AutoSwitchOnEnemyDeath = Config.Bind("Settings", "Auto Switch on Enemy Death", false, "Automatically switch to the next enemy when the current one dies.");

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
