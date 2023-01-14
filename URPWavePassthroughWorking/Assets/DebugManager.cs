using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    public Image image;
    public void ChangeColorOnButtonClick() {
        if (image.color == Color.red) {
            image.color = Color.white;
        } else {
            image.color = Color.red;
        }
        //image.color = Color.red;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            ChangeColorOnButtonClick();
        }
    }
}
