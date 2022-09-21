using UnityEngine;

namespace Weaver.Visuals.Monolith
{
    public class MonolithNode : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _parentLinkerLine = null!;

        private Transform? _nodeParentTransform;
        private readonly Vector3[] _linePositionSetter = new Vector3[2];

        public string Path { get; private set; } = string.Empty;
        
        public MonolithNode LinkParent(Transform nodeParentTransform)
        {
            _nodeParentTransform = nodeParentTransform;
            return this;
        }

        public MonolithNode UnlinkParent()
        {
            _nodeParentTransform = null;
            return this;
        }

        public MonolithNode SetPath(string path)
        {
            Path = path;
            return this;
        }

        private void Update()
        {
            // If there's no parent or line, there's nothing for us to update.
            if (_nodeParentTransform == null || _parentLinkerLine == null)
                return;

            // Update the line renderer to show ourselves and our parent as being "connected".
            _linePositionSetter[0] = transform.position;
            _linePositionSetter[1] = _nodeParentTransform.position;
            _parentLinkerLine.SetPositions(_linePositionSetter);
        }
    }
}