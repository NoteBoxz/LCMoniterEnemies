using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UnityEngine;

namespace LCMoniterEnemies
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("Zaggy1024.OpenBodyCams", BepInDependency.DependencyFlags.SoftDependency)]
    public class LCMoniterEnemies : BaseUnityPlugin
    {
        public static LCMoniterEnemies Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        internal static ConfigEntry<string> BlackList { get; set; } = null!;
        internal static ConfigEntry<float> TargetYoffset { get; set; } = null!;
        internal static ConfigEntry<float> TargetXoffset { get; set; } = null!;
        internal static ConfigEntry<float> TargetZoffset { get; set; } = null!;

        internal static ConfigEntry<bool> AutoSwitchOnEnemyDeath { get; set; } = null!;
        internal static ConfigEntry<bool> CreateBodyCam { get; set; } = null!;
        public static List<string> GetParsedAttackBlacklist()
        {
            if (string.IsNullOrEmpty(BlackList.Value))
            {
                return new List<string>();
            }
            return BlackList.Value.Split(',').ToList();
        }

        public static List<EnemyType> EnemyTypes = Resources.FindObjectsOfTypeAll<EnemyType>()
            .Where(e => e.enemyPrefab != null)
            .GroupBy(e => e.enemyPrefab)
            .Select(g => g.First())
            .ToList();


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
            CreateBodyCam = Config.Bind("Settings", "Create BodyCam", true, "Create a BodyCam for each enemy. If false, the enemy will spesfic point for a bodycam.");

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            try
            {
                // Get all types from the executing assembly
                Type[] types = GetTypesWithErrorHandling();

                // Patch everything except FilterEnemyTypesPatch
                foreach (var type in types)
                {
                    try
                    {
                        Harmony.PatchAll(type);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Error patching type {type.FullName}: {e.Message}");
                        if (e.InnerException != null)
                        {
                            Logger.LogError($"Inner exception: {e.InnerException.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error during patching process: {e.Message}");
                if (e.InnerException != null)
                {
                    Logger.LogError($"Inner exception: {e.InnerException.Message}");
                }
            }

            Logger.LogDebug("Finished patching!");
        }

        internal static Type[] GetTypesWithErrorHandling()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Logger.LogWarning("ReflectionTypeLoadException caught while getting types. Some types will be skipped.");
                foreach (var loaderException in e.LoaderExceptions)
                {
                    Logger.LogWarning($"Loader Exception: {loaderException.Message}");
                    if (loaderException is FileNotFoundException fileNotFound)
                    {
                        Logger.LogWarning($"Could not load file: {fileNotFound.FileName}");
                    }
                }
                return e.Types.Where(t => t != null).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError($"Unexpected error while getting types: {e.Message}");
                return new Type[0];
            }
        }

        internal static void Unpatch()
        {
            Harmony?.UnpatchSelf();

            Logger.LogDebug("MoniterEnemies unpatched!  Harmony unloaded.");
        }
    }
}
