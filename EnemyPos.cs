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
  
        public void OnDestory()
        {
            ManualCameraRendererPatch.RemoveEnemyFromRadarTargets(this);
        }
    }
}
