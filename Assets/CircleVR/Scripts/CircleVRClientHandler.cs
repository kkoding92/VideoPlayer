using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CircleVRClientHandler : MonoBehaviour ,ICircleVRTransportEventHandler
{
    private CircleVRClient client;

    public abstract void OnEnter(ContentServerStatus status);

    public virtual void OnPlay()
    {
        Debug.Log("[INFO] Content Client Play");
    }

    public virtual void OnPause()
    {
        Debug.Log("[INFO] Content Client Pause");
    }

    public virtual void OnStop()
    {
        Debug.Log("[INFO] Content Client Stop");
    }

    public abstract void OnData(byte key, byte[] data);

    public virtual void OnManualUpdate() { }

    public virtual void OnInit(CircleVRClient client , AirVRCameraRig rig)
    {
        this.client = client;
    }

    public void SendReliable(byte key, byte[] data)
    {
        client.SendBroadcastReliable(key , data);
    }

    public void SendUnreliable(byte key, byte[] data)
    {
        client.SendBroadcastUnreliable(key, data);
    }

    public void SendStateUpdate(byte key, byte[] data)
    {
        client.SendBroadcastStateUpdate(key, data);
    }

    protected void RequestContentServerData()
    {
        SendReliable((byte)CircleVRPacketType.RequestServerContent);
    }

    public void SendReliable(int connectionId, byte key, byte[] data)
    {
        client.SendReliable(connectionId, key, data);
    }

    public void SendUnreliable(int connectionId, byte key, byte[] data)
    {
        client.SendUnreliable(connectionId, key, data);
    }

    public void SendStateUpdate(int connectionId, byte key, byte[] data)
    {
        client.SendStateUpdate(connectionId, key, data);
    }

    public string ByteToString(byte[] strByte)
    {
        return client.ByteToString(strByte);
    }

    public byte[] StringToByte(string str)
    {
        return client.StringToByte(str);
    }

    public void SendReliable(byte key)
    {
        client.SendBroadcastReliable(key);
    }

    public void SendUnreliable(byte key)
    {
        client.SendBroadcastUnreliable(key);
    }

    public void SendStateUpdate(byte key)
    {
        client.SendBroadcastStateUpdate(key);
    }

    public void SendReliable(int connectionId, byte key)
    {
        client.SendReliable(connectionId , key);
    }

    public void SendUnreliable(int connectionId, byte key)
    {
        client.SendUnreliable(connectionId, key);
    }

    public void SendStateUpdate(int connectionId, byte key)
    {
        client.SendStateUpdate(connectionId, key);
    }
}
