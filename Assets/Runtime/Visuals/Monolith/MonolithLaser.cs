using System;
using UnityEngine;

namespace Weaver.Visuals.Monolith
{
    public class MonolithLaser : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _line = null!;

        [SerializeField, Min(0.1f)]
        private float _flashTime;

        [SerializeField]
        private AnimationCurve _alphaCurve = null!;

        private Transform? _source;
        private Transform? _destination;
        private float _timeSinceFlashStart;
        private Action<MonolithLaser>? _onComplete;
        private readonly Vector3[] _linePositionSetter = new Vector3[2];
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int _emissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly Color _whiteWithNoAlpha = new(1f, 1f, 1f, 0f);
        
        private void Update()
        {
            // If we finished flashing, don't do anything.
            if (_timeSinceFlashStart >= _flashTime)
                return;
            
            if (_source == null || _destination == null)
            {
                _linePositionSetter[0] = Vector3.zero;
                _linePositionSetter[1] = Vector3.zero;
                _line.SetPositions(_linePositionSetter);
                return;
            }

            _linePositionSetter[0] = _destination.position;
            _linePositionSetter[1] = _source.position;
            _line.SetPositions(_linePositionSetter);

            var color = new Color(1f, 1f, 1f, _alphaCurve.Evaluate(_timeSinceFlashStart / _flashTime));
            _line.material.SetColor(_baseColor, color);
            
            _timeSinceFlashStart += Time.deltaTime;
            if (_timeSinceFlashStart >= _flashTime)
                _onComplete?.Invoke(this);
        }

        public void Flash(Transform source, Transform destination, Color color, Action<MonolithLaser>? onComplete = null)
        {
            _source = source;
            _onComplete = onComplete;
            _destination = destination;
            _timeSinceFlashStart = 0f;
            _line.material.SetColor(_emissionColor, color);
            _line.material.SetColor(_baseColor, _whiteWithNoAlpha);
        }
    }
}