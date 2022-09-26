using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weaver.Visuals.Utilities
{
    public sealed class LaunchObjectAtStart : MonoBehaviour
    {
        [SerializeField]
        private Transform _from = null!;

        [SerializeField]
        private Vector3 _value;

        [SerializeField]
        private Vector3 _axis;

        private void OnDrawGizmos()
        {
            float V() => Random.Range(-1f, 1f);
            var pos = transform.position;

            //var normalizedValue = new Vector3(V(), V(), V()); // _value.normalized;
            var normalizedAxis = _axis.normalized;

            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(pos, pos + (normalizedValue + normalizedAxis) * 5f);
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(pos, pos + normalizedAxis * 5f);
            
            if (_from == null)
                return;
            
            var fromPos = _from.position;
            var validVector = pos - fromPos;
            
            //var rv = Quaternion.AngleAxis()
            
            Gizmos.DrawLine(pos, pos + (validVector.normalized + new Vector3(V(), V(), V())) * 10);
            Gizmos.DrawLine(fromPos, pos);
            
            Gizmos.DrawSphere(pos, 1f);
            
            Gizmos.color = Color.blue;
            
            Gizmos.DrawLine(pos, pos + validVector.normalized * 5f);
        }

        private void Start()
        {
            var rb = GetComponent<Rigidbody>();
            if (!rb)
                return;
            
            rb!.AddForce(transform.TransformDirection(new Vector3(Random.value, Random.value, Random.value) * 100));
        }
    }
}