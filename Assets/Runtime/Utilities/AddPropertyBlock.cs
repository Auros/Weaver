using System;
using UnityEngine;

namespace Weaver.Runtime.Utilities
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