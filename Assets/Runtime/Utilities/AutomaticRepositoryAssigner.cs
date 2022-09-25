using System.IO;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using Weaver.Models;

namespace Weaver.Utilities
{
    public class AutomaticRepositoryAssigner : MonoBehaviour
    {
        [Inject]
        private IPublisher<WeaverAssembler?> _assemblerPublisher = null!;

        [Inject]
        private IClock _clock = null!;

        [SerializeField]
        private string _repositoryPath = string.Empty;

        [SerializeField]
        private bool _resetTimeOnChange;
        
        private string _valueLastFrame = string.Empty;

        private WeaverAssembler? _lastAssembler;
        
        private void Update()
        {
            if (_valueLastFrame == _repositoryPath)
                return;

            _valueLastFrame = _repositoryPath;
            
            _ = UniTask.RunOnThreadPool(async () =>
            {
                
                var repo = _repositoryPath;
                if (!_repositoryPath.EndsWith("\\.git"))
                    repo += "\\.git";
                // Build the weaver assembler on a separate thread.
                // It can take quite some time for it to build the mappings for every object.
                // I might make the snapshots lazy loaded in the future.
                var assembler = Directory.Exists(repo) ? await WeaverAssembler.Create(repo) : null;
                
                _lastAssembler?.Dispose();
                _lastAssembler = null;

                _lastAssembler = assembler;

                await UniTask.SwitchToMainThread();
                
                if (_resetTimeOnChange)
                    _clock.SetCurrentTime(0);
                
                _assemblerPublisher.Publish(assembler);
            });
        }
    }
}