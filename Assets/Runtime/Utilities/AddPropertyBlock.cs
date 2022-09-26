using UnityEngine;

namespace Weaver.Utilities
{
    [RequireComponent(typeof(Renderer))]
    public sealed class AddPropertyBlock : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Renderer>().SetPropertyBlock(new MaterialPropertyBlock());
        }
    }
}