using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;
using Wave.Essence.Hand;
using Wave.Essence.Hand.StaticGesture;
using TMPro;

public class NewUIManager : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject instructions;
    public CustomGestureProvider gestureProvider;
    //public GameObject cube;
    public TextMeshProUGUI gestureText;

    void Start() {
        startScreen.SetActive(true);
        instructions.SetActive(false);
        gestureText.text = "None";
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

        // if (Interop.WVR_GetHandGestureData() == GestureType.Fist 
        //     ||  Interop.WVR_GetHandGestureData() == GestureType.Fist) {
        //     ReShowInstructions();
        // }

        if (gestureProvider != null) {
            if (gestureProvider.GetHandGesture(false) == GestureType.Fist) {
                gestureText.text = "Fist";
            }
            if (gestureProvider.GetHandGesture(false) == GestureType.Five) {
                gestureText.text = "Five";
            }
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
