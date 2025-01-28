using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class Connection
{
    private WebSocket websocket = new WebSocket("ws://127.0.0.1:8080");
    public event Action<ClientboundPacket> OnPacketReceived;
    public bool IsConnected => websocket?.State == WebSocketState.Open;

    public Connection()
    {
        websocket.OnMessage += HandleMessage;
    }

    public async void Connect()
    {
        await websocket.Connect();
    }

    private void HandleMessage(byte[] data)
    {
        var packet = ClientboundPacket.Deserialize(data);
        if (packet != null)
        {
            OnPacketReceived?.Invoke(packet);
        }
    }

    public async void SendPacket(ServerboundPacket packet)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.Send(packet.Serialize());
        }
    }

    private void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    }

    private async void OnDestroy()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
