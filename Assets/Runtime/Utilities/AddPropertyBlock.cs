using UnityEngine;

namespace Weaver.Utilities
{
    [RequireComponent(typeof(Renderer))]
    public class AddPropertyBlock : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Renderer>().SetPropertyBlock(new MaterialPropertyBlock());
        }
    }
}