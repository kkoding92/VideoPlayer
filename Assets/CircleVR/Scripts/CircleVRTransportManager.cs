using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleVRTransportManager
{
    private CircleVRTransportBase circleVRTransport;

    public CircleVRTransportManager(Configuration config ,Transform trackerOrigin = null)
    {
        circleVRTransport = CreateTransport(config, trackerOrigin);
    }

    public CircleVRTransportBase CircleVRTransport
    {
        get
        {
            return circleVRTransport;
        }
    }

    public CircleVRTransportBase CreateTransport(Configuration config , Transform trackerOrigin = null)
    {
        ICircleVRTransportEventHandler eventHandler;
        if (config.clientTrackerId != "")
        {
            eventHandler = GameObject.FindObjectOfType<CircleVRClientHandler>();
            CircleVRClient client = new CircleVRClient(eventHandler , config , trackerOrigin);
            return client;
        }

        eventHandler = GameObject.FindObjectOfType<CircleVRServerHandler>();
        CircleVRHost host = new CircleVRHost(eventHandler, CircleVR.MAX_CLIENT_COUNT, config);
        return host;
    }

    public void ManualUpdate()
    {
        Debug.Assert(CircleVRTransport != null);

        CircleVRTransport.ManualUpdate();
    }
}
