#define ESTRADA_WEBGL_ALLOW_SAMPLE_RATE_MISMATCH

#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Estrada
{
    internal class WebGLMicrophoneController : IMicrophoneController
    {
        private const float TargetLatency = 0.05f;
        private const float TargetFrameLength = 0.1f;

        private static WebGLMicrophoneController _instance;

        private string[] _availableDevices;

        private AudioClip _currentClip;

        private CurrentDevice? _currentDevice;

        private Permission _permission = Permission.NotRequested;

        private readonly int _minSampleRate;
        private readonly int _maxSampleRate;

        private int _read;
        private bool _loop;

        private float[] _buffer;

        private static bool DetectAudioClipLimitations(out int supportedSampleRate)
        {
            const int testSampleRate = 16000;
                
            var clip = AudioClip.Create("__Estrada_Dummy", 1, 1, testSampleRate, false);

            supportedSampleRate = clip.frequency;
            
            return clip.frequency != testSampleRate;
        }
        
        public WebGLMicrophoneController()
        {
            _instance = this;

            PInvoke.Estrada_Initialize(TargetLatency, TargetFrameLength);

            _minSampleRate = PInvoke.Estrada_MinSampleRate();
            _maxSampleRate = PInvoke.Estrada_MaxSampleRate();
            
#if !ESTRADA_WEBGL_ALLOW_SAMPLE_RATE_MISMATCH
            if (DetectAudioClipLimitations(out var availableSampleRate))
            {
                if (availableSampleRate < _minSampleRate || _maxSampleRate < availableSampleRate)
                {
                    var message = $"Unity only supports a sampling rate of {availableSampleRate} Hz, while your browser supports ";

                    message += _minSampleRate == _maxSampleRate
                        ? $"{_minSampleRate} Hz."
                        : $"sampling rates in the [{_minSampleRate} {_maxSampleRate}] Hz range";
                    
                    Debug.LogError(message);
                    
                    throw new NotSupportedException(message);
                }

                _minSampleRate = availableSampleRate;
                _maxSampleRate = availableSampleRate;
            }
#endif

            PInvoke.Estrada_InstallCallbacks(HandlePermissionGranted, HandlePermissionDenied, HandleDeviceChange,
                HandleTrackEnded, HandleNewSamples);
        }
        
        public bool RequiresPermission()
        {
            return true;
        }

        public IEnumerator RequestPermission()
        {
            if (_permission == Permission.Granted)
            {
                yield break;
            }

            _permission = Permission.NotRequested;

            PInvoke.Estrada_RequestPermission();

            yield return new WaitWhile(() => _permission == Permission.NotRequested);
        }

        public bool HasPermission()
        {
            return _permission == Permission.Granted;
        }

        public AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency)
        {
            var isDefault = string.IsNullOrEmpty(deviceName);

            if (isDefault)
            {
                deviceName = _availableDevices[0];
            }

            _loop = loop;

            PInvoke.Estrada_Start(deviceName, frequency, lengthSec);

            _read = 0;

            var recommendedBufferLength = lengthSec * frequency;

            _currentClip = AudioClip.Create("Microphone", recommendedBufferLength, 1, frequency, false);
            
            var actualBufferLength = _currentClip.samples;

            if (_buffer == null || _buffer.Length != actualBufferLength)
            { 
                _buffer = new float[actualBufferLength];
            }
            else
            {
                Array.Fill(_buffer, 0.0f);
            }

            _currentDevice = new CurrentDevice
            {
                IsDefault = isDefault,
                Name = deviceName
            };

            return _currentClip;
        }

        public void End(string deviceName)
        {
            if (!IsRecording(deviceName))
            {
                return;
            }

            PInvoke.Estrada_End();

            Cleanup();
        }

        public bool IsRecording(string deviceName)
        {
            return _currentDevice.HasValue && _currentDevice.Value.Equals(deviceName);
        }

        public int GetPosition(string deviceName)
        {
            return IsRecording(deviceName) ? _read : 0;
        }

        public bool GetCurrentData(float[] data, int offsetSamples)
        {
            if (!_currentClip)
            {
                return false;
            }

            var samplesToWrite = data.Length;

            if (samplesToWrite == 0)
            {
                return true;
            }

            var written = 0;

            while (samplesToWrite > 0)
            {
                var read = (offsetSamples + written) % _buffer.Length;
                var samplesInBounds = _buffer.Length - read;
                var canWrite = Math.Min(samplesToWrite, samplesInBounds);

                Array.Copy(_buffer, read,
                    data, written, canWrite);

                samplesToWrite -= canWrite;
                written += canWrite;
            }

            return true;
        }

        public void GetDeviceCaps(string deviceName, out int minFreq, out int maxFreq)
        {
            minFreq = _minSampleRate;
            maxFreq = _maxSampleRate;
        }

        public string[] Devices()
        {
            if (_permission != Permission.Granted)
            {
                Debug.LogError("Microphone permission is required to view available devices");
            }

            return _availableDevices;
        }

        [MonoPInvokeCallback(typeof(NativeCallback))]
        private static void HandlePermissionGranted()
        {
            _instance._permission = Permission.Granted;
        }

        [MonoPInvokeCallback(typeof(NativeCallbackString))]
        private static void HandlePermissionDenied(string error)
        {
            _instance._permission = Permission.Denied;
        }

        [MonoPInvokeCallback(typeof(NativeCallback))]
        private static void HandleDeviceChange()
        {
            _instance.ReadAvailableDevices();
        }

        [MonoPInvokeCallback(typeof(NativeCallback))]
        private static void HandleTrackEnded(string deviceName)
        {
            _instance.Cleanup();
        }

        [MonoPInvokeCallback(typeof(NativeCallback))]
        private static void HandleNewSamples()
        {
            _instance.ReadSamples();

            _instance._currentClip.SetData(_instance._buffer, 0);
        }

        private void Cleanup()
        {
            _currentDevice = null;
        }

        private void ReadSamples()
        {
            var written = PInvoke.Estrada_WriteSamples(_buffer, _buffer.Length, _read, _loop);

            _read += written;

            if (!_loop && _read == _buffer.Length)
            {
                _read = 0;

                if (_currentDevice != null)
                {
                    End(_currentDevice.Value.Name);
                }

                return;
            }

            _read %= _buffer.Length;
        }

        private void ReadAvailableDevices()
        {
            _availableDevices = new string[PInvoke.Estrada_AvailableDevicesNum()];

            for (var i = 0; i < _availableDevices.Length; i++)
            {
                _availableDevices[i] = PInvoke.Estrada_DeviceNameAt(i);
            }
        }

        private struct CurrentDevice
        {
            public bool IsDefault;
            public string Name;

            public bool Equals(string deviceName)
            {
                if (IsDefault && string.IsNullOrEmpty(deviceName))
                {
                    return true;
                }

                return Name == deviceName;
            }
        }

        private enum Permission
        {
            NotRequested,
            Granted,
            Denied
        }

        private delegate void NativeCallback();

        private delegate void NativeCallbackString(string message);

        private static class PInvoke
        {
            private const string LibName = "__Internal";

            [DllImport(LibName)]
            public static extern void Estrada_Initialize(float targetLatency, float targetFrameLength);

            [DllImport(LibName)]
            public static extern int Estrada_MinSampleRate();

            [DllImport(LibName)]
            public static extern int Estrada_MaxSampleRate();

            [DllImport(LibName)]
            public static extern void Estrada_InstallCallbacks(NativeCallback permissionGranted,
                NativeCallbackString permissionDenied, NativeCallback deviceChange, NativeCallbackString trackEnded,
                NativeCallback newSamples);

            [DllImport(LibName)]
            public static extern void Estrada_RequestPermission();

            [DllImport(LibName)]
            public static extern int Estrada_WriteSamples(float[] sharedArray, int length, int from, bool allowOverflow);

            [DllImport(LibName)]
            public static extern void Estrada_Start(string deviceName, int frequency, int maxDuration);

            [DllImport(LibName)]
            public static extern void Estrada_End();

            [DllImport(LibName)]
            public static extern int Estrada_AvailableDevicesNum();

            [DllImport(LibName)]
            public static extern string Estrada_DeviceNameAt(int index);
        }
    }
}
#endif