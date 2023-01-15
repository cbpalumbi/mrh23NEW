using Recognissimo.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Recognissimo.Samples.VoiceActivityDetectorExample
{
    [AddComponentMenu("")]
    public class VoiceActivityDetectorExample : MonoBehaviour
    {
        [SerializeField]
        private VoiceActivityDetector activityDetector;

        [SerializeField]
        private Text status;

        private void Start()
        {
            activityDetector.TimeoutMs = 0;
            activityDetector.Spoke.AddListener(() => status.text = "<color=green>Speech</color>");
            activityDetector.Silenced.AddListener(() => status.text = "<color=red>Silence</color>");
            activityDetector.InitializationFailed.AddListener(e => status.text = e.Message);
            activityDetector.StartProcessing();
        }
    }
}