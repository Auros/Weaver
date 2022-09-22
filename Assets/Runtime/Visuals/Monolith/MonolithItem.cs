using ElRaccoone.Tweens;
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

        public MonolithItem SetPath(string path)
        {
            Path = path;
            return this;
        }

        public MonolithItem RunHeartbeat()
        {
            this
                .TweenLocalScale(Vector3.one * _heartbeatJumpScale, _heartbeatJumpTime)
                .SetEase(_heartbeatJumpEase).SetOnComplete(() =>
                {
                    this.TweenLocalScale(Vector3.one, _heartbeatReturnTime).SetEase(_heartbeatReturnEase);
                });
            return this;
        }
    }
}