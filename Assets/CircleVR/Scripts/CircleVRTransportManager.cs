using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleVRTransportManager
{
    private CircleVRTransportBase circleVRTransport;

    public CircleVRTransportManager(Configuration config ,Transform trackerOrigin = null)
    {
        //circleVRTransport = CreateTransport(config, trackerOrigin);
    }

    public CircleVRTransportBase CircleVRTransport
    {
        get
        {
            return circleVRTransport;
        }
    }

    //public CircleVRTransportBase CreateTransport(Configuration config , Transform trackerOrigin = null)
    //{
    //    if (config.clientTrackerId != "")
    //    {
    //        CircleVRClientHandler clientHandler  = GameObject.FindObjectOfType<CircleVRClientHandler>();
    //        CircleVRClient client = new CircleVRClient(clientHandler, config , trackerOrigin);
    //        return client;
    //    }

    //    CircleVRServerHandler handler = GameObject.FindObjectOfType<CircleVRServerHandler>();
    //    CircleVRHost host = new CircleVRHost(handler, CircleVR.MAX_CLIENT_COUNT, config);
    //    return host;
    //}

    public void ManualUpdate()
    {
        Debug.Assert(CircleVRTransport != null);

        CircleVRTransport.ManualUpdate();
    }
}
