using UnityEngine;

namespace Weaver
{
    public static class WeaverExtensions
    {
        public static Vector3 Vector3(this System.Random random)
        {
            float V() => Mathf.Lerp(-1f, 1f, (float)random.NextDouble());
            return new Vector3(V(), V(), V());
        } 
    }
}