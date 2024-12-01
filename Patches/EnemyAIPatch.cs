using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace LCMoniterEnemies.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StarPostFix(EnemyAI __instance)
        {
            if (LCMoniterEnemies.GetParsedAttackBlacklist().Contains(__instance.enemyType.enemyName))
            {
                return;
            }
            if (__instance.GetComponentInChildren<EnemyPos>() == null)
            {
                int count = GameObject.FindObjectOfType<ManualCameraRenderer>().radarTargets.Count(target => target.name.StartsWith(__instance.enemyType.enemyName));
                string namef = "???";
                if (count > 0)
                {
                    namef = $"{__instance.enemyType.enemyName} #{count}";
                }
                else
                {
                    namef = $"{__instance.enemyType.enemyName}";
                }
                GameObject posOffset = new GameObject(namef);
                Transform enemyTransform = __instance.transform;
                Vector3 OffsetVector = new Vector3(0, LCMoniterEnemies.TargetYoffset.Value, 0);
                posOffset.transform.position = enemyTransform.position + OffsetVector;
                posOffset.transform.rotation = enemyTransform.rotation;
                posOffset.transform.SetParent(enemyTransform, true);
                EnemyPos pos = posOffset.AddComponent<EnemyPos>();
                pos.Root = __instance;
                TransformAndName tnt = new TransformAndName(pos.transform, namef, true);
                pos.TnN = tnt;
                ManualCameraRendererPatch.AddEnemyToRadarTargets(pos);
            }
        }
    }
}