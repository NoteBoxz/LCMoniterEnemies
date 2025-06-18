using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace LCMoniterEnemies.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(nameof(StartOfRound.Start))]
        [HarmonyPrefix]
        public static void StartPrefix()
        {
            if(!LCMoniterEnemies.CreateBodyCam.Value)
            {
                return;
            }

            foreach (EnemyType enemyType in LCMoniterEnemies.EnemyTypes)
            {
                try
                {
                    if (enemyType == null || enemyType.enemyPrefab == null)
                    {
                        LCMoniterEnemies.Logger.LogWarning("EnemyType or its prefab is null, skipping...");
                        continue;
                    }

                    Transform? point = EnemyPos.GetOrCreateBodyCamPoint(enemyType.enemyPrefab.gameObject);
                    if (point != null)
                    {
                        EnemyPos.BodyCamPoints[enemyType] = point;
                        LCMoniterEnemies.Logger.LogDebug($"BodyCamPoint for {enemyType.name} set to {point.name} at {point.position},{point.rotation} ({point.localPosition})");
                    }
                }
                catch (System.Exception ex)
                {
                    LCMoniterEnemies.Logger.LogError($"Error while processing EnemyType: {ex.Message}");
                    continue;
                }
            }
        }
    }
}