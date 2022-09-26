using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Weaver.UI.Components
{
    public sealed class MoveToTargetOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private float _movementTime = 0.5f;

        [SerializeField]
        private EaseType _movementEasing = EaseType.CubicOut;

        [SerializeField]
        private RectTransform _targetTransform = null!;
        
        [SerializeField]
        private RectTransform _activeTargetPosition = null!;
        
        [SerializeField]
        private RectTransform _inactiveTargetPosition = null!;

        private Tween<Vector3>? _tween;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tween != null)
                _tween.Cancel();
            _tween = _targetTransform.TweenPosition(_activeTargetPosition.position, _movementTime).SetEase(_movementEasing);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tween != null)
                _tween.Cancel();
            _tween = _targetTransform.TweenPosition(_inactiveTargetPosition.position, _movementTime).SetEase(_movementEasing);
        }
    }
}