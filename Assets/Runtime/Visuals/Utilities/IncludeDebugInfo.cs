using System.Collections.Generic;
using UnityEngine;

namespace Weaver.Visuals.Utilities
{
    public sealed class IncludeDebugInfo : MonoBehaviour
    {
        [field: SerializeField]
        public string Information { get; set; } = string.Empty;

        [field: SerializeField]
        public List<string> Items = new();
    }
}