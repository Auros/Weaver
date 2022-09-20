using System;
using System.Linq;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;
using Weaver.Models;

namespace Weaver
{
    public sealed class WeaverStateDaemon : IStartable, ITickable, IDisposable
    {
        private readonly IClock _clock;
        private readonly ISubscriber<WeaverAssembler?> _weaverAssemblerEvent;

        private IDisposable? _subscription;
        private WeaverAssembler? _assembler;
        private int? _lastSnapshotIndex;

        public WeaverStateDaemon(IClock clock, ISubscriber<WeaverAssembler?> weaverAssemblerEvent)
        {
            _clock = clock;
            _weaverAssemblerEvent = weaverAssemblerEvent;
        }
        
        public void Start()
        {
            _subscription = _weaverAssemblerEvent.Subscribe(AssemblerChanged);
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
            
            // TODO: Calculate snapshop differences and invoke events.
            Debug.Log(_lastSnapshotIndex.Value);
        }
        
        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}