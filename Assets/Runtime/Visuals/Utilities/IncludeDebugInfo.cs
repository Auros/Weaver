using UnityEngine;

namespace Weaver.Visuals.Utilities
{
    public class IncludeDebugInfo : MonoBehaviour
    {
        [field: SerializeField]
        public string Information { get; set; } = string.Empty;
    }
}