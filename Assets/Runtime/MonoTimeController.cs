using UnityEngine;
using Weaver.Models;

namespace Weaver
{
    public sealed class MonoTimeController : MonoBehaviour, IClock
    {
        [SerializeField]
        private float _upperBound = 10f;

        private float _value;

        private void Update()
        {
            _value += Time.deltaTime;
        }

        public float GetCurrentTime()
        {
            return _value / _upperBound;
        }

        public void SetCurrentTime(float value)
        {
            _value = value * _upperBound;
        }
    }
}