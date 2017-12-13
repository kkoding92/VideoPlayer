using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlayerClient : CircleVRTransportBase {
    public VideoPlayerClient(int maxConnection) : base(maxConnection)
    {
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();
    }

    protected override void OnBroadcast(int hostId, byte[] data, int size, byte error)
    {
        base.OnBroadcast(hostId, data, size, error);
    }

    protected override void OnConnect(int hostId, int connectionId, byte error)
    {
        base.OnConnect(hostId, connectionId, error);
    }

    protected override void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        base.OnData(hostId, connectionId, channelId, data, size, error);
    }

    protected override void OnDisconnect(int hostId, int connectionId, byte error)
    {
        base.OnDisconnect(hostId, connectionId, error);
    }
}
