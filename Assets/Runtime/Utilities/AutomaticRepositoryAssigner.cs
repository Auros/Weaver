using System.IO;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace Weaver.Utilities
{
    public class AutomaticRepositoryAssigner : MonoBehaviour
    {
        [Inject]
        private IPublisher<WeaverAssembler?> _assemblerPublisher = null!;

        [SerializeField]
        private string _repositoryPath = string.Empty;

        private string _valueLastFrame = string.Empty;

        private WeaverAssembler? _lastAssembler;
        
        private void Update()
        {
            if (_valueLastFrame == _repositoryPath)
                return;

            _valueLastFrame = _repositoryPath;

            _ = UniTask.RunOnThreadPool(async () =>
            {
                // Build the weaver assembler on a separate thread.
                // It can take quite some time for it to build the mappings for every object.
                // I might make the snapshots lazy loaded in the future.
                var assembler = Directory.Exists(_repositoryPath) ? new WeaverAssembler(_repositoryPath) : null;
                _lastAssembler = assembler;

                await UniTask.SwitchToMainThread();
                _assemblerPublisher.Publish(assembler);
            });
        }
    }
}