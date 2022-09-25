using ElRaccoone.Tweens.Core;
using UnityEngine;

namespace Weaver.Visuals.Monolith
{
    public class MonolithItem : MonoBehaviour
    {
        [SerializeField]
        private float _heartbeatJumpTime = 0.2f;

        [SerializeField]
        private float _heartbeatJumpScale = 1.5f;

        [SerializeField]
        private EaseType _heartbeatJumpEase = EaseType.ExpoOut;

        [SerializeField]
        private float _heartbeatReturnTime = 0.75f;

        [SerializeField]
        private EaseType _heartbeatReturnEase = EaseType.QuadOut;
        
        public string Path { get; private set; } = string.Empty;

        private bool _runningJump;
        private bool _runningReturn;

        private float _timeSinceStartedJump;
        private float _timeSinceStartedReturn;
        private Vector3 _initialScale;
        private Vector3 _startScale;
        private Vector3 _targetScale;

        private bool _runningMove;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _positionDuration;
        private EaseType _positionEasing;
        private float _timeSinceStartedMoving;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        private void Update()
        {
            if (_runningMove)
            {
                transform.localPosition = Vector3.Lerp(_startPosition, _targetPosition,
                    Easer.Apply(_positionEasing, _timeSinceStartedMoving / _positionDuration));
                _timeSinceStartedMoving += Time.deltaTime;

                if (_positionDuration > _timeSinceStartedMoving)
                    return;

                transform.localPosition = _targetPosition;
                _runningMove = false;
            }
            
            if (_runningJump)
            {
                transform.localScale = Vector3.Lerp(_startScale, _targetScale,
                    Easer.Apply(_heartbeatJumpEase, _timeSinceStartedJump / _heartbeatJumpTime));
                _timeSinceStartedJump += Time.deltaTime;

                // If the tween isn't finished, skip.
                if (_heartbeatJumpTime > _timeSinceStartedJump)
                    return;

                _startScale = transform.localScale = _targetScale;
                _runningReturn = true;
                _runningJump = false;
            }
            else if (_runningReturn)
            {
                transform.localScale = Vector3.Lerp(_startScale, _initialScale,
                    Easer.Apply(_heartbeatReturnEase, _timeSinceStartedReturn / _heartbeatReturnTime));
                _timeSinceStartedReturn += Time.deltaTime;

                // If the tween isn't finished, skip.
                if (_heartbeatReturnTime > _timeSinceStartedReturn)
                    return;

                transform.localScale = _initialScale;
                _runningReturn = false;
            }
        }
        
        public MonolithItem SetPath(string path)
        {
            Path = path;
            return this;
        }

        public void RunHeartbeat()
        {
            // Manually writing my own tweens because there's really no better option for the performance
            // targets we're trying to hit.
            // Damn all libraries that add and remove components for each tween D :
            
            _runningJump = true;
            _runningReturn = false;
            _timeSinceStartedJump = 0;
            _timeSinceStartedReturn = 0;

            // Set the starting scale to our current scale, as we could already be in the middle of a tween
            _startScale = transform.localScale;
            _targetScale = _initialScale * _heartbeatJumpScale;
        }

        public void MoveTo(Vector3 to, float duration, EaseType easeType)
        {
            _runningMove = true;
            _positionEasing = easeType;
            
            _timeSinceStartedMoving = 0;
            _positionDuration = duration;
            
            _targetPosition = to;
            _startPosition = transform.localPosition;
        }
    }
}