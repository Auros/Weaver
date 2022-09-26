using ElRaccoone.Tweens.Core;
using UnityEngine;
using VContainer;
using Weaver.Tweening;

namespace Weaver.Visuals.Monolith
{
    public sealed class MonolithItem : MonoBehaviour
    {
        [Inject]
        private readonly TweeningController _tweeningController = null!;
        
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

        private Vector3 _initialScale;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        public MonolithItem SetPath(string path)
        {
            Path = path;
            return this;
        }

        public void RunHeartbeat()
        {
            // Start the heartbeat jump
            // This is the enlarging of the item.
            _tweeningController.Clear(this);
            _tweeningController.AddTween(
                transform.localScale,
                _initialScale * _heartbeatJumpScale,
                vector => transform.localScale = vector,
                _heartbeatJumpTime,
                _heartbeatJumpEase,
                this, onComplete: () =>
                {
                    // Once the jump finishes, we run the return
                    // This is the shrinking of the item.
                    _tweeningController.AddTween(
                        transform.localScale,
                        _initialScale,
                        vector => transform.localScale = vector,
                        _heartbeatReturnTime,
                        _heartbeatReturnEase,
                        this
                    );
                }
            );
        }
    }
}