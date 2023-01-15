using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Estrada
{
    public static class Microphone
    {
        private static readonly IMicrophoneController MicrophoneController = CreateMicrophoneController();

        /// <summary>
        ///     Returns names of available devices.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static string[] devices => MicrophoneController.Devices();

        private static IMicrophoneController CreateMicrophoneController()
        {
            return
#if !UNITY_WEBGL || UNITY_EDITOR
                new DefaultMicrophoneController();
#else
                new WebGLMicrophoneController();
#endif
        }

        /// <summary>
        ///     Whether a microphone permission is required
        /// </summary>
        /// <returns>True if required.</returns>
        public static bool RequiresPermission()
        {
            return MicrophoneController.RequiresPermission();
        }

        /// <summary>
        ///     Request permission to record microphone.
        /// </summary>
        /// <returns>Enumerator to run coroutine on.</returns>
        public static IEnumerator RequestPermission()
        {
            yield return MicrophoneController.RequestPermission();
        }

        /// <summary>
        ///     Whether a permission to record microphone has been granted.
        /// </summary>
        /// <returns>Microphone access state.</returns>
        public static bool HasPermission()
        {
            return MicrophoneController.HasPermission();
        }

        /// <summary>
        ///     Start recording.
        /// </summary>
        /// <param name="deviceName">Name of the device to be recorded. Null or empty string means default device.</param>
        /// <param name="loop">
        ///     Indicates whether the recording should continue recording if lengthSec is reached, and wrap around
        ///     and record from the beginning of the AudioClip.
        /// </param>
        /// <param name="lengthSec">Length of the recording before overlap.</param>
        /// <param name="frequency">The sample rate of the AudioClip produced by the recording.</param>
        /// <returns>Returns AudioClip or null if the recording fails to start.</returns>
        /// <exception cref="InvalidOperationException">If permission to record was not granted or no devices available.</exception>
        /// <exception cref="ArgumentException">
        ///     <list type="bullet">
        ///         <item>If <paramref name="deviceName" /> is not in the <see cref="devices" />.</item>
        ///         <item>If <paramref name="lengthSec" /> is negative or is greater than hour.</item>
        ///         <item>If <paramref name="frequency" /> is negative.</item>
        ///     </list>
        /// </exception>
        public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency)
        {
            if (RequiresPermission() && !HasPermission())
            {
                throw new InvalidOperationException("Permission to record a microphone was not granted");
            }

            if (devices.Length == 0)
            {
                throw new InvalidOperationException("No available devices");
            }

            if (!string.IsNullOrEmpty(deviceName) && !devices.Contains(deviceName))
            {
                throw new ArgumentException($"Could not find device {deviceName} in the list of the available devices");
            }

            switch (lengthSec)
            {
                case <= 0:
                    throw new ArgumentException(
                        $"Length of recording must be greater than zero seconds (was: {lengthSec} seconds)");
                case > 3600:
                    throw new ArgumentException(
                        $"Length of recording must be less than one hour (was: {lengthSec} seconds)");
            }

            if (frequency <= 0)
            {
                throw new ArgumentException($"Frequency of recording must be greater than zero (was: {frequency} Hz)");
            }

            return MicrophoneController.Start(deviceName, loop, lengthSec, frequency);
        }

        /// <summary>
        ///     Stops recording
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        public static void End(string deviceName)
        {
            MicrophoneController.End(deviceName);
        }

        /// <summary>
        ///     Whether recording is active.
        /// </summary>
        /// <returns>Recording state.</returns>
        public static bool IsRecording(string deviceName)
        {
            return MicrophoneController.IsRecording(deviceName);
        }

        /// <summary>
        ///     Fills an array with sample data from the clip.
        ///     If not recording, samples from previous clip will be used.
        /// </summary>
        /// <param name="data">Array to fill. </param>
        /// <param name="offsetSamples">Internal buffer offset. </param>
        /// <returns>Whether the microphone data was accessed. </returns>
        public static bool GetCurrentData(float[] data, int offsetSamples)
        {
            return MicrophoneController.GetCurrentData(data, offsetSamples);
        }

        /// <summary>
        ///     Get current position of the recording.
        /// </summary>
        /// <returns>Current recording position in samples.</returns>
        public static int GetPosition(string deviceName)
        {
            return MicrophoneController.GetPosition(deviceName);
        }

        /// <summary>
        ///     Get the frequency capabilities of a device.
        /// </summary>
        /// <param name="deviceName">Device name. Null or empty string means default device. </param>
        /// <param name="minFreq">Minimal supported sample rate. </param>
        /// <param name="maxFreq">Maximal supported sample rate. </param>
        public static void GetDeviceCaps(string deviceName, out int minFreq, out int maxFreq)
        {
            MicrophoneController.GetDeviceCaps(deviceName, out minFreq, out maxFreq);
        }
    }
}