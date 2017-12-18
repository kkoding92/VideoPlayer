using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CircleVRClientHandler : MonoBehaviour ,ICircleVRTransportEventHandler
{
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

    public abstract void OnData(string data);

    public virtual void OnManualUpdate() { }

    public abstract void OnInit(AirVRCameraRig rig);

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

    protected void RequestContentServerData()
    {
        SendReliable("RequestServerContent");
    }
}
