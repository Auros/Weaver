using System;
using MessagePipe;
using TMPro;
using UnityEngine;
using VContainer;
using Weaver.Models;

namespace Weaver.UI
{
    public class ProgressReporter : MonoBehaviour
    {
        [Inject]
        private readonly ISubscriber<string, float> _progressEvent = null!;

        [SerializeField]
        private TMP_Text _text = null!;

        private IDisposable? _subscription;
        
        private void Start()
        {
            _subscription = _progressEvent.Subscribe(WeaverEventKeys.LoadingProgress, ProgressChanged);
        }

        private void ProgressChanged(float value)
        {
            if (Mathf.Approximately(value, 1f))
            {
                // Done!
                _text.text = string.Empty;
            }
            else
            {
                _text.text = value.ToString("P1");
            }
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }
    }
}