using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames;

namespace MemorySculpture {

    public class MemoryController : MonoBehaviour
    {
        public string message;
        // public 
        // Start is called before the first frame update

        public RectTransform textRectTransform;
        public RectTransform textParentRectTransform;
        public CurvyText curvyText;
        public float spinSpeed = 10.2f;
        public MeshRenderer renderer;

        private List<AudioSource> audioComponents = new List<AudioSource>();

        void Start()
        {
            var components = gameObject.GetComponents<AudioSource>();
            foreach (var comp in components) {
                audioComponents.Add(comp);
            }
        }

        // Update is called once per frame
        void Update()
        {
            textParentRectTransform.LookAt(Camera.main.transform);
            textRectTransform.rotation = Quaternion.Euler(0, 0, Time.time * spinSpeed);
        }

        public void SetParams(MemoryDatum datum) {
            message = datum.message;
            curvyText.text = message;
            float sum = datum.scores.Sad + datum.scores.Sad + datum.scores.Happy + datum.scores.Angry + datum.scores.Surprise;
            renderer.material.SetFloat("_purple", datum.scores.Sad * 3 + 0.3f * Random.Range(0, 1));
            renderer.material.SetFloat("_blue", datum.scores.Sad * 3 + 0.3f * Random.Range(0, 1));
            renderer.material.SetFloat("_yellow", datum.scores.Happy * 3 + 0.3f * Random.Range(0, 1));
            renderer.material.SetFloat("_red", datum.scores.Angry * 3 + 0.3f * Random.Range(0, 1));
            renderer.material.SetFloat("_pink", datum.scores.Surprise * 3 + 0.3f * Random.Range(0, 1));
            renderer.material.SetFloat("_scale", 4 + datum.scores.Angry + datum.scores.Happy - datum.scores.Sad);
            renderer.material.SetFloat("_saturation", sum * 3 + 0.3f * Random.Range(0, 1));
            renderer.material.SetFloat("_intensity", sum * 3 + 0.3f * Random.Range(0, 1));
        }

        public void PlayAudio() {

        }
    }
}

