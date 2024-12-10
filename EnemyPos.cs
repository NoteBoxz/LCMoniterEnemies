using LCMoniterEnemies.Patches;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LCMoniterEnemies
{
    public class EnemyPos : MonoBehaviour
    {
        public EnemyAI Root = null!;
        public TransformAndName TnN = null!;

        public void OnDestroy()
        {
            ManualCameraRendererPatch.RemoveEnemyFromRadarTargets(this);
        }

        public void LateUpdate()
        {
            // Set the object's position relative to the parent
            transform.localPosition = new Vector3(
                transform.localPosition.x, // Keep the current local X position
                LCMoniterEnemies.TargetYoffset.Value,               // Set Y to the desired offset
                transform.localPosition.z  // Keep the current local Z position
            );
            
            if (Root.isEnemyDead && LCMoniterEnemies.AutoSwitchOnEnemyDeath.Value)
            {
                Destroy(gameObject);
            }
        }
    }
}
