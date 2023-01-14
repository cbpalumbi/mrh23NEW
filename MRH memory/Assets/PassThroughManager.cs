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
        Interop.WVR_ShowPassthroughUnderlay(true); //Show Passthrough Underlay
    }

    void HidePassthroughOverlay()
    {
        Interop.WVR_ShowPassthroughUnderlay(false); //Hide Passthrough Underlay
    }
}
