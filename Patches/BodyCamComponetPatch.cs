using HarmonyLib;
using OpenBodyCams;
using System.Linq;
using UnityEngine;

namespace LCMoniterEnemies.Patches
{
    [HarmonyPatch(typeof(BodyCamComponent))]
    internal class BodyCamComponetPatch
    {
        [HarmonyPatch(nameof(BodyCamComponent.SetTargetToTransform))]
        [HarmonyPrefix]
        public static bool SetTargetToTransformPrefix(ref Transform transform)
        {
            if (transform != null && transform.gameObject.TryGetComponent(out EnemyPos enemyPos) && enemyPos.BodyCamPoint != null)
            {
                // Modify the argument before the original method runs
                transform = enemyPos.BodyCamPoint;
                LCMoniterEnemies.Logger.LogDebug($"BodyCamComponent.SetTargetToTransform: Setting target to {enemyPos.BodyCamPoint.name} for enemy {enemyPos.Root.enemyType.name}");
            }
            else
            {
                LCMoniterEnemies.Logger.LogDebug("BodyCamComponent.SetTargetToTransform: No valid EnemyPos or BodyCamPoint found, using original transform.");
            }

            return true;
        }
    }
}