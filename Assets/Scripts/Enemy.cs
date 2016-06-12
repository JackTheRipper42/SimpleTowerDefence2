using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Enemy : MonoBehaviour
    {
        public float Speed = 5f;
        public LayerMask TerrainLayerMask;

        private State _state;
        private Vector3 _startPosition;
        private float _lerpPosition;
        private float _lerpLength;
        private Vector3[] _path;
        private int _waypointIndex;

        public void Initialize([NotNull] Vector3[] path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException("The path array is empty.", "path");
            }

            _state = State.Idle;
            _path = path;
            _waypointIndex = 0;
            SetPosition(path[0], path[1] - path[0]);
            MoveToNextWaypoint();
        }

        protected virtual void Update()
        {
            if (Time.deltaTime > 0f)
            {

                switch (_state)
                {
                    case State.Idle:
                        break;
                    case State.Walking:
                        _lerpPosition += (Speed*Time.deltaTime)/_lerpLength;
                        var newPosition = Vector3.Lerp(_startPosition, _path[_waypointIndex], _lerpPosition);
                        SetPosition(newPosition, _path[_waypointIndex] - _startPosition);
                        if (_lerpPosition >= 1)
                        {
                            MoveToNextWaypoint();
                        }
                        break;
                    case State.DestinationReached:
                        break;
                    case State.Dead:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void MoveToNextWaypoint()
        {
            _waypointIndex++;
            if (_waypointIndex < _path.Length)
            {
                _state = State.Walking;
                _lerpPosition = 0;
                _lerpLength = (_path[_waypointIndex] - transform.position).magnitude;
                _startPosition = transform.position;
            }
            else
            {
                _state = State.DestinationReached;
            }
        }

        private void SetPosition(Vector3 position, Vector3 waypointDirection)
        {
            var raycastStart = position + Vector3.up*100f;
            var ray = new Ray(raycastStart, Vector3.down);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, TerrainLayerMask.value))
            {
                Debug.DrawLine(hit.point, hit.point + hit.normal*10f, Color.red);
                Debug.DrawLine(transform.position, hit.point, Color.blue);

                var t = -Vector3.Dot(waypointDirection, hit.normal)/hit.normal.sqrMagnitude;
                
                var lookDirection = waypointDirection + t*hit.normal;

                transform.rotation = Quaternion.LookRotation(lookDirection);
                transform.position = hit.point;
            }
            else
            {
                transform.position = position;
            }
        }

        private enum State
        {
            Idle,
            Walking,
            DestinationReached,
            Dead
        }
    }
}
