using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;
using VContainer;
using Weaver.Models;
using Weaver.Visuals.Utilities;
using Random = UnityEngine.Random;

namespace Weaver.Visuals.Basic
{
    public sealed class BasicNodeRenderer : MonoBehaviour
    {
        [Inject]
        private readonly ISubscriber<string, WeaverNode> _nodeSubscriber = null!;

        [SerializeField]
        private GameObject _nodePrefab = null!;
        
        private IDisposable? _subscriptionDisposer;

        private readonly Dictionary<string, PhysicalNode> _physicalNodes = new();

        private class PhysicalNode
        {
            public Transform Transform { get; }
            public Transform? Parent { get; set; }
            public LineRenderer? Renderer { get; }

            public void ManualUpdate()
            {
                if (Renderer == null || Parent == null)
                    return;
                
                Renderer.SetPosition(0, Transform.position);
                Renderer.SetPosition(1, Parent.position);
            }

            public PhysicalNode(Transform transform, LineRenderer? renderer)
            {
                if (renderer != null)
                {
                    renderer.positionCount = 2;
                    renderer.startWidth = renderer.endWidth = 0.15f;
                }
                
                Transform = transform;
                Renderer = renderer;
            }
        }

        private void Start()
        {
            var disposer = DisposableBag.CreateBuilder();

            _nodeSubscriber.Subscribe(WeaverEventKeys.NodeCreated, NodeCreated).AddTo(disposer);
            _nodeSubscriber.Subscribe(WeaverEventKeys.NodeDestroyed, NodeDestroyed).AddTo(disposer);
            
            _subscriptionDisposer = disposer.Build();
        }

        private void Update()
        {
            foreach (var item in _physicalNodes.Values)
                item.ManualUpdate();
        }

        private void OnDestroy()
        {
            _subscriptionDisposer?.Dispose();
        }

        private void NodeCreated(WeaverNode node)
        {
            if (node.Parent == null)
                return;

            var physicalNode = GetPhysicalNode(node);

            var physicalParent = GetPhysicalNode(node.Parent);
            physicalNode.Parent = physicalParent.Transform;

            static float V() => Random.Range(-1f, 1f);
            
            // random point calculation on sphere
            
            const float distance = 15f;

            if (node.Parent.Parent == null)
            {
                // Ancestor Child
                var nodePosition = physicalNode.Transform.position;
                var ancestorPosition = physicalParent.Transform.position;
                var normalizedMoveVector = (nodePosition - ancestorPosition).normalized;
                physicalNode.Transform.localPosition = physicalNode.Transform.TransformDirection(normalizedMoveVector + new Vector3(V(), V(), V()) * distance);
            }
            else
            {
                var nodePosition = physicalParent.Transform.position;
                var ancestorPosition = _physicalNodes[string.Empty].Transform.position;
                var normalizedMoveVector = (nodePosition - ancestorPosition).normalized;
                physicalNode.Transform.localPosition = nodePosition + physicalParent.Transform.TransformDirection(normalizedMoveVector * 5f) + new Vector3(V(), V(), V());

                var rb = physicalNode.Transform.GetComponent<Rigidbody>();
                if (rb == null)
                    return;
                
                rb.AddForce(physicalNode.Transform.TransformDirection(normalizedMoveVector * 500f));
            }
            /*
            var ancestorPosition = physicalParent.Transform.position;
            // If it's not a direct child of the ancestor
            if (node.Parent.Parent != null)
                ancestorPosition = _physicalNodes[string.Empty].Transform.position;
            

            //var ancestorPosition = physicalParent.Transform.position;
            var nodePosition = physicalNode.Transform.position;
            var normalizedMoveVector = (nodePosition - ancestorPosition).normalized;
            //var randomMovementVector = nodePosition + (normalizedMoveVector + new Vector3(V(), V(), V()) * distance);

            physicalNode.Transform.localPosition = physicalParent.Transform.localPosition +
                physicalNode.Transform.TransformDirection(normalizedMoveVector + new Vector3(V(), V(), V()) * distance);*/
        }
        
        private void NodeDestroyed(WeaverNode node)
        {
            if (!_physicalNodes.TryGetValue(node.Name, out var physicalNode))
                return;

            _physicalNodes.Remove(node.Name);
            Destroy(physicalNode.Transform.gameObject);
        }

        private PhysicalNode GetPhysicalNode(WeaverNode input)
        {
            if (_physicalNodes.TryGetValue(input.Name, out var physicalNode))
                return physicalNode;

            var nodeObject = Instantiate(_nodePrefab, transform);
            if (input.Name == string.Empty)
            {
                nodeObject.transform.localScale *= 2f;
                nodeObject.GetComponent<MeshRenderer>().material.color = Color.cyan;
            }

            physicalNode = new PhysicalNode(nodeObject.transform, nodeObject.AddComponent<LineRenderer>());
            physicalNode.Transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            nodeObject.gameObject.SetActive(true);

            nodeObject.gameObject.AddComponent<IncludeDebugInfo>().Information = input.Name;

            _physicalNodes.Add(input.Name, physicalNode);
            
            return physicalNode;
        }
    }
}