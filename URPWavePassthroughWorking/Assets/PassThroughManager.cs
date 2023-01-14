using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Wave.Native;
using Wave.Essence.Extra;


public class PassThroughManager : MonoBehaviour
{
    // public Material alphaOverlayMat;
    //Passthrough Overlay Sample Code

    // Blit(source, dest, mat);

    void Start() {
        ShowPassthroughOverlay();
    }

    private void OnApplicationQuit() {
        HidePassthroughOverlay();
    }
    
    //WVR_Result.WVR_SUCCESS: Parameter is valid.
    //WVR_Result.WVR_Error_RuntimeVersionNotSupport: Passthrough Underlay is not supported by the runtime on the device.
    //WVR_Result.WVR_Error_FeatureNotSupport:
    void ShowPassthroughOverlay()
    {
        WVR_Result result = Interop.WVR_ShowPassthroughUnderlay(true); //Show Passthrough Underlay
        // if (result == WVR_Result.WVR_SUCCESS) {
        //     Debug.Log("Underlay Success!");
        // } else if (result == WVR_Result.WVR_Error_RuntimeVersionNotSupport) {
        //     Debug.Log("Underlay version not supported?");
        // } else {
        //     Debug.Log("Underlay feature not supported?");
        // }
        // Interop.WVR_ShowPassthroughOverlay(true);
    }

    void HidePassthroughOverlay()
    {
        WVR_Result result = Interop.WVR_ShowPassthroughUnderlay(false); //Hide Passthrough Underlay
        // Interop.WVR_ShowPassthroughOverlay(false); //Hide Passthrough Underlay
    }
}
