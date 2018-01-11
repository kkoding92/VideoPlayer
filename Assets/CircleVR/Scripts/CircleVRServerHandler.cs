using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CircleVRServerHandler :  MonoBehaviour, ICircleVRTransportEventHandler , ICircleVRUIEventHandler
{
    private CircleVRHost host;

    protected bool playing;

    protected float elapseTime;

    public abstract void OnData(byte key, byte[] data);

    public virtual void OnInit(CircleVRHost host)
    {
        this.host = host;

        CircleVRUI.Instance.Delegate = this;
    }

    public virtual void OnManualUpdate()
    {
        if (!playing)
            return;

        elapseTime += Time.deltaTime;
    }

    public bool IsPlaying()
    {
        return playing;
    }

    public ContentServerStatus GetContentServerStatus()
    {
        Debug.Log("[INFO] Content Server Status\nPlaying / " + playing + " , ElapseTime / " + elapseTime.ToString());
        return new ContentServerStatus(playing, elapseTime);
    }

    public abstract void OnRequestContentData(int connectionId);

    public virtual void OnPlay()
    {
        if (playing)
            return;

        playing = true;
        host.SendBroadcastReliable(CircleVRPacketType.Play , null);
        Debug.Log("[INFO] Content Server Send Play");
    }

    public virtual void OnPause()
    {
        if (!playing)
            return;

        playing = false;
        host.SendBroadcastReliable(CircleVRPacketType.Pause, null);
        Debug.Log("[INFO] Content Server Send Pause");
    }

    public virtual void OnStop()
    {
        playing = false;
        elapseTime = 0.0f;
        host.SendBroadcastReliable(CircleVRPacketType.Stop, null);
        Debug.Log("[INFO] Content Server Send Stop");
    }

    public void SendReliable(byte key , byte[] data)
    {
        host.SendBroadcastReliable(key, data);
    }

    public void SendUnreliable(byte key, byte[] data)
    {
        host.SendBroadcastUnreliable(key , data);
    }

    public void SendStateUpdate(byte key, byte[] data)
    {
        host.SendBroadcastStateUpdate(key, data);
    }

    public void SendReliable(int connectionId, byte key, byte[] data)
    {
        host.SendReliable(connectionId, key, data);
    }

    public void SendUnreliable(int connectionId, byte key, byte[] data)
    {
        host.SendUnreliable(connectionId, key, data);
    }

    public void SendStateUpdate(int connectionId, byte key, byte[] data)
    {
        host.SendStateUpdate(connectionId, key, data);
    }

    public string ByteToString(byte[] strByte)
    {
        return host.ByteToString(strByte);
    }

    public byte[] StringToByte(string str)
    {
        return host.StringToByte(str);
    }

    public void SendReliable(byte key)
    {
        host.SendBroadcastReliable(key);
    }

    public void SendUnreliable(byte key)
    {
        host.SendBroadcastUnreliable(key);
    }

    public void SendStateUpdate(byte key)
    {
        host.SendBroadcastStateUpdate(key);
    }

    public void SendReliable(int connectionId, byte key)
    {
        host.SendReliable(connectionId, key);
    }

    public void SendUnreliable(int connectionId, byte key)
    {
        host.SendUnreliable(connectionId, key);
    }

    public void SendStateUpdate(int connectionId, byte key)
    {
        host.SendStateUpdate(connectionId, key);
    }
}
