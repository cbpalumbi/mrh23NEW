using System.Collections;
using UnityEngine;

namespace Estrada
{
    internal interface IMicrophoneController
    {
        bool RequiresPermission();

        IEnumerator RequestPermission();

        bool HasPermission();

        AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency);

        void End(string deviceName);

        bool IsRecording(string deviceName);

        int GetPosition(string deviceName);

        bool GetCurrentData(float[] data, int offsetSamples);

        void GetDeviceCaps(string deviceName, out int minFreq, out int maxFreq);

        string[] Devices();
    }
}