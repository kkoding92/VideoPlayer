using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CircleVRServerHandler :  MonoBehaviour, ICircleVRTransportEventHandler , ICircleVRUIEventHandler
{
    protected bool playing;

    protected float elapseTime;

    public abstract void OnData(string data);

    public virtual void OnInit()
    {
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

    public abstract void OnRequestContentData(out string contentServerData);

    public virtual void OnPlay()
    {
        if (playing)
            return;

        playing = true;
        CircleVR.SendReliable("Play");
        Debug.Log("[INFO] Content Server Send Play");
    }

    public virtual void OnPause()
    {
        if (!playing)
            return;

        playing = false;
        CircleVR.SendReliable("Pause");
        Debug.Log("[INFO] Content Server Send Pause");
    }

    public virtual void OnStop()
    {
        playing = false;
        elapseTime = 0.0f;
        CircleVR.SendReliable("Stop");
        Debug.Log("[INFO] Content Server Send Stop");
    }

    public void SendReliable(string data)
    {
        CircleVR.SendReliable(data);
    }

    public void SendUnreliable(string data)
    {
        CircleVR.SendUnreliable(data);
    }

    public void SendStateUpdate(string data)
    {
        CircleVR.SendStateUpdate(data);
    }
}
