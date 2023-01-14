using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

public class PassThroughManager : MonoBehaviour
{
    //Passthrough Overlay Sample Code

    void Start() {
        ShowPassthroughOverlay();
    }

    private void OnApplicationQuit() {
        HidePassthroughOverlay();
    }
    
    void ShowPassthroughOverlay()
    {
        //Interop.WVR_ShowPassthroughOverlay(true); 
        Interop.WVR_ShowPassthroughUnderlay(true); //Show Passthrough Underlay
        Debug.Log("triggering passthrough");
    }

    void HidePassthroughOverlay()
    {
        //Interop.WVR_ShowPassthroughOverlay(false); 
        Interop.WVR_ShowPassthroughUnderlay(false); //Hide Passthrough Underlay
    }
}
