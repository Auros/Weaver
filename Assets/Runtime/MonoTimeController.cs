using UnityEngine;
using Weaver.Models;

namespace Weaver
{
    public class MonoTimeController : MonoBehaviour, IClock
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
            return 1f - _value / _upperBound;
        }
    }
}