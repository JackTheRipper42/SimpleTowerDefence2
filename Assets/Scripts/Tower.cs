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
        public LayerMask EnemyLayerMask;
        public float RotationSpeed = 40f;
        public float FireTime = 0.5f;
        public float ReloadTime = 4f;
        public float Damage = 30f;
        public float Range = 30f;

        private GameManager _gameManager;
        private float _lastShot;
        private Enemy _target;

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

            if (_target == null || IsValidTarget(_target))
            {
                var possibleTargets = _gameManager.Enemies.Where(IsValidTarget);
                _target = GetEnemy(possibleTargets);
            }
            if (_target == null)
            {
                DisableLaserBeam();
                return;
            }

            var lookRotation = Quaternion.LookRotation(_target.CenterPosition - Barrel.transform.position);
            var currentRotation = Barrel.rotation;
            var realRotation = Quaternion.RotateTowards(currentRotation, lookRotation, RotationSpeed * Time.deltaTime);
            Turret.rotation = Quaternion.Euler(0f, realRotation.eulerAngles.y, 0f);
            Barrel.rotation = Quaternion.Euler(realRotation.eulerAngles.x, realRotation.eulerAngles.y, 0f);

            var stillShooting = Time.time <= _lastShot + FireTime;
            var canShootAgain = Time.time >= _lastShot + FireTime + ReloadTime;

            if (stillShooting || canShootAgain)
            {
                var raycastHits = Physics.RaycastAll(
                    new Ray(Barrel.transform.position, realRotation * Vector3.forward),
                    Range,
                    ObstacleLayerMask | EnemyLayerMask);

                Enemy hit;
                Vector3 hitPosition;
                if (CanHit(raycastHits.ToList(), Barrel.transform.position, _target, out hit, out hitPosition))
                {
                    if (canShootAgain)
                    {
                        _lastShot = Time.time;
                    }
                    EnableLaserBeam(hit, hitPosition);
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

        private void EnableLaserBeam(Enemy target, Vector3 targetPosition)
        {
            BeamRenderer.enabled = true;
            BeamRenderer.SetPosition(0, BeamRenderer.transform.position);
            BeamRenderer.SetPosition(1, targetPosition);
            target.Hit(Damage/FireTime*Time.deltaTime);
        }

        private void DisableLaserBeam()
        {
            BeamRenderer.enabled = false;
        }

        private bool CanHit(
            List<RaycastHit> raycastHits, 
            Vector3 position, 
            Enemy target, 
            out Enemy hit,
            out Vector3 hitPosition)
        {
            raycastHits.Sort((a, b) => (a.point - position).magnitude.CompareTo((b.point - position).magnitude));
            hit = null;
            hitPosition = new Vector3();
            foreach (var raycastHit in raycastHits)
            {
                if (1 << raycastHit.transform.gameObject.layer == ObstacleLayerMask.value)
                {
                    return false;
                }
                var enemy = raycastHit.transform.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    if (hit == null)
                    {
                        hit = enemy;
                        hitPosition = raycastHit.point;
                    }
                    if (enemy == target)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsValidTarget(Enemy enemy)
        {
            if (!enemy.Alive)
            {
                return false;
            }

            if ((enemy.CenterPosition - Barrel.transform.position).magnitude > Range)
            {
                return false;
            }

            return !Physics.Linecast(Barrel.transform.position, enemy.CenterPosition, ObstacleLayerMask);
        }

        private float CalculateAngle(Vector3 targetPosition)
        {
            var relative = targetPosition - Barrel.transform.position;
            var barrelVector = Barrel.transform.rotation*Vector3.forward;

            return Mathf.Acos(Vector3.Dot(relative, barrelVector)/(relative.magnitude*barrelVector.magnitude));
        }

        private Enemy GetEnemy(IEnumerable<Enemy> enemies)
        {
            Enemy closestAngleEnemy = null;
            float minAngle = float.MaxValue;

            foreach (var enemy in enemies)
            {
                var angle = Mathf.Abs(CalculateAngle(enemy.CenterPosition));
                if (angle < minAngle)
                {
                    closestAngleEnemy = enemy;
                    minAngle = angle;
                }
            }

            return closestAngleEnemy;
        }
    }
}
