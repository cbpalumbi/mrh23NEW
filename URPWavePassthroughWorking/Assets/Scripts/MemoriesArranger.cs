using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

namespace MemorySculpture {
    public class MemoriesArranger : MonoBehaviour
    {
        public float spinSpeed = 0.0f;
        public float bowMag = 0.0f;
        public float bowMin = 0.0f;
        public float stretchMag = 0.0f;
        public float spiralMag = 0.0f;
        public GameObject memoryPrefab;
        public int maxNumMems = 40;
        public int debugSpawnNum = 0;
        private List<GameObject> memoryList = new List<GameObject>();
        private Transform transform;
        // Start is called before the first frame update
        void Start()
        {
            transform = gameObject.GetComponent<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
            // transform.Rotate(spinSpeed * Time.deltaTime, 0, 0);
            transform.localRotation = Quaternion.Euler(0, Time.time * spinSpeed, 0);
            int i = 0;
            foreach (GameObject memory in memoryList) {
                // memory.GetComponent<Transform>().localPosition = GetPosition(i, memoryList.Count, Time.time * spinSpeed);
                i++;
            }
            
        }

        [Button]
        private void DebugSpawn() {
            foreach (GameObject memory in memoryList) {
                Destroy(memory);
            }
            memoryList.Clear();
            // InitFromList(debugSpawnNum);
        }

        private Vector3 GetPosition(int i, int numMems, float rotOffset) {
            float t = ((float) i) / ((float)numMems - 1.0f); // [0, 1]
            float theta = t * spiralMag; // + rotOffset;
            float bowAmount = bowMin + ((1.0f - Mathf.Pow(2.0f * t - 1.0f, 2.0f)) * 0.5f) * bowMag;
            float x = stretchMag * (2.0f * (t - 0.5f)); 
            float y = Mathf.Cos(theta) * bowAmount;
            float z = Mathf.Sin(theta) * bowAmount;
            return new Vector3(x, y, z);
        }

        // arrange memories in spiral pattern
        public void InitFromList(MemoryDatum[] memories) {
            int numMems = memories.Length;

            for (int i = 0; i < numMems; i++) {
                AddMemory(memories[i], GetPosition(i, numMems, 0.0f));
            }
        }

        public void AddMemory(MemoryDatum datum, Vector3 position) {
            GameObject newMemory = Instantiate(memoryPrefab, transform.position + position, Quaternion.identity);
            memoryList.Add(newMemory);
            newMemory.GetComponent<Transform>().SetParent(transform);
            newMemory.GetComponent<MemoryController>().SetParams(datum);
        }
    }
}

