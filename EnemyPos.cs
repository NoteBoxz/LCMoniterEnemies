using LCMoniterEnemies.Patches;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace LCMoniterEnemies
{
    public class EnemyPos : MonoBehaviour
    {
        public EnemyAI Root = null!;
        public TransformAndName TnN = null!;
        public Transform? BodyCamPoint = null;
        public static Dictionary<EnemyType, Transform> BodyCamPoints = new Dictionary<EnemyType, Transform>();

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

        void Start()
        {
            if (!LCMoniterEnemies.CreateBodyCam.Value)
            {
                return;
            }

            if (BodyCamPoints.ContainsKey(Root.enemyType))
            {
                BodyCamPoint = BodyCamPoints[Root.enemyType];
            }
            else if (transform.parent != null)
            {
                BodyCamPoint = GetOrCreateBodyCamPoint(transform.parent.gameObject);
            }
        }



        public static Transform? GetOrCreateBodyCamPoint(GameObject obj)
        {
            // Get all child objects
            Transform[] allChildren = obj.GetComponentsInChildren<Transform>(false);
            NavMeshAgent agent = obj.GetComponentInChildren<NavMeshAgent>();

            foreach (Transform child in allChildren)
            {
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                string str = child.gameObject.name.ToLower();
                if (str.Contains("head") || str.Contains("h_j000") || str.Contains("f_j001"))
                {
                    GameObject bodyCamPoint = new GameObject($"BodyCamPoint");
                    bodyCamPoint.transform.SetParent(child);
                    if (agent != null)
                    {
                        bodyCamPoint.transform.rotation = Quaternion.LookRotation(agent.transform.forward);
                        Vector3 position = agent.transform.forward * agent.radius * 0.1f;
                        position.y = child.transform.localPosition.y;
                        bodyCamPoint.transform.localPosition = position;
                    }
                    LCMoniterEnemies.Logger.LogDebug($"Created BodyCamPoint for {obj.name} at {bodyCamPoint.transform.localPosition}, {bodyCamPoint.transform.localRotation.eulerAngles}");
                    return bodyCamPoint.transform;
                }
            }

            return null;
        }
    }
}
