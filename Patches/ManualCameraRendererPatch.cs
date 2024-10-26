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
        private static Coroutine updateEnemiesCoroutine;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(ManualCameraRenderer __instance)
        {
            // Start the coroutine to update enemies
            updateEnemiesCoroutine = __instance.StartCoroutine(UpdateEnemiesCoroutine(__instance));
        }

        private static IEnumerator UpdateEnemiesCoroutine(ManualCameraRenderer __instance)
        {
            while (true)
            {
                UpdateEnemyRadarTargets(__instance);
                yield return new WaitForSeconds(1f); // Update every second, adjust as needed
            }
        }

        private static void UpdateEnemyRadarTargets(ManualCameraRenderer __instance)
        {
            // Get all current enemies
            EnemyAI[] currentEnemies = Object.FindObjectsOfType<EnemyAI>();
            HashSet<Transform> currentEnemyTransforms = new HashSet<Transform>(currentEnemies.Select(e => e.transform));

            // Remove despawned enemies
            __instance.radarTargets.RemoveAll(target =>
                target.isNonPlayer &&
                target.transform.GetComponent<EnemyAI>() != null &&
                !currentEnemyTransforms.Contains(target.transform));

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
            string targetName = $"{enemyName}";
            __instance.radarTargets.Add(new TransformAndName(enemyTransform, targetName, true));
            return targetName;
        }

        [HarmonyPatch("AddTransformAsTargetToRadar")]
        [HarmonyPostfix]
        public static void AddTransformAsTargetToRadarPostfix(ManualCameraRenderer __instance, Transform newTargetTransform, string targetName, bool isNonPlayer, ref string __result)
        {
            if (newTargetTransform.GetComponent<EnemyAI>() != null)
            {
                __result = AddEnemyToRadarTargets(__instance, newTargetTransform, targetName);
            }
        }
    }
}