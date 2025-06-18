using HarmonyLib;
using UnityEngine;

namespace LCMoniterEnemies.Patches
{
    [HarmonyPatch(typeof(ManualCameraRenderer))]
    internal class ManualCameraRendererPatch
    {
        public static void RemoveEnemyFromRadarTargets(EnemyPos enemyTransform)
        {
            if (enemyTransform.TnN == null)
            {
                LCMoniterEnemies.Logger.LogWarning("EnemyPos's tranform and name was null when removing");
                return;
            }

            ManualCameraRenderer[] __instances = Object.FindObjectsOfType<ManualCameraRenderer>();
            foreach (ManualCameraRenderer __instance in __instances)
            {
                if (!(__instance.targetTransformIndex + 1 >= __instance.radarTargets.Count) &&
                    __instance.radarTargets.Contains(enemyTransform.TnN) && LCMoniterEnemies.AutoSwitchOnEnemyDeath.Value && __instance.targetTransformIndex == __instance.radarTargets.IndexOf(enemyTransform.TnN) && __instance.IsServer)
                {
                    __instance.SwitchRadarTargetAndSync(__instance.targetTransformIndex + 1);
                }
                if (__instance.targetTransformIndex + 1 >= __instance.radarTargets.Count && __instance.IsServer)
                {
                    LCMoniterEnemies.Logger.LogWarning($"{__instance.name} Predicted CameraViewIndex will be out of bounds when clearing, setting to {__instance.radarTargets.Count - 1}");
                    __instance.SwitchRadarTargetAndSync(__instance.radarTargets.Count - 1);
                }
                if (__instance.radarTargets.Contains(enemyTransform.TnN))
                {
                    __instance.radarTargets.Remove(enemyTransform.TnN);
                }
            }
        }

        public static void AddEnemyToRadarTargets(EnemyPos enemyTransform)
        {
            if (enemyTransform.TnN == null)
            {
                LCMoniterEnemies.Logger.LogWarning("EnemyPos's tranform and name was null when adding");
                return;
            }

            ManualCameraRenderer[] __instances = Object.FindObjectsOfType<ManualCameraRenderer>();
            foreach (ManualCameraRenderer __instance in __instances)
            {
                if (enemyTransform.TnN == null)
                {
                    LCMoniterEnemies.Logger.LogWarning("EnemyPos's tranform and name was null when adding. Generateing new one...");
                    TransformAndName tnt = new TransformAndName(enemyTransform.transform, enemyTransform.name, true);
                    enemyTransform.TnN = tnt;
                }
                if (!__instance.radarTargets.Contains(enemyTransform.TnN))
                    __instance.radarTargets.Add(enemyTransform.TnN);
            }
        }
    }
}