using System;
using MessagePipe;
using UnityEngine;
using VContainer;
using Weaver.Models;

namespace Weaver.Visuals.Monolith
{
    public class MonolithVisualiuzer : MonoBehaviour
    {
        [Inject]
        private readonly ISubscriber<string, WeaverNode> _nodeEventSubscriber = null!;
        
        [Inject]
        private readonly ISubscriber<string, WeaverItemEvent> _itemEventSubscriber = null!;

        [SerializeField]
        private MonolithNodePoolController _monolithNodePoolController = null!;
        
        private IDisposable? _subscriptionDisposer;

        private void Start()
        {
            var disposer = DisposableBag.CreateBuilder();
            _nodeEventSubscriber.Subscribe(WeaverEventKeys.NodeCreated, NodeCreated).AddTo(disposer);
            _nodeEventSubscriber.Subscribe(WeaverEventKeys.NodeDestroyed, NodeDestroyed).AddTo(disposer);
            _itemEventSubscriber.Subscribe(WeaverEventKeys.ItemCreated, ItemCreated).AddTo(disposer);
            _itemEventSubscriber.Subscribe(WeaverEventKeys.ItemChanged, ItemChanged).AddTo(disposer);
            _itemEventSubscriber.Subscribe(WeaverEventKeys.ItemDestroyed, ItemDestroyed).AddTo(disposer);
            _subscriptionDisposer = disposer.Build();
        }

        private void NodeCreated(WeaverNode node)
        {
            
        }

        private void NodeDestroyed(WeaverNode node)
        {
            
        }

        private void ItemCreated(WeaverItemEvent item)
        {
            
        }
        
        private void ItemChanged(WeaverItemEvent item)
        {
            
        }
        
        private void ItemDestroyed(WeaverItemEvent item)
        {
            
        }

        private void OnDestroy()
        {
            _subscriptionDisposer?.Dispose();
        }
    }
}