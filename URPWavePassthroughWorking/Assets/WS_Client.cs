using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


// private class Responses
// {

// }

// string jsonData = "{\"DeptId\": 101, \"DepartmentName\": \"IT\"}";

// Department deptObj = JsonSerializer.Deserialize<Department>(jsonData);

// Console.WriteLine("Department Id: {0}", deptObj.DeptId);
// Console.WriteLine("Department Name: {0}", deptObj.DepartmentName);

[Serializable]
public class Scores {
    public float Angry { get; set; }
    public float Fear { get; set; }
    public float Happy { get; set; }
    public float Sad { get; set; }
    public float Surprise { get; set; }
}

[Serializable]
public class Response {
    public string message { get; set; }
    public Scores scores { get; set; }
}

[Serializable]
public class Responses {
    public bool isInit { get; set; }
    public Response[] responses { get; set; }
}

public class WS_Client : MonoBehaviour
{
    // public event ReceiveAction OnReceived;
    public string message = "";
    [SerializeField]
    private string wsServerUrl = "";
    private ClientWebSocket webSocket = null;
    public GameObject speechToText;
    public bool sendData;
    private bool isInit = true;

    public TextMeshProUGUI debugTextBox;

    void Start() {
        Task connect = Connect(wsServerUrl);
    }

    public async Task Connect(string uri)
    {
        try
        {
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
            Debug.Log(webSocket.State);
            await Receive();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    private async Task Send(string message)
    {
        var encoded = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    void OnDestroy() {
        if (webSocket != null)
            webSocket.Dispose();
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Task send = Send(message);
        }
        if (sendData) {
		    Task send = Send(message);
            sendData = false;
        }
    }

    private async Task Receive()
    {

        ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);

        while (webSocket.State == WebSocketState.Open)
        {

            WebSocketReceiveResult result = null;

            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        string message = reader.ReadToEnd();
                        if (isInit) {
                            isInit = false;
                            Responses? responses = JsonUtility.FromJson<Responses>(message);
                            // Debug.Log("received initial responses: ");
                        } else {
                            Response? response = JsonUtility.FromJson<Response>(message);
                        }
                        Debug.Log(message);
                        // if (OnReceived != null) OnReceived(message);
                        debugTextBox.text = message;

                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }
    }
}
