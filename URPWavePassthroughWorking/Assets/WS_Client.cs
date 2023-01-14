using WebSocketSharp;
using UnityEngine;

public class WS_Client : MonoBehaviour
{
    [HideInInspector]
    public WebSocket ws;
    public DebugManager debug;

    void Start() {
        ws = new WebSocket("ws://localhost:8081");
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

    public void SendMemoryMessage(string text) {
        if (ws == null)
        {
            Debug.LogError("Tried to send a message but ws client is null!");
            return;
        }
        else 
        {
            ws.Send(text);
        }
    }
}
