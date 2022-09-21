using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;
using UnityEngine.Pool;
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
        
        [Inject]
        private readonly ISubscriber<string, WeaverItemEvent> _itemSubscriber = null!;

        [SerializeField]
        private GameObject _nodePrefab = null!;

        [SerializeField]
        private BasicNodeItem _nodeItemPrefab = null!;
        
        private IDisposable? _subscriptionDisposer;
        
        private ObjectPool<BasicNodeItem> _itemPool = null!;

        private readonly Dictionary<string, PhysicalNode> _physicalNodes = new();

        private class PhysicalNode
        {
            public Transform Transform { get; }
            public Transform? Parent { get; set; }
            public LineRenderer? Renderer { get; }

            private int _lastItemCalculation;
            public List<BasicNodeItem> PhysicalItems { get; } = new();

            public void ManualUpdate()
            {
                if (PhysicalItems.Count != _lastItemCalculation && PhysicalItems.Count != 0)
                {
                    var size = PhysicalItems.Count;
                    var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));
                    var radiusModifier = Mathf.Pow(size, 1f / 3f);

                    for (int i = 0; i < size; i++)
                    {
                        var y = 1f - i / (size - 1f) * 2f;
                        var radius = Mathf.Sqrt(1f - y * y);

                        radius *= radiusModifier;
                
                        var theta = phi * i;
                        var x = Mathf.Cos(theta) * radius;
                        var z = Mathf.Sin(theta) * radius;

                        if (float.IsNaN(x))
                            break;
                
                        PhysicalItems[i].transform.localPosition = new Vector3(x, y, z);
                    }
                }
                _lastItemCalculation = PhysicalItems.Count;
                
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
            _itemSubscriber.Subscribe(WeaverEventKeys.ItemCreated, ItemCreated).AddTo(disposer);
            _itemSubscriber.Subscribe(WeaverEventKeys.ItemDestroyed, ItemDestroyed).AddTo(disposer);
            
            _subscriptionDisposer = disposer.Build();
            
            _itemPool = new ObjectPool<BasicNodeItem>(
                () => Instantiate(_nodeItemPrefab),
                item =>
                {
                    item.gameObject.SetActive(true);
                }, item =>
                {
                    item.transform.SetParent(null);
                    item.gameObject.SetActive(false);
                }, item =>
                {
                    Destroy(item.gameObject);
                });
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

        private void ItemCreated(WeaverItemEvent itemEvent)
        {
            CreateItem(itemEvent.Node);
        }

        private void CreateItem(WeaverNode node)
        {
            var physicalNode = GetPhysicalNode(node);
            var item = _itemPool.Get();
            item.transform.SetParent(physicalNode.Transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            physicalNode.PhysicalItems.Add(item);
        }

        private void ItemDestroyed(WeaverItemEvent itemEvent)
        {
            DestroyItem(itemEvent.Node);
        }

        private void DestroyItem(WeaverNode node)
        {
            var physicalNode = GetPhysicalNode(node);
            
            if (physicalNode.PhysicalItems.Count == 0)
                return;
            
            var item = physicalNode.PhysicalItems[^1];
            physicalNode.PhysicalItems.Remove(item);
            _itemPool.Release(item);
        }
        
        private void NodeCreated(WeaverNode node)
        {
            if (node.Parent == null)
                return;

            var physicalNode = GetPhysicalNode(node);

            var physicalParent = GetPhysicalNode(node.Parent);
            physicalNode.Parent = physicalParent.Transform;

            static float V() => Random.Range(-1f, 1f);
            
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
            
            foreach (var _ in node.Children)
                CreateItem(node);
        }
        
        private void NodeDestroyed(WeaverNode node)
        {
            if (!_physicalNodes.TryGetValue(node.Name, out var physicalNode))
                return;

            foreach (var _ in node.Children)
                DestroyItem(node);
            
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