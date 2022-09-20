using System;
using System.Linq;
using MessagePipe;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Weaver.Models;

namespace Weaver
{
    public sealed class WeaverStateDaemon : IStartable, ITickable, IDisposable
    {
        private readonly IClock _clock;
        private readonly IPublisher<string, WeaverNode> _nodeCreated;
        private readonly IPublisher<string, WeaverNode> _nodeDestroyed;
        private readonly IPublisher<string, WeaverItemEvent> _itemChanged;
        private readonly IPublisher<string, WeaverItemEvent> _itemCreated;
        private readonly IPublisher<string, WeaverItemEvent> _itemDestroyed;
        private readonly ISubscriber<WeaverAssembler?> _weaverAssemblerEvent;

        private int? _lastSnapshotIndex;
        private WeaverAssembler? _assembler;
        private IDisposable? _subscriptionDisposer;

        private readonly WeaverItemEvent _itemEventHolder = new();
        
        public WeaverStateDaemon(
            IClock clock,
            IPublisher<string, WeaverNode> nodeCreated,
            IPublisher<string, WeaverNode> nodeDestroyed,
            IPublisher<string, WeaverItemEvent> itemChanged,
            IPublisher<string, WeaverItemEvent> itemCreated,
            IPublisher<string, WeaverItemEvent> itemDestroyed,
            ISubscriber<WeaverAssembler?> weaverAssemblerEvent)
        {
            _clock = clock;
            _nodeCreated = nodeCreated;
            _nodeDestroyed = nodeDestroyed;
            _itemChanged = itemChanged;
            _itemCreated = itemCreated;
            _itemDestroyed = itemDestroyed;
            _weaverAssemblerEvent = weaverAssemblerEvent;
        }
        
        public void Start()
        {
            var disposer = DisposableBag.CreateBuilder();
            _weaverAssemblerEvent.Subscribe(AssemblerChanged).AddTo(disposer);
            _subscriptionDisposer = disposer.Build();
        }

        private void AssemblerChanged(WeaverAssembler? assembler)
        {
            _assembler = assembler;

            if (_assembler is null)
            {
                _lastSnapshotIndex = null;
                return;
            }
            
            var current = _clock.GetCurrentTime();
            var snapshot = _assembler.Snapshots.FirstOrDefault(a => a.Time >= current);
            _lastSnapshotIndex = snapshot is not null ? Array.IndexOf(_assembler.Snapshots, snapshot) : 0;
        }

        public void Tick()
        {
            // Don't do anything if there's no active assembler or snapshot.
            if (_assembler is null || _lastSnapshotIndex is null)
                return;
            
            // Get the time from the current clock.
            var time = _clock.GetCurrentTime();
            
            // Get the last active snapshot
            var lastSnapshot = _assembler.Snapshots[_lastSnapshotIndex.Value];
            
            // Check to see if we moved forwards or backwards in time.
            var movedForwards = time - lastSnapshot.Time >= 0;
            
            // Now we get the snapshot tied to the current time.
            WeaverSnapshot? snapshot = null;

            if (movedForwards)
            {
                // We traversed forwards into the snapshots, look up the array until
                // we are outside of the search range, or we find the match.
                for (int i = _lastSnapshotIndex.Value + 1; i < _assembler.Snapshots.Length; i++)
                {
                    var localSnapshot = _assembler.Snapshots[i];
                    
                    // If the snapshot we're looking at is greater than the current time,
                    // we can exit early since we haven't reached that time yet.
                    if (localSnapshot.Time > time)
                        break;

                    snapshot = localSnapshot;
                }
            }
            else
            {
                // We traversed backwards from the snapshots, look down the array until
                // we are outside of the search range, or we find the match.
                for (int i = _lastSnapshotIndex.Value - 1; i >= 0; i--)
                {
                    var localSnapshot = _assembler.Snapshots[i];
                    
                    // If the snapshot we're looking at is greater than the current time,
                    // we can skip over it, as there are more snapshots before it that are valid.
                    if (localSnapshot.Time > time)
                        continue;

                    // If not, we can set the value, as this is the closest value.
                    snapshot = localSnapshot;
                    break;
                }
            }

            // If we couldn't find a snapshot, then there's nothing left for us to do!
            if (snapshot is null)
                return;
            
            // The current snapshot is the same as the last snapshot, nothing to do!
            if (snapshot == lastSnapshot)
                return;

            _lastSnapshotIndex = Array.IndexOf(_assembler.Snapshots, snapshot);
            PublishSnapshotChanges(lastSnapshot, snapshot);
        }

        private void PublishSnapshotChanges(WeaverSnapshot oldSnapshot, WeaverSnapshot newSnapshot)
        {
            PublishNodeChanges(oldSnapshot.Ancestor, newSnapshot.Ancestor);
        }

        private void PublishNodeChanges(WeaverNode oldNode, WeaverNode newNode)
        {
            var changed = ListPool<Action>.Get();

            // Find nodes that have been deleted.
            for (int i = 0; i < oldNode.Children.Length; i++)
            {
                var node = oldNode.Children[i];
                var newChild = newNode.Children.FirstOrDefault(n => n.Name == node.Name);
                if (newChild is not null)
                {
                    // Add delegate for invoking the change event later
                    changed.Add(() => PublishNodeChanges(node, newChild));
                    continue;
                }
                
                PublishNodeDeletion(node);
            }

            for (int i = 0; i < oldNode.Items.Length; i++)
            {
                var item = oldNode.Items[i];
                var newItem = newNode.Items.FirstOrDefault(it => it.Name == item.Name);
                if (newItem is null)
                {
                    // An item has been deleted
                    _itemEventHolder.Item = item;
                    _itemEventHolder.Node = newNode;
                    _itemDestroyed.Publish(WeaverEventKeys.ItemDestroyed, _itemEventHolder);
                }
                else if (item.Hash != newItem.Hash)
                {
                    // An item has been updated
                    _itemEventHolder.Item = newItem;
                    _itemEventHolder.Node = newNode;
                    _itemChanged.Publish(WeaverEventKeys.ItemChanged, _itemEventHolder);
                }
            }

            for (int i = 0; i < newNode.Items.Length; i++)
            {
                var item = newNode.Items[i];
                
                // If the newer node item exists in the older node, skip.
                if (oldNode.Items.Any(it => it.Name == item.Name))
                    continue;

                // An item has been created
                _itemEventHolder.Item = item;
                _itemEventHolder.Node = newNode;
                _itemCreated.Publish(WeaverEventKeys.ItemCreated, _itemEventHolder);
            }
            
            // Invoke all the child node changes
            foreach (var change in changed)
                change.Invoke();
            
            // Find nodes that have been added
            for (int i = 0; i < newNode.Children.Length; i++)
            {
                var node = newNode.Children[i];
                var oldChild = oldNode.Children.FirstOrDefault(n => n.Name == node.Name);
                if (oldChild is not null)
                    continue;
                
                PublishNodeCreation(node);
            }
            
            ListPool<Action>.Release(changed);
        }

        private void PublishNodeCreation(WeaverNode node)
        {
            _nodeCreated.Publish(WeaverEventKeys.NodeCreated, node);
            
            // Update all children
            foreach (var child in node.Children)
                PublishNodeCreation(child);
        }

        private void PublishNodeDeletion(WeaverNode node)
        {
            // Update all children
            foreach (var child in node.Children)
                PublishNodeDeletion(child);
            
            _nodeDestroyed.Publish(WeaverEventKeys.NodeDestroyed, node);
        }
        
        public void Dispose()
        {
            _subscriptionDisposer?.Dispose();
        }
    }
}