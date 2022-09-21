using ElRaccoone.Tweens.Core;
using JetBrains.Annotations;
using UnityEngine;
using Weaver.Visuals.Utilities;

namespace Weaver.Visuals.Monolith
{
    [PublicAPI]
    public class MonolithNode : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidbody = null!;

        [SerializeField]
        private IncludeDebugInfo _debug = null!;
        
        [SerializeField]
        private LineRenderer _parentLinkerLine = null!;

        [SerializeField]
        private float _timeToConnectFromParent = 1f;

        [SerializeField]
        private EaseType _connectionEasing = EaseType.CubicOut; 
        
        private MonolithNode? _nodeParent;
        private readonly Vector3[] _linePositionSetter = new Vector3[2];

        public string Path { get; private set; } = string.Empty;

        public bool HasBuiltLine => _timeSinceParentConnection >= _timeToConnectFromParent;

        private float _timeSinceParentConnection;
        
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

        public void Launch(Vector3 inDirection)
        {
            _rigidbody.AddForce(transform.TransformDirection(inDirection));
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