using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CircleVRClient : CircleVRProtocolBase
{
    private string serverIP;
    private int serverPort;

    private string content;

    private bool connecting;
    private bool connected;

    private int connectionId;
    
    private bool initFinished;

    public override void Init(Configuration config)
    { 
        CreateHost(out hostID, out reliableChannel, out unreliableChannel, 1);

        serverIP = config.serverIp;
        serverPort = config.serverPort;

        content = CircleVR.Instance.ContentName;
        
        if(Delegate != null)
            Delegate.OnClientInit();

        initFinished = true;
    }
    
    private void Connect()
    {
        if (!initFinished)
            return;
    
        if (connecting)
            return;
        
        byte error;

        connectionId = NetworkTransport.Connect(hostID, serverIP, serverPort, 0, out error);

        if (error != 0)
        {
            Debug.LogError(error);
            return;
        }

        connecting = true;
    }

    public override void ManualUpdate()
    {
        if (!initFinished)
            return;

        if (!connected)
            Connect();

        base.ManualUpdate();
    }

    protected override void OnDisconnect(int hostId, int connectionId, byte error)
    {
        base.OnDisconnect(hostId, connectionId, error);
        
        connected = false;
        connecting = false;
    }

    protected override void OnConnect(int hostId, int connectionId, byte error)
    {
        base.OnConnect(hostId, connectionId, error);
        SendClientData();
    }

    protected override void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        base.OnData(hostId, connectionId, channelId, data, size, error);

        string deserializedData = Deserialize(data, size);

        if (!connected)
        {
            if (deserializedData == "Connected")
            {
                connected = true;
                connecting = false;
                return;
            }
            if (deserializedData == "Failed")
            {
                connecting = false;
                return;
            }
        }
    }
   
    private void SendClientData()
    {
        ClientData data = new ClientData(content);
        string json = JsonUtility.ToJson(data);
        SendData(hostID, json, connectionId, reliableChannel);
    }
}
