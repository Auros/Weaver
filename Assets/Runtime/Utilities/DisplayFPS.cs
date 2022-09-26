using TMPro;
using UnityEngine;

namespace Weaver.Utilities
{
    public sealed class DisplayFPS : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text = null!;

        [SerializeField, Min(1)]
        private int _samplingRate = 10;

        private int _samples;
        private float _sampleFrameTime;

        private void Update()
        {
            _samples++;
            _sampleFrameTime += Time.deltaTime;

            // If the sample rate exceeds the number of samples we've already made,
            // we skip until we have enough samples.
            if (_samplingRate > _samples)
                return;

            var framerate = _samples / _sampleFrameTime;
            _text.text = $"{(int)framerate}";

            // Reset the values
            _samples = 0;
            _sampleFrameTime = 0;
        }
    }
}