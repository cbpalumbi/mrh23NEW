using System;
using Recognissimo.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Recognissimo.Components
{
    /// <summary>
    ///     <see cref="SpeechProcessor" /> for voice activity detection.
    /// </summary>
    [AddComponentMenu("Recognissimo/Speech Processors/Voice Activity Detector")]
    public sealed class VoiceActivityDetector : SpeechProcessor
    {
        [SerializeField]
        private VoiceActivityDetectorSettings settings = new()
        {
            spoke = new UnityEvent(),
            silenced = new UnityEvent()
        };

        /// <summary>
        ///     The number of milliseconds of silence after which the corresponding event should be triggered.
        /// </summary>
        public int TimeoutMs
        {
            get => settings.timeoutMs;
            set => settings.timeoutMs = value;
        }

        /// <summary>
        ///     Voice became active.
        /// </summary>
        public UnityEvent Spoke => settings.spoke;

        /// <summary>
        ///     Voice became inactive.
        /// </summary>
        public UnityEvent Silenced => settings.silenced;

        private void OnResult(VoiceActivityDetectorAlgorithm.Result result)
        {
            var currentEvent = result.IsActive ? Spoke : Silenced;
            currentEvent.Invoke();
        }

        private VoiceActivityDetectorAlgorithm.Settings OnSetup()
        {
            return new VoiceActivityDetectorAlgorithm.Settings
            {
                TimeoutMs = TimeoutMs
            };
        }

        internal override Algorithm CreateAlgorithm()
        {
            return new VoiceActivityDetectorAlgorithm(OnSetup, OnResult);
        }

        [Serializable]
        private struct VoiceActivityDetectorSettings
        {
            [Tooltip("The number of milliseconds of silence after which the corresponding event should be triggered")]
            public int timeoutMs;

            [Tooltip("Voice became active")]
            public UnityEvent spoke;

            [Tooltip("Voice became inactive")]
            public UnityEvent silenced;
        }
    }
}