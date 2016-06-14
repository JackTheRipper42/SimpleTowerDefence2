using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Enemy : MonoBehaviour
    {
        public float Speed = 5f;
        public float MaxHealth = 100f;
        public LayerMask TerrainLayerMask;
        
        private State _state;
        private Vector3 _startPosition;
        private float _lerpPosition;
        private float _lerpLength;
        private Vector3[] _path;
        private int _waypointIndex;
        private float _health;
        private GameManager _gameManager;
        private Rigidbody _rigidbody;
        private Renderer[] _renderers;

        public Vector3 CenterPosition { get; private set; }

        public bool Alive
        {
            get { return _state != State.Dead; }
        }

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
            _rigidbody = GetComponent<Rigidbody>();
            _renderers = GetComponentsInChildren<Renderer>();
            _waypointIndex = 0;
            Move(path[0], path[1] - path[0], Movement.Teleport);
            MoveToNextWaypoint();
        }

        public void Hit(float damage)
        {
            if (_state == State.Dead)
            {
                return;
            }

            _health -= damage;
            if (_health <= 0)
            {
                _state = State.Dead;
                _gameManager.Killed(this);
            }
        }

        protected virtual void Start()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _rigidbody = GetComponent<Rigidbody>();
            _renderers = GetComponentsInChildren<Renderer>();
            _health = MaxHealth;
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
                        Move(newPosition, _path[_waypointIndex] - _startPosition, Movement.Move);
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
                _lerpLength = (_path[_waypointIndex] - _rigidbody.position).magnitude;
                _startPosition = _rigidbody.position;
            }
            else
            {
                _state = State.DestinationReached;
            }
        }

        private void Move(Vector3 position, Vector3 waypointDirection, Movement movement)
        {
            var raycastStart = position + Vector3.up*100f;
            var ray = new Ray(raycastStart, Vector3.down);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, TerrainLayerMask.value))
            {
                var t = -Vector3.Dot(waypointDirection, hit.normal)/hit.normal.sqrMagnitude;

                var lookDirection = waypointDirection + t*hit.normal;

                switch (movement)
                {
                    case Movement.Teleport:
                        _rigidbody.position = hit.point;
                        break;
                    case Movement.Move:
                        _rigidbody.MovePosition(hit.point);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                _rigidbody.rotation = Quaternion.LookRotation(lookDirection);
            }
            else
            {
                switch (movement)
                {
                    case Movement.Teleport:
                        _rigidbody.position = position;
                        break;
                    case Movement.Move:
                        _rigidbody.MovePosition(position);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            CenterPosition = CalculateCenterPosition();
        }

        private Vector3 CalculateCenterPosition()
        {
            var centerSum = new Vector3();
            var sizeSum = 0f;

            foreach (var subRenderer in _renderers)
            {
                centerSum += subRenderer.bounds.center * subRenderer.bounds.size.magnitude;
                sizeSum += subRenderer.bounds.size.magnitude;
            }

            return centerSum / sizeSum;
        }
        
        private enum State
        {
            Idle,
            Walking,
            DestinationReached,
            Dead
        }

        private enum Movement
        {
            Teleport,
            Move
        }
    }
}
