using System.IO;
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

        private void Update()
        {
            if (_valueLastFrame == _repositoryPath)
                return;

            _valueLastFrame = _repositoryPath;

            _assemblerPublisher.Publish(Directory.Exists(_repositoryPath) ? new WeaverAssembler(_repositoryPath) : null);
        }
    }
}