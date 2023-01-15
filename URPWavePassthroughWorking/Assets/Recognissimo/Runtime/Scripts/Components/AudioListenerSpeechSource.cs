using System;
using UnityEngine;

namespace Recognissimo.Components
{
    /// <summary>
    ///     <see cref="SpeechSource" /> that provides Unity AudioListener audio data.
    /// </summary>
    [AddComponentMenu("Recognissimo/Speech Sources/AudioListener Speech Source")]
    public sealed class AudioListenerSpeechSource : SpeechSource
    {
        private const float TimeSensitivity = 0.25f;
        private const int DefaultBufferSize = 8192;

        /// <summary>
        ///     AudioListener channel for receiving data.
        /// </summary>
        public int channel;

        private readonly float[] _buffer = new float[DefaultBufferSize];

        private bool _isProducing;
        private int _minSamples;
        private int _samplesLeft;

        /// <inheritdoc />
        public override int SampleRate => AudioSettings.outputSampleRate;

        private void Update()
        {
            if (!_isProducing)
            {
                return;
            }

            _samplesLeft += (int) Math.Floor(SampleRate * Time.deltaTime);

            if (_samplesLeft < _minSamples)
            {
                return;
            }

            var availableSamples = Math.Min(_samplesLeft, _buffer.Length);
            AudioListener.GetOutputData(_buffer, channel);

            OnSamplesReady(new SamplesReadyEventArgs(_buffer, availableSamples));
            _samplesLeft -= availableSamples;
        }

        /// <inheritdoc />
        public override void StartProducing()
        {
            _minSamples = (int) Math.Floor(SampleRate * TimeSensitivity);
            _isProducing = true;
        }

        /// <inheritdoc />
        public override void StopProducing()
        {
            _isProducing = false;
            _samplesLeft = 0;
        }
    }
}