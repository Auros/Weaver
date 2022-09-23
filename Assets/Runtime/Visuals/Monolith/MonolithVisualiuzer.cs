using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using MessagePipe;
using UnityEngine;
using VContainer;
using Weaver.Models;
using Random = System.Random;

namespace Weaver.Visuals.Monolith
{
    /// <summary>
    /// In this Weaver visualizer, 
    /// </summary>
    public class MonolithVisualiuzer : MonoBehaviour
    {
        [Inject]
        private readonly ISubscriber<string, WeaverNode> _nodeEventSubscriber = null!;
        
        [Inject]
        private readonly ISubscriber<string, WeaverItemEvent> _itemEventSubscriber = null!;
                
        [Inject]
        private MonolithNodePoolController _monolithNodePoolController = null!;

        [Inject]
        private MonolithOwnerPoolController _monolithOwnerPoolController = null!;

        [SerializeField, Min(0.01f)]
        private float _nodeSpawnDistance = 10f;

        [SerializeField]
        private float _nodeLaunchForce = 500f;

        [SerializeField]
        private float _nodeSpawnOffset = 5f;

        [SerializeField]
        private string _seed = nameof(MonolithVisualiuzer);

        [SerializeField]
        private AnimationCurve _nodeWeightCurve = null!;

        [SerializeField]
        private CinemachineTargetGroup _cinemachineTargetGroup = null!;
        
        private Random _random = new();
        private IDisposable? _subscriptionDisposer;
        private readonly List<MonolithOwner> _activeOwners = new();
        private readonly Dictionary<string, MonolithNode> _physicalNodes = new();
        private readonly Dictionary<string, MonolithOwner> _physicalOwners = new();

        private void Start()
        {
            var disposer = DisposableBag.CreateBuilder();
            _nodeEventSubscriber.Subscribe(WeaverEventKeys.NodeCreated, NodeCreated).AddTo(disposer);
            _nodeEventSubscriber.Subscribe(WeaverEventKeys.NodeDestroyed, NodeDestroyed).AddTo(disposer);
            _itemEventSubscriber.Subscribe(WeaverEventKeys.ItemCreated, ItemCreated).AddTo(disposer);
            _itemEventSubscriber.Subscribe(WeaverEventKeys.ItemChanged, ItemChanged).AddTo(disposer);
            _itemEventSubscriber.Subscribe(WeaverEventKeys.ItemDestroyed, ItemDestroyed).AddTo(disposer);
            _subscriptionDisposer = disposer.Build();

            _random = new Random(_seed.GetHashCode());
        }

        private void NodeCreated(WeaverNode node)
        {
            // We use System.Random because I want randomness to be seeded by the visualizer instance.
            float V() => Mathf.Lerp(-1f, 1f, (float)_random.NextDouble());
            
            var physical = GetPhysicalNode(node);
            var parent = node.Parent != null ? GetPhysicalNode(node.Parent) : null;
            
            var isAncestorChild = parent == null;
            var physicalTransform = physical.transform;
            var parentTransform = parent != null ? parent.transform : null;
            
            var nodePos = isAncestorChild ? physicalTransform.localPosition : parentTransform!.localPosition;
            var ancestorPos = _physicalNodes[string.Empty].transform.localPosition;
            var normalizedMoveVector = (nodePos - ancestorPos).normalized;
            var parentPos = isAncestorChild ? ancestorPos : parentTransform!.localPosition;
            
            var calculatedPos = isAncestorChild switch
            {
                true => physicalTransform.TransformDirection(normalizedMoveVector * _nodeSpawnOffset + new Vector3(V(), V(), V()) * _nodeSpawnDistance),
                false => parentPos + parentTransform!.TransformDirection(normalizedMoveVector * _nodeSpawnOffset + new Vector3(V(), V(), V()) * _nodeSpawnDistance)
            };

            physicalTransform.localPosition = calculatedPos;
            
            physical.Launch(normalizedMoveVector * _nodeLaunchForce);
            
            foreach (var item in node.Items)
                CreateItem(node, item);
        }

        private void NodeDestroyed(WeaverNode node)
        {
            if (!_physicalNodes.TryGetValue(node.Name, out var physicalNode))
                return;

            foreach (var item in node.Items)
                DestroyItem(node, item , false); // We don't want to clear items, as the node clear call below will handle that.

            foreach (var owner in _activeOwners)
                owner.ClearActionsForNode(physicalNode);
            
            _physicalNodes.Remove(node.Name);
            _cinemachineTargetGroup.RemoveMember(physicalNode.transform);
            _monolithNodePoolController.Release(physicalNode);
        }

        private void ItemCreated(WeaverItemEvent item)
        {
            CreateItem(item.Node, item.Item);
        }
        
        private void ItemDestroyed(WeaverItemEvent item)
        {
            DestroyItem(item.Node, item.Item);
        }

        private void CreateItem(WeaverNode node, WeaverItem item)
        {
            var physicalNode = GetPhysicalNode(node);
            var physicalOwner = GetPhysicalOwner(node.Owner);
            physicalOwner.AddAction(physicalNode, item, MonolithActionType.Created);
        }
        
        private void ItemChanged(WeaverItemEvent @event)
        {
            var physicalNode = GetPhysicalNode(@event.Node);
            GetPhysicalOwner(@event.Node.Owner).AddAction(physicalNode, @event.Item, MonolithActionType.Changed);
        }
        private void DestroyItem(WeaverNode node, WeaverItem item, bool clearActions = true)
        {
            var physicalNode = GetPhysicalNode(node);
            var physicalOwner = GetPhysicalOwner(node.Owner);
            physicalOwner.AddAction(physicalNode, item, MonolithActionType.Destroyed);

            if (!clearActions)
                return;
            
            foreach (var owner in _activeOwners)
                if (owner != physicalOwner) // Skip over ourselves, we want that action to get fired off.
                    owner.ClearActionsForItem(item);
        }

        private void OnDestroy()
        {
            _subscriptionDisposer?.Dispose();
        }

        private MonolithNode GetPhysicalNode(WeaverNode node)
        {
            // If there's already a physical node active with that name, use it.
            if (_physicalNodes.TryGetValue(node.Name, out var physical))
                return physical;

            // Grab a physical node from the pool and set its path and mass.
            physical = _monolithNodePoolController
                .Get()
                .SetPath(node.Name)
                .SetMass(_nodeWeightCurve.Evaluate(node.Generation));
            
            // Build up the parent nodes to set the parent transform.
            // Notice: Recursively builds the parents until it reaches an ancestor.
            if (node.Parent is not null)
                physical.LinkParent(GetPhysicalNode(node.Parent));
            else
            {
                // Upon creation, ensure that the root node is positioned at the center of our canvas.
                
                var physicalTransform = physical.transform;
                physicalTransform.localPosition = Vector3.zero;
                physicalTransform.localRotation = Quaternion.identity;
            }
            
            _cinemachineTargetGroup.AddMember(physical.transform, 1f, 20f);
            _physicalNodes.Add(node.Name, physical);
            return physical;
        }

        private MonolithOwner GetPhysicalOwner(WeaverOwner owner)
        {
            // If there's already an owner active that matches, use it.
            if (_physicalOwners.TryGetValue(owner.Id, out var physical))
                return physical;

            physical = _monolithOwnerPoolController.Get();
            _physicalOwners.Add(owner.Id, physical);
            _activeOwners.Add(physical); // We also add it to a list so we can safely iterate it without allocation.
            return physical;
        }
    }
}