using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Tower : MonoBehaviour
    {
        public Transform Turret;
        public Transform Barrel;
        public LineRenderer BeamRenderer;
        public float LaserWidth = 0.2f;
        public Color LaserColor = Color.red;
        public Enemy Enemy;

        protected virtual void Start()
        {
            BeamRenderer.SetVertexCount(2);
            BeamRenderer.SetWidth(LaserWidth, LaserWidth);
            BeamRenderer.SetColors(LaserColor, LaserColor);
        }

        protected virtual void Update()
        {
            if (Enemy == null)
            {
                return;
            }

            var enemyRenderer = Enemy.GetComponentsInChildren<Renderer>();
            var targetPosition = CalculateCenterPosition(enemyRenderer);

            var lookRotation = Quaternion.LookRotation(targetPosition - Barrel.transform.position).eulerAngles;
            Turret.rotation = Quaternion.Euler(0f, lookRotation.y, 0f);
            Barrel.rotation = Quaternion.Euler(lookRotation.x, lookRotation.y, 0f);

            BeamRenderer.SetPosition(0, BeamRenderer.transform.position);
            BeamRenderer.SetPosition(1, targetPosition);
        }

        private static Vector3 CalculateCenterPosition(IEnumerable<Renderer> targetRenderers)
        {
            var centerSum = new Vector3();
            var sizeSum = 0f;

            foreach (var subRenderer in targetRenderers)
            {
                centerSum += subRenderer.bounds.center*subRenderer.bounds.size.magnitude;
                sizeSum += subRenderer.bounds.size.magnitude;
            }

            return centerSum/sizeSum;
        }
    }
}
