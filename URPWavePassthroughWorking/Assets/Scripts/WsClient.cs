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
// using EasyButtons;

// List<string> optionList = new List<string>
//             { "AdditionalCardPersonAdressType",
//              /* rest of elements */ };


namespace MemorySculpture {

    [Serializable]
    public class Scores {
        public float Angry;
        public float Fear;
        public float Happy;
        public float Sad;
        public float Surprise;
    }

    [Serializable]
    public class MemoryDatum {
        public string message;
        public Scores scores;
    }

    [Serializable]
    public class MemoryData {
        public bool isInit;
        public MemoryDatum[] memories;
    }

    public class WsClient : MonoBehaviour
    {
        // public event ReceiveAction OnReceived;
        public string message = "";
        [SerializeField]
        private string wsServerUrl = "";
        private ClientWebSocket webSocket = null;
        public GameObject speechToText;
        public SculptureController sculptureController;
        public bool sendData;
        private bool isInit = true;

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
            Debug.Log("sending message: " + message);
            var encoded = Encoding.UTF8.GetBytes(message);
            // input message from result of SR
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
        }

        // [Button]
        public void SendData() {
            Task send = Send(message);
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
                            Debug.Log(message);
                            if (isInit) {
                                isInit = false;
                                MemoryData? memoryData = JsonUtility.FromJson<MemoryData>(message);
                                Debug.Log("Received initial memoryData: ");
                                Debug.Log(memoryData.isInit);
                                sculptureController.InitFromData(memoryData);
                            } else {
                                MemoryDatum? memoryDatum = JsonUtility.FromJson<MemoryDatum>(message);
                                sculptureController.AppendDatum(memoryDatum);
                            }
                            // if (OnReceived != null) OnReceived(message);

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

}

