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
                LCMoniterEnemies.TargetXoffset.Value,
                LCMoniterEnemies.TargetYoffset.Value,
                LCMoniterEnemies.TargetZoffset.Value
            );
        }
        public void Update()
        {
            if (Root.isEnemyDead && LCMoniterEnemies.AutoSwitchOnEnemyDeath.Value)
            {
                Destroy(gameObject);
            }
        }
    }
}
