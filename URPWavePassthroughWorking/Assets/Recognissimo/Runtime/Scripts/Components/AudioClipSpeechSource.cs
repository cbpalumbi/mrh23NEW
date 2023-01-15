using System;
using System.Collections;
using UnityEngine;

namespace Recognissimo.Components
{
    /// <summary>
    ///     <see cref="SpeechSource" /> that provides audio data from an AudioClip.
    /// </summary>
    [AddComponentMenu("Recognissimo/Speech Sources/AudioClip Speech Source")]
    public sealed class AudioClipSpeechSource : SpeechSource
    {
        private const int DefaultBufferSize = 8192;

        /// <summary>
        ///     Audio clip from which the data will be taken.
        /// </summary>
        [Tooltip("Audio clip from which the data will be taken")]
        public AudioClip clip;

        /// <summary>
        ///     Whether to read the <see cref="clip" /> in parts.
        ///     Setting false will require buffer reallocation for each new clip.
        /// </summary>
        [Tooltip("Whether to read the clip in parts")]
        public bool readSequentially;

        private float[] _buffer;

        private bool _isProducing;

        private Coroutine _producingCoroutine;

        /// <inheritdoc />
        public override int SampleRate => clip.frequency;

        private void OnEnable()
        {
            RegisterInitializationTask("Load audio clip", LoadAudio, CallCondition.ValueChanged(() => clip));
        }

        /// <inheritdoc />
        public override void StartProducing()
        {
            _isProducing = true;

            if (readSequentially)
            {
                StartCoroutine(ReadAudioSequentially());
            }
            else
            {
                ReadAudio();
            }
        }

        /// <inheritdoc />
        public override void StopProducing()
        {
            _isProducing = false;

            if (_producingCoroutine != null)
            {
                StopCoroutine(_producingCoroutine);
            }
        }

        private IEnumerator LoadAudio()
        {
            clip.LoadAudioData();

            while (clip.loadState == AudioDataLoadState.Loading)
            {
                yield return null;
            }

            if (clip.channels > 1)
            {
                throw new NotSupportedException("Reading non-mono AudioClip is not supported yet.");
            }

            if (readSequentially)
            {
                _buffer ??= new float[DefaultBufferSize];
                yield break;
            }

            if (_buffer == null || _buffer.Length != clip.samples)
            {
                _buffer = new float[clip.samples];
            }
        }

        private void ReadAudio()
        {
            clip.GetData(_buffer, 0);

            OnSamplesReady(new SamplesReadyEventArgs(_buffer, _buffer.Length));

            OnDried();
        }

        private IEnumerator ReadAudioSequentially()
        {
            var samplesLeft = clip.samples;

            var processedSamples = 0;

            while (_isProducing && samplesLeft > 0)
            {
                var requestedSamples = Math.Min(samplesLeft, _buffer.Length);

                clip.GetData(_buffer, processedSamples);

                OnSamplesReady(new SamplesReadyEventArgs(_buffer, requestedSamples));

                samplesLeft -= requestedSamples;
                processedSamples += requestedSamples;

                yield return null;
            }

            if (_isProducing)
            {
                OnDried();
            }
        }
    }
}