using UnityEngine;

namespace Weaver.Visuals.Utilities
{
    public class SphereItem : MonoBehaviour
    {
        [field: SerializeField, Min(0.1f)]
        public float Radius { get; private set; } = 1f;
    }
}