using System;
using System.Collections;
using UnityEngine;
using Microphone = Estrada.Microphone;

namespace Recognissimo.Components
{
    /// <summary>
    ///     <see cref="SpeechSource" /> that provides audio data from a microphone.
    /// </summary>
    [AddComponentMenu("Recognissimo/Speech Sources/Microphone Speech Source")]
    public sealed class MicrophoneSpeechSource : SpeechSource
    {
        private const int DefaultMaxRecordingTime = 2;

        [SerializeField]
        private RecordingSettings recordingSettings = new()
        {
            deviceName = null,
            timeSensitivity = 0.25f
        };

        private float[] _buffer;

        private int _prevPos;

        private int _recordingLength;

        /// <summary>
        ///     Microphone name. Use null or empty string to use default microphone.
        /// </summary>
        public string DeviceName
        {
            get => recordingSettings.deviceName;
            set => recordingSettings.deviceName = value;
        }

        /// <summary>
        ///     How often audio frames should be submitted to the recognizer (seconds).
        ///     Use smaller values to submit audio samples more often.
        ///     Recommended value is 0.25 seconds.
        /// </summary>
        public float TimeSensitivity
        {
            get => recordingSettings.timeSensitivity;
            set => recordingSettings.timeSensitivity = value;
        }

        /// <summary>
        ///     Whether recording is active.
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        ///     Whether recording is paused.
        /// </summary>
        public bool IsPaused { get; set; }

        private void Update()
        {
            if (!IsRecording)
            {
                return;
            }

            if (!Microphone.IsRecording(recordingSettings.deviceName))
            {
                OnRuntimeFailure(
                    new RuntimeFailureEventArgs(new SpeechSourceRuntimeException("Cannot access microphone")));
                return;
            }

            var currPos = Microphone.GetPosition(recordingSettings.deviceName);

            if (IsPaused)
            {
                return;
            }

            var bufferLength = _buffer.Length;

            var availableSamples = (currPos - _prevPos + _recordingLength) % _recordingLength;

            while (availableSamples >= bufferLength)
            {
                if (!Microphone.GetCurrentData(_buffer, _prevPos))
                {
                    OnRuntimeFailure(
                        new RuntimeFailureEventArgs(new SpeechSourceRuntimeException("Cannot access microphone data")));
                    return;
                }

                OnSamplesReady(new SamplesReadyEventArgs(_buffer, bufferLength));

                _prevPos = (_prevPos + bufferLength) % _recordingLength;

                availableSamples -= bufferLength;
            }
        }

        private void OnEnable()
        {
            if (Microphone.RequiresPermission())
            {
                RegisterInitializationTask("Check microphone permissions", CheckMicrophonePermissions,
                    CallCondition.Always);
            }

            RegisterInitializationTask("Detect sample rate", DetectSampleRate,
                CallCondition.ValueChanged(() => DeviceName));

            RegisterInitializationTask("Initialize microphone", InitializeMicrophone,
                CallCondition.Always);
        }

        private IEnumerator CheckMicrophonePermissions()
        {
            yield return Microphone.RequestPermission();

            if (!Microphone.HasPermission())
            {
                FailInitialization(new InvalidOperationException("Permission to use a microphone is denied"));
            }
        }

        private void DetectSampleRate()
        {
            const int minSupportedSampleRate = 16000;
            const int targetSampleRate = 16000;

            Microphone.GetDeviceCaps(DeviceName, out var minFreq, out var maxFreq);

            var supportsAnySampleRate = minFreq == 0 && maxFreq == 0;

            if (supportsAnySampleRate)
            {
                SampleRate = targetSampleRate;
                return;
            }

            if (minFreq < minSupportedSampleRate && maxFreq < minSupportedSampleRate)
            {
                FailInitialization(
                    new InvalidOperationException(
                        "Available sample rates are less than the minimum supported 16000 Hz"));
                return;
            }

            SampleRate = minFreq <= targetSampleRate && targetSampleRate <= maxFreq
                ? targetSampleRate
                : minFreq;
        }

        private IEnumerator InitializeMicrophone()
        {
            if (TimeSensitivity == 0f)
            {
                throw new InvalidOperationException($"{nameof(TimeSensitivity)} must be greater than zero");
            }

            const int secondsReserve = 1;

            var maxRecordingTime = (int) Math.Max(TimeSensitivity + secondsReserve, DefaultMaxRecordingTime);

            var clip = Microphone.Start(DeviceName, true, maxRecordingTime,
                SampleRate);

            _recordingLength = clip.samples;

            var bufferLength = (int) (TimeSensitivity * SampleRate);

            if (_buffer == null || _buffer.Length != bufferLength)
            {
                _buffer = new float[bufferLength];
            }

            while (Microphone.GetPosition(recordingSettings.deviceName) == 0)
            {
                yield return null;
            }
        }

        /// <inheritdoc />
        public override void StartProducing()
        {
            IsRecording = true;

            _prevPos = Microphone.GetPosition(recordingSettings.deviceName);
        }

        /// <inheritdoc />
        public override void StopProducing()
        {
            IsRecording = false;

            Microphone.End(recordingSettings.deviceName);
        }

        /// <summary>
        ///     Lists available microphone names.
        /// </summary>
        /// <returns>Available devices.</returns>
        public string[] Devices()
        {
            return Microphone.devices;
        }

        [Serializable]
        private struct RecordingSettings
        {
            [Tooltip("Microphone name. Leave empty to use default microphone.")]
            public string deviceName;

            [Tooltip("How often audio frames should be submitted to the recognizer (seconds). " +
                     "Use smaller values to submit audio samples more often. ")]
            public float timeSensitivity;
        }
    }
}