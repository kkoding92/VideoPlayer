using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlayerServer : MonoBehaviour, ICircleVRTransportEventHandler
{
    public void OnConnect(int hostId, int connectionId, byte error)
    {
    }

    public void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
    }

    public void OnDisConnect(int hostId, int connectionId, byte error)
    {
    }

    public void OnManualUpdate()
    {
    }
}
