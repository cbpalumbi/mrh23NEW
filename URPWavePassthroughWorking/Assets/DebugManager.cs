using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugManager : MonoBehaviour
{
    
    public TextMeshProUGUI memoryText1;
    public TextMeshProUGUI memoryText2;
    public TextMeshProUGUI memoryText3;

    WS_Client wsclient;

    void Start() {
        wsclient = GameObject.Find("WebSocketManager").GetComponent<WS_Client>();
        if (wsclient != null) {
            Debug.Log("Found a WS Client");
        } else {
            Debug.Log("Couldnt find ws client");
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            SendTestMsg1OnButtonClick();
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            SendTestMsg2OnButtonClick();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            SendTestMsg3OnButtonClick();
        }
    }

    public void SendTestMsg1OnButtonClick() {
        wsclient.SendMemoryMessage(memoryText1.text);
    }

    public void SendTestMsg2OnButtonClick() {
        wsclient.SendMemoryMessage(memoryText2.text);
    }

    public void SendTestMsg3OnButtonClick() {
        wsclient.SendMemoryMessage(memoryText3.text);
    }


    // public Image image;
    // public void ChangeColorOnButtonClick() {
    //     if (image.color == Color.red) {
    //         image.color = Color.white;
    //     } else {
    //         image.color = Color.red;
    //     }
    //     //image.color = Color.red;
    // }

    // void Update() {
    //     if (Input.GetKeyDown(KeyCode.T)) {
    //         ChangeColorOnButtonClick();
    //     }
    // }
}
