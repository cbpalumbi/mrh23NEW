using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryInteractionManager : MonoBehaviour
{
    [HideInInspector]
    public GameObject selectedMemory = null;
    float speed = 5f;
    public Material blue;

    // if there is no currently selected memory
        // on hover(item) and on point gesture (ui manager)
        // set that memory to selected memory
        // initiate draw towards

    // if there is a currently selected memory 
        // if fist gesture 
        // return memory to sculpture
        // set selected memory to none

    // memory interaction manager always tries to draw selected to itself

    void Update() {
        if (selectedMemory != null) { // TODO: add distance check
            
            var step = speed * Time.deltaTime; // calculate distance to move
            selectedMemory.transform.position = Vector3.MoveTowards(selectedMemory.transform.position, transform.position, step);


        }
    }

    
    
        
    

    

}
