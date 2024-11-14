using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LCMoniterEnemies.Patches
{
    [HarmonyPatch(typeof(ManualCameraRenderer))]
    public class ManualCameraRendererPatch
    {
        public static float UpdateTimer = 0f;
        public static bool HasClearedEnemies = false;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void Update2(ManualCameraRenderer __instance)
        {
            if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
            {
                if (!HasClearedEnemies)
                {
                    HandleClearedEnemies(__instance);
                    HasClearedEnemies = true;
                }
                return;
            }
            HasClearedEnemies = false;
            UpdateTimer += Time.deltaTime;
            if (UpdateTimer > LCMoniterEnemies.UpdateInterval.Value)
            {
                UpdateTimer = 0f;
                UpdateEnemyRadarTargets(__instance);
            }
            if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
            {
                HandleClearedEnemies(__instance);
            }
        }

        public static void HandleClearedEnemies(ManualCameraRenderer __instance)
        {
            LCMoniterEnemies.Logger.LogInfo("Clearing enemy radar targets");
            EnemyAI[] currentEnemies = Object.FindObjectsOfType<EnemyAI>();
            __instance.radarTargets.RemoveAll(target => target.isNonPlayer && currentEnemies.ToList().Contains(target.transform.GetComponent<EnemyAI>()));
            LCMoniterEnemies.Logger.LogInfo($"Current enemies: {currentEnemies.Length}, Radar targets: {__instance.radarTargets.Count}");
            if (__instance.targetTransformIndex >= __instance.radarTargets.Count)
            {
                LCMoniterEnemies.Logger.LogWarning($"CameraViewIndex out of bounds when clearing, setting to {__instance.radarTargets.Count - 1}");
                __instance.SwitchRadarTargetAndSync(__instance.radarTargets.Count - 1);
            }
        }

        private static void UpdateEnemyRadarTargets(ManualCameraRenderer __instance)
        {
            if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                return;

            // Get all current enemies
            EnemyAI[] currentEnemies = Object.FindObjectsOfType<EnemyAI>();

            // Remove despawned enemies
            __instance.radarTargets.RemoveAll(target => target.isNonPlayer && target.transform == null);

            if (__instance.targetTransformIndex >= __instance.radarTargets.Count)
            {
                LCMoniterEnemies.Logger.LogWarning($"CameraViewIndex out of bounds, setting to {__instance.radarTargets.Count - 1}");
                __instance.SwitchRadarTargetAndSync(__instance.radarTargets.Count - 1);
            }
            // Add new enemies
            foreach (EnemyAI enemy in currentEnemies)
            {
                if (!__instance.radarTargets.Any(target => target.transform == enemy.transform))
                {
                    AddEnemyToRadarTargets(__instance, enemy.transform, enemy.enemyType.enemyName);
                }
            }
        }

        private static string AddEnemyToRadarTargets(ManualCameraRenderer __instance, Transform enemyTransform, string enemyName)
        {
            if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                return "???";

            // Count existing instances of this enemy type
            int count = __instance.radarTargets.Count(target => target.name.StartsWith(enemyName));

            // Create the target name with the count
            string targetName = "???";
            if (count == 0)
            {
                targetName = enemyName;
            }
            else
            {
                targetName = $"{enemyName} #{count}";
            }

            __instance.radarTargets.Add(new TransformAndName(enemyTransform, targetName, true));
            return targetName;
        }

        [HarmonyPatch("AddTransformAsTargetToRadar")]
        [HarmonyPostfix]
        public static void AddTransformAsTargetToRadarPostfix(ManualCameraRenderer __instance, Transform newTargetTransform, string targetName, bool isNonPlayer, ref string __result)
        {
            if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase)
                return;

            if (newTargetTransform.GetComponent<EnemyAI>() != null)
            {
                __result = AddEnemyToRadarTargets(__instance, newTargetTransform, targetName);
            }
        }
    }
}