using WebSocketSharp;
using UnityEngine;

public class WS_Client : MonoBehaviour
{
    WebSocket ws;

    void Start() {
        ws = new WebSocket("ws://localhost:8080");
        ws.OnMessage += (sender, e) => 
        {
            Debug.Log("Message received from " + ((WebSocket)sender).Url + ", Data: " + e.Data);
        };
        ws.Connect();
    }

    void Update() 
    {
        if (ws == null)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ws.Send("Sunflowers are happy. Dogs are great.");
        }
    }
}
