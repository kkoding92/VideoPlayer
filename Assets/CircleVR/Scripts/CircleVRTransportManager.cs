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
        GameObject headPrefab = Resources.Load<GameObject>("head");
        if (config.pairs == null)
        {
            CircleVRClientHandler clientHandler  = GameObject.FindObjectOfType<CircleVRClientHandler>();
            CircleVRClient client = new CircleVRClient(headPrefab ,clientHandler, config , trackerOrigin);
            return client;
        }

        CircleVRServerHandler handler = GameObject.FindObjectOfType<CircleVRServerHandler>();
        VideoPlayerServer videoServer = GameObject.FindObjectOfType<VideoPlayerServer>();
        CircleVRHost host = new CircleVRHost(handler, videoServer, CircleVR.MAX_CLIENT_COUNT, config , trackerOrigin);
        return host;
    }

    public void ManualUpdate()
    {
        Debug.Assert(CircleVRTransport != null);

        CircleVRTransport.ManualUpdate();
    }
}
