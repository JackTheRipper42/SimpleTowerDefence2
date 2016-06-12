using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Tower : MonoBehaviour
    {
        public Transform Turret;
        public Transform Barrel;
        public Enemy Enemy;

        protected virtual void Update()
        {
            if (Enemy == null)
            {
                return;
            }

            var enemyRenderer = Enemy.GetComponentsInChildren<Renderer>();
            var targetPosition = CalculateCenterPosition(enemyRenderer);

            Debug.DrawLine(Barrel.transform.position, targetPosition, Color.magenta);

            var lookRotation = Quaternion.LookRotation(targetPosition - Barrel.transform.position).eulerAngles;
            Turret.rotation = Quaternion.Euler(0f, lookRotation.y, 0f);
            Barrel.rotation = Quaternion.Euler(lookRotation.x, lookRotation.y, 0f);
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
