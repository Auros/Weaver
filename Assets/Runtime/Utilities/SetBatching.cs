using UnityEngine;
using UnityEngine.Rendering;

namespace Weaver.Utilities
{
    public class SetBatching : MonoBehaviour
    {
        [SerializeField]
        private bool _value = true;

        private void Awake()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = _value;
        }
    }
}