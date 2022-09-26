using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using Weaver.Models;

namespace Weaver.Utilities
{
    public class AutomaticRepositoryAssigner : MonoBehaviour, IProgress<float>
    {
        [Inject]
        private IPublisher<WeaverAssembler?> _assemblerPublisher = null!;

        [Inject]
        private IPublisher<string, float> _progressPublisher = null!;

        [Inject]
        private IClock _clock = null!;

        [SerializeField]
        private string _repositoryPath = string.Empty;

        [SerializeField]
        private bool _resetTimeOnChange;

        [SerializeField]
        private WeaverAssembler.LoadType _loadType;

        [SerializeField, Min(0)]
        private int _chunks;
        
        private string _valueLastFrame = string.Empty;

        private float _progress;
        private float _progressLastFrame;
        private WeaverAssembler? _lastAssembler;
        private CancellationTokenSource _cts = new();
        
        private void Update()
        {
            // If the progress has changed, publish it.
            if (!Mathf.Approximately(_progress, _progressLastFrame))
                _progressPublisher.Publish(WeaverEventKeys.LoadingProgress, _progress);
            _progressLastFrame = _progress;
            
            if (_valueLastFrame == _repositoryPath)
                return;
            
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _valueLastFrame = _repositoryPath;
            
            _ = UniTask.RunOnThreadPool(async () =>
            {
                var cts = _cts;
                var repo = _repositoryPath;
                if (!_repositoryPath.EndsWith("\\.git"))
                    repo += "\\.git";

                // Build the weaver assembler on a separate thread.
                // It can take quite some time for it to build the mappings for every object.
                // I might make the snapshots lazy loaded in the future.
                var assembler = Directory.Exists(repo) ? await WeaverAssembler.Create(repo, _loadType, this, _cts.Token, _chunks) : null;
                
                _lastAssembler?.Dispose();
                _lastAssembler = null;

                _lastAssembler = assembler;

                await UniTask.SwitchToMainThread();
                
                if (_resetTimeOnChange)
                    _clock.SetCurrentTime(0);
                
                _assemblerPublisher.Publish(assembler);

                if (_loadType != WeaverAssembler.LoadType.ParallelLazyLoaded &&
                    _loadType != WeaverAssembler.LoadType.LazyLoaded)
                    return;

                if (assembler == null)
                    return;
                
                // For lazy loaded assemblers, we start loading the nodes in the background.
                await UniTask.SwitchToThreadPool();

                for (int i = 0; i < assembler.Snapshots.Length; i++)
                {
                    if (cts.IsCancellationRequested)
                        break;

                    // Load the node recursively
                    LoadNode(assembler.Snapshots[i].Ancestor);
                }
                
            }, cancellationToken: _cts.Token);
        }

        private static void LoadNode(WeaverNode node)
        {
            for (int i = 0; i < node.Children.Length; i++)
                LoadNode(node.Children[i]);
        }
        
        private void OnDestroy()
        {
            _cts.Cancel();
        }

        public void Report(float value)
        {
            _progress = value;
        }
    }
}