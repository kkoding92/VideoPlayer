using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICircleVRTransportEventHandler
{
    void OnConnect(int hostId, int connectionId, byte error);
    void OnDisConnect(int hostId, int connectionId, byte error);
    void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error);
    void OnManualUpdate();
}
