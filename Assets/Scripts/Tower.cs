using System.Collections.Generic;
using System.Linq;
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
        public LayerMask ObstacleLayerMask;
        public LayerMask EnemzLayerMask;
        public float RotationSpeed = 40f;
        public float FireTime = 0.5f;
        public float ReloadTime = 4f;

        private GameManager _gameManager;
        private float _lastShot;

        protected virtual void Start()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _lastShot = int.MinValue;
            BeamRenderer.SetVertexCount(2);
            BeamRenderer.SetWidth(LaserWidth, LaserWidth);
            BeamRenderer.SetColors(LaserColor, LaserColor);
        }

        protected virtual void Update()
        {
            if (Time.deltaTime <= 0f)
            {
                return;
            }

            var enemy = _gameManager.Enemies.FirstOrDefault();

            if (enemy == null)
            {
                return;
            }

            var enemyRenderer = enemy.GetComponentsInChildren<Renderer>();
            var targetPosition = CalculateCenterPosition(enemyRenderer);

            if (Physics.Linecast(Barrel.transform.position, targetPosition, ObstacleLayerMask.value))
            {
                DisableLaserBeam();
            }
            else
            {
                var lookRotation = Quaternion.LookRotation(targetPosition - Barrel.transform.position);
                var currentRotation = Barrel.rotation;
                var realRotation = Quaternion.RotateTowards(currentRotation, lookRotation, RotationSpeed*Time.deltaTime);

                Turret.rotation = Quaternion.Euler(0f, realRotation.eulerAngles.y, 0f);
                Barrel.rotation = Quaternion.Euler(realRotation.eulerAngles.x, realRotation.eulerAngles.y, 0f);

                RaycastHit hit;
                if (Physics.Raycast(
                    new Ray(Barrel.transform.position, realRotation*Vector3.forward),
                    out hit,
                    float.PositiveInfinity,
                    ObstacleLayerMask | EnemzLayerMask))
                {
                    var hittedEmeny = hit.transform.GetComponentInParent<Enemy>();
                    if (hittedEmeny != null)
                    {
                        if (Time.time <= _lastShot + FireTime)
                        {
                            RenderLaserBeam(targetPosition);
                        }
                        else if (Time.time >= _lastShot + FireTime + ReloadTime)
                        {
                            _lastShot = Time.time;
                            RenderLaserBeam(targetPosition);
                        }
                        else
                        {
                            DisableLaserBeam();
                        }
                    }
                    else
                    {
                        DisableLaserBeam();
                    }

                }
                else
                {
                    DisableLaserBeam();
                }
            }
        }

        private void RenderLaserBeam(Vector3 targetPosition)
        {
            BeamRenderer.enabled = true;
            BeamRenderer.SetPosition(0, BeamRenderer.transform.position);
            BeamRenderer.SetPosition(1, targetPosition);
        }

        private void DisableLaserBeam()
        {
            BeamRenderer.enabled = false;
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
