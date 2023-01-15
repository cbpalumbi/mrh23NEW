#if !UNITY_WEBGL || UNITY_EDITOR // Prevent WebGL building fail
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Android;

namespace Estrada
{
    internal class DefaultMicrophoneController : IMicrophoneController
    {
        private readonly IMicrophonePermissionRequester _microphonePermissionRequester = Application.platform switch
        {
            RuntimePlatform.IPhonePlayer => new IOSMicrophonePermissionRequester(),
            RuntimePlatform.OSXPlayer => new MacOSMicrophonePermissionRequester(),
            RuntimePlatform.Android => new AndroidMicrophonePermissionRequester(),
            _ => null
        };

        private AudioClip _currentClip;

        public bool RequiresPermission()
        {
            return _microphonePermissionRequester != null;
        }

        public IEnumerator RequestPermission()
        {
            if (_microphonePermissionRequester != null)
            {
                yield return _microphonePermissionRequester.RequestPermission();
            }
        }

        public bool HasPermission()
        {
            return _microphonePermissionRequester == null || _microphonePermissionRequester.HasPermission();
        }

        public AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency)
        {
            _currentClip = UnityEngine.Microphone.Start(deviceName, loop, lengthSec, frequency);

            return _currentClip;
        }

        public void End(string deviceName)
        {
            UnityEngine.Microphone.End(deviceName);
        }

        public bool IsRecording(string deviceName)
        {
            return UnityEngine.Microphone.IsRecording(deviceName);
        }

        public int GetPosition(string deviceName)
        {
            return UnityEngine.Microphone.GetPosition(deviceName);
        }

        public bool GetCurrentData(float[] data, int offsetSamples)
        {
            return _currentClip && _currentClip.GetData(data, offsetSamples);
        }

        public void GetDeviceCaps(string deviceName, out int minFreq, out int maxFreq)
        {
            UnityEngine.Microphone.GetDeviceCaps(deviceName, out minFreq, out maxFreq);
        }

        public string[] Devices()
        {
            return UnityEngine.Microphone.devices;
        }

        private interface IMicrophonePermissionRequester
        {
            public IEnumerator RequestPermission();

            public bool HasPermission();
        }

        private class AndroidMicrophonePermissionRequester : IMicrophonePermissionRequester
        {
            private readonly PermissionCallbacks _permissionCallbacks = new();

            private bool _pending;

            public AndroidMicrophonePermissionRequester()
            {
                _permissionCallbacks.PermissionGranted += PermissionExecuted;
                _permissionCallbacks.PermissionDenied += PermissionExecuted;
                _permissionCallbacks.PermissionDeniedAndDontAskAgain += PermissionExecuted;
            }

            public IEnumerator RequestPermission()
            {
                _pending = true;

                Permission.RequestUserPermission(Permission.Microphone, _permissionCallbacks);

                yield return new WaitWhile(() => _pending);
            }

            public bool HasPermission()
            {
                return Permission.HasUserAuthorizedPermission(Permission.Microphone);
            }

            private void PermissionExecuted(string _)
            {
                _pending = false;
            }
        }

        private class IOSMicrophonePermissionRequester : IMicrophonePermissionRequester
        {
            public IEnumerator RequestPermission()
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }

            public bool HasPermission()
            {
                return Application.HasUserAuthorization(UserAuthorization.Microphone);
            }
        }

        private class MacOSMicrophonePermissionRequester : IMicrophonePermissionRequester
        {
            private static bool _status;
            private static bool _callbackFired;

            public IEnumerator RequestPermission()
            {
                _callbackFired = false;
#if UNITY_STANDALONE_OSX
                Estrada_RequestMacOSPermission(Callback);
#endif
                while (!_callbackFired)
                {
                    yield return null;
                }
            }

            public bool HasPermission()
            {
                return _status;
            }

            private static void Callback(bool result)
            {
                _callbackFired = true;
                _status = result;
            }

            private delegate void PermissionRequestCallback(bool result);
            
#if UNITY_STANDALONE_OSX
            [DllImport("Estrada")]
            private static extern void Estrada_RequestMacOSPermission(PermissionRequestCallback callback);
#endif
        }
    }
}
#endif