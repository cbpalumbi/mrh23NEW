using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

public class NewUIManager : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject instructions;
    //public GameObject cube;

    void Start() {
        startScreen.SetActive(true);
        instructions.SetActive(false);
    }

    void Update() {
        
        if(Interop.WVR_GetInputTouchState(WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_InputId.WVR_InputId_Alias1_A)) {
            ReShowInstructions();
        }

        if(Interop.WVR_GetInputTouchState(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_A)) {
            ReShowInstructions();
        }

        if (Input.GetKey(KeyCode.M)) {
            ReShowInstructions();
        }
        
    }

    // void Update() {
    //     if (Input.GetKey(KeyCode.M)) {
    //         OnContinueButtonClicked();
    //     }
    // }

    public void OnContinueButtonClicked() {
        startScreen.SetActive(false);
        instructions.SetActive(true);
    }

    public void OnCloseInstructionsButtonClicked() {
        instructions.SetActive(false);
    }

    void ReShowInstructions() {
        instructions.SetActive(true);
    }
}
