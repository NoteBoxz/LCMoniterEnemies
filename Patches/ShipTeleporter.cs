using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace LCMoniterEnemies.Patches
{
    [HarmonyPatch(typeof(ShipTeleporter))]
    internal class ShipTeleporterPatch
    {
        [HarmonyPatch(nameof(ShipTeleporter.beamUpPlayer))]
        [HarmonyPrefix]
        public static void beamUpPlayerPostfix(ShipTeleporter __instance)
        {
            if (!LCMoniterEnemies.TryTelewarp.Value)
            {
                LCMoniterEnemies.Logger.LogInfo("Telewarp is disabled, skipping beam up.");
                return;
            }
            __instance.StartCoroutine(beamUpEnemy(__instance));
        }

        static IEnumerator beamUpEnemy(ShipTeleporter __instance)
        {
            EnemyAI? enemyToBeamUp = ManualCameraRendererPatch.EnemyTargeting;
            if (enemyToBeamUp == null)
            {
                LCMoniterEnemies.Logger.LogInfo("Targeted enemy is null");
                yield break;
            }
            LCMoniterEnemies.Logger.LogInfo($"Attemping to teleport enemy '{enemyToBeamUp.gameObject.name}'");
            if (StartOfRound.Instance.shipIsLeaving)
            {
                LCMoniterEnemies.Logger.LogInfo($"Ship could not teleport enemy '{enemyToBeamUp.gameObject.name}' because the ship is leaving the nav mesh.");
                yield break;
            }

            // does nothing for almost all enemies, execpt for the masked.
            enemyToBeamUp.ShipTeleportEnemy();
            enemyToBeamUp.creatureSFX.PlayOneShot(__instance.beamUpPlayerBodySFX);

            // FX delay
            yield return new WaitForSeconds(3f);
            if (StartOfRound.Instance.shipIsLeaving)
            {
                LCMoniterEnemies.Logger.LogInfo($"Ship could not teleport enemy '{enemyToBeamUp.gameObject.name}' because the ship is leaving the nav mesh.");
                yield break;
            }

            // Actually beam up the enemy, because almost all enemies don't override the ship teleport function.
            enemyToBeamUp.SetEnemyOutside(outside: true);
            if (enemyToBeamUp.IsOwner)
            {
                enemyToBeamUp.agent.enabled = false;
                enemyToBeamUp.transform.position = __instance.teleporterPosition.position;
                enemyToBeamUp.agent.enabled = true;
            }
            enemyToBeamUp.serverPosition = __instance.teleporterPosition.position;

            // Teleporter Visuals
            __instance.shipTeleporterAudio.PlayOneShot(__instance.teleporterBeamUpSFX);
            if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
        }
    }
}