using System.Collections.Generic;
using System.Linq;
using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;
using Weaver.Models;
using Weaver.Visuals.Utilities;

namespace Weaver.Visuals.Monolith
{
    [PublicAPI]
    public class MonolithNode : MonoBehaviour
    {
        [Inject]
        private MonolithItemPoolController _monolithItemPoolController = null!;
   
        [SerializeField]
        private Rigidbody _rigidbody = null!;

        [SerializeField]
        private IncludeDebugInfo _debug = null!;
        
        [SerializeField]
        private LineRenderer _parentLinkerLine = null!;

        [SerializeField, Min(0)]
        private float _itemTweeningMovementSpeed = 0.5f;
        
        [SerializeField]
        private float _timeToConnectFromParent = 1f;

        [SerializeField]
        private EaseType _connectionEasing = EaseType.CubicOut;

        [SerializeField]
        private EaseType _itemMovementEasing = EaseType.CubicOut;
        
        private MonolithNode? _nodeParent;
        private float _timeSinceParentConnection;
        private readonly Vector3[] _linePositionSetter = new Vector3[2];

        public string Path { get; private set; } = string.Empty;

        public bool HasBuiltLine => _timeSinceParentConnection >= _timeToConnectFromParent;

        public List<MonolithItem> ActiveItems { get; set; } = new();

        // Golden Ratio
        private static readonly float _phi = Mathf.PI * (3f - Mathf.Sqrt(5));
        
        public MonolithNode LinkParent(MonolithNode nodeParent)
        {
            // We don't want to reset the timer if the parent is the same.
            if (_nodeParent == nodeParent)
                return this;
            
            _nodeParent = nodeParent;
            _timeSinceParentConnection = 0f;
            return this;
        }

        public MonolithNode UnlinkParent()
        {
            _nodeParent = null;
            return this;
        }

        public MonolithNode SetPath(string path)
        {
            _debug.Information = path;
            Path = path;
            return this;
        }

        public MonolithNode SetMass(float mass)
        {
            _rigidbody.mass = mass;
            return this;
        }

        public void Launch(Vector3 inDirection)
        {
            _rigidbody.AddForce(transform.TransformDirection(inDirection));
        }

        public MonolithItem AddItem(WeaverItem weaverItem)
        {
            var item = _monolithItemPoolController.Get().SetPath(weaverItem.Name);
            ActiveItems.Add(item);
            _debug.Items.Add(item.Path);
            
            var itemTransform = item.transform;
            itemTransform.SetParent(transform);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
            item.RunHeartbeat();
            UpdateItems();

            return item;
        }

        public void RemoveItem(WeaverItem weaverItem)
        {
            var item = ActiveItems.FirstOrDefault(a => a.Path != weaverItem.Name);
            if (item == null)
                return;
            
            _monolithItemPoolController.Release(item);
            ActiveItems.Remove(item);
            _debug.Items.Remove(item.Path);
            UpdateItems();
        }

        private void UpdateItems()
        {
            if (ActiveItems.Count == 0)
                return;
            
            var size = ActiveItems.Count;
            
            // Get the square root of the number of elements we have
            // to naturally scale the size of the node over time.
            var radiusModifier = Mathf.Pow(size, 1f / 3f);

            for (int i = 0; i < size; i++)
            {
                var y = 1f - i / (size - 1f) * 2f;
                var radius = Mathf.Sqrt(1f - y * y);
                var theta = _phi * i;
                var x = Mathf.Cos(theta) * radius;
                var z = Mathf.Sin(theta) * radius;

                // Stop computing if we didn't calculate the correct number.
                if (float.IsNaN(x))
                    break;

                //ActiveItems[i].transform.localPosition = new Vector3(x, y, z) * radiusModifier;
                ActiveItems[i].transform
                    .TweenLocalPosition(new Vector3(x, y, z) * radiusModifier, _itemTweeningMovementSpeed)
                    .SetEase(_itemMovementEasing);
            }
        }

        private void Update()
        {
            // If the parent hasn't been set, or the parent has had it's line built, add onto the 
            // lifetime tracker for ourselves.
            if (_nodeParent == null || _nodeParent.HasBuiltLine)
                _timeSinceParentConnection += Time.deltaTime;
            
            // If there's no line, there's nothing left for us to update.
            if (_parentLinkerLine == null)
                return;

            var hasParent = _nodeParent != null;
            
            // Update the line renderer to show ourselves and our parent as being "connected".
            // The tweening slowly moves the end of the line from the parent source to ourselves, to have a more
            // animated effect when nodes spawn in.
            _linePositionSetter[0] = hasParent
                ? _timeSinceParentConnection >= _timeToConnectFromParent 
                    ? transform.position
                        : Vector3.Lerp( 
                            _nodeParent!.transform.position, transform.position, 
                            Easer.Apply(_connectionEasing, 
                            Mathf.InverseLerp(0f, _timeToConnectFromParent, _timeSinceParentConnection))) 
                : Vector3.zero;
            
            _linePositionSetter[1] = hasParent ? _nodeParent!.transform.position : Vector3.zero;
            _parentLinkerLine.SetPositions(_linePositionSetter);
        }
    }
}