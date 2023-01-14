using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewUIManager : MonoBehaviour
{
    public GameObject startScreen;
    public GameObject instructions;

    void Start() {
        startScreen.SetActive(true);
        instructions.SetActive(false);
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
}
