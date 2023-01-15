using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MemorySculpture {
    public class VfxInterface : MonoBehaviour
    {
        private Texture2D tex0;
        private Texture2D tex1;
        
        // Start is called before the first frame update
        void Start()
        {
            tex0 = new Texture2D(128, 128);
            tex1 = new Texture2D(128, 128);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void PopulateTextures() {

        }

        // public void AddNewMemory(string text, )
    }
}

