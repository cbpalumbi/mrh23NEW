using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MemorySculpture {

    public class SculptureController : MonoBehaviour
    {
        public MemoriesArranger memoriesArranger;
        public VfxInterface vfxInterface;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void InitFromData(MemoryData memoryData) {
            memoriesArranger.InitFromList(memoryData.memories);
        }

        public void AppendDatum(MemoryDatum memoryDatum) {

        }
    }
}

