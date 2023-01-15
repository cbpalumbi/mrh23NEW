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
    [HideInInspector]
    GestureType heldGesture;
    GestureType holdingGesture;
    //bool isRunningCoroutine = false;

    void Start() {
        startScreen.SetActive(true);
        instructions.SetActive(false);
        gestureText.text = "None";
        holdingGesture = GestureType.Unknown; //the one you have now but haven't necesarily held for long enough
        heldGesture = GestureType.Unknown; // the previous gesture held for long enough
    }


    void Update() {
        
        if(Interop.WVR_GetInputTouchState(WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_InputId.WVR_InputId_Alias1_A)) {
            ReShowWelcomeScreen();
        }

        if(Interop.WVR_GetInputTouchState(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_A)) {
            ReShowWelcomeScreen();
        }

        if (Input.GetKey(KeyCode.M)) {
            ReShowInstructions();
        }

        if (gestureProvider != null) {

            if (gestureProvider.GetHandGesture(false) != holdingGesture) {
                StopCoroutine(ChangeGestureIfHeldForSeconds());
                holdingGesture = gestureProvider.GetHandGesture(false);
                StartCoroutine(ChangeGestureIfHeldForSeconds());
            }
            
        }

        if (heldGesture == GestureType.Like && !instructions.activeInHierarchy) {
            ReShowInstructions();
        }
        
    }

    IEnumerator ChangeGestureIfHeldForSeconds() {
        for (int i = 0; i < 10; i++) {
            yield return new WaitForSeconds(0.1f);
        }
        heldGesture = holdingGesture;
        gestureText.text = heldGesture.ToString();
    }

    public void OnContinueButtonClicked() {
        startScreen.SetActive(false);
        instructions.SetActive(true);
    }

    public void OnCloseInstructionsButtonClicked() {
        instructions.SetActive(false);
    }

    void ReShowInstructions() {
        startScreen.SetActive(false);
        instructions.SetActive(true);
    }

    void ReShowWelcomeScreen() {
        instructions.SetActive(false);
        startScreen.SetActive(true);
    }
}
