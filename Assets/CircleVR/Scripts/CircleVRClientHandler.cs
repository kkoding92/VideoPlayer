using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CircleVRClientHandler : MonoBehaviour ,ICircleVRTransportEventHandler
{
    public abstract void OnConnect(int hostId, int connectionId, byte error);

    public abstract void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error);

    public abstract void OnDisConnect(int hostId, int connectionId, byte error);

    public abstract void OnManualUpdate();

    public abstract void OnInit(AirVRCameraRig rig);
}

