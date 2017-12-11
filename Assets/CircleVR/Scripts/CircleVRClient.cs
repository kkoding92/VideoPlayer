using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CircleVRClient : CircleVRTransportBase, AirVRCameraRigManager.EventHandler
{
    private string serverIP;
    private int serverPort;

    private bool connecting;
    private bool connected;

    private int connectionId;

    private float autoRequestTime = 2.0f;

    private string trackerId;

    private int userId = -1;

    private Transform[] trackers;

    private AirVRStereoCameraRig rig;
    private Transform parent;

    private CircleVRClientHandler contentClientHandler;

    private bool initFinished;

    public CircleVRClient(CircleVRClientHandler contentClientHandler , Configuration config , Transform trackerOrigin = null) : base(1)
    {
        CircleVR.Instance.HostUI.SetActive(false);

        CircleVR.Instance.Log = "\nTracker ID : " + config.clientTrackerId.ToString() + "\n"
    + "IP : " + Network.player.ipAddress.ToString();

        rig = CreateAirVRCameraRig();

        trackers = CreateTracker(trackerOrigin , CircleVR.MAX_CLIENT_COUNT);

        Debug.Assert(rig);

        serverIP = config.serverIp;
        serverPort = config.serverPort;
        trackerId = config.clientTrackerId;

        if (contentClientHandler != null)
        {
            this.contentClientHandler = contentClientHandler;
            this.contentClientHandler.OnInit(rig);
        }
        initFinished = true;
    }

    private Transform[] CreateTracker(Transform trackerOrigin , int maxClientCount)
    {
        Transform[] trackers = new Transform[maxClientCount];
        for (int i = 0; i < trackers.Length; i++)
        {
            trackers[i] = CreateTracker("Tracker" + i.ToString());
            trackers[i].parent = trackerOrigin;
        }

        return trackers;
    }

    private AirVRStereoCameraRig CreateAirVRCameraRig()
    {
        int onAirVRPort = 9090;
        AirVRServerInitParams initParam = GameObject.FindObjectOfType<AirVRServerInitParams>();
        if (initParam)
            onAirVRPort = initParam.port;

        AirVRStereoCameraRig rig;
               
        rig = new GameObject("AirVRCameraRig").AddComponent<AirVRStereoCameraRig>();
        rig.trackingModel = AirVRStereoCameraRig.TrackingModel.ExternalTracker;
        rig.centerEyeAnchor.gameObject.AddComponent<Camera>();

        CircleVR.Instance.Log = "\nonAirVR Port : " + onAirVRPort.ToString();

        AirVRCameraRigManager.managerOnCurrentScene.Delegate = this;

        return rig;
    }

    private Transform CreateTracker(string name)
    {
        GameObject tracker = new GameObject(name);

        GameObject centerAnchor = new GameObject("CenterAnchor");
        centerAnchor.transform.parent = tracker.transform;
        centerAnchor.transform.localPosition = CircleVR.Instance.CENTER_ANCHOR_POSITION;
        centerAnchor.transform.localEulerAngles = CircleVR.Instance.CENTER_ANCHOR_EULER;

        GameObject head = GameObject.Instantiate(CircleVR.Instance.HeadModelPrefab);
        head.transform.parent = tracker.transform;
        head.transform.localPosition = centerAnchor.transform.localPosition;
        head.transform.localEulerAngles = centerAnchor.transform.localEulerAngles;

        return tracker.transform;
    }

    private void Connect()
    {
        if (!initFinished)
            return;

        if (connecting)
            return;

        CircleVR.Instance.Log = "\nTry Connect!";

        byte error;

        connectionId = NetworkTransport.Connect(circleVRHostId, serverIP, serverPort, 0, out error);

        if (error != 0)
        {
            Debug.LogError(error);
            return;
        }

        connecting = true;
    }

    private void SetRigParent(int userID)
    {
        Debug.Assert(trackers[userID]);

        parent = trackers[userID].Find("CenterAnchor");
        rig.externalTracker = parent;
    }

    private float nowTime;

    private void AutoConnect()
    {
        if (connecting)
            return;

        if(nowTime >= autoRequestTime)
        {
            nowTime = 0.0f;
            Connect();
            return;
        }

        nowTime += Time.deltaTime;
    }

    public override void ManualUpdate()
    {
        if (!rig)
            return;

        if (!rig.isBoundToClient)
            return;

        if (!initFinished)
            return;

        if (userId == -1)
            return;

        if (!connected)
            AutoConnect();

        base.ManualUpdate();
    }

    protected override void OnDisconnect(int hostId, int connectionId, byte error)
    {
        base.OnDisconnect(hostId, connectionId, error);

        CircleVR.Instance.Log = "\nDisconnected!";
        connected = false;
        connecting = false;

        contentClientHandler.OnDisConnect(hostId, connectionId, error);
    }

    protected override void OnConnect(int hostId, int connectionId, byte error)
    {
        base.OnConnect(hostId, connectionId, error);
        CircleVR.Instance.Log = "\nConnection Request Success!";
        SendClientData();

        contentClientHandler.OnConnect(hostId, connectionId, error);
    }

    protected override void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        base.OnData(hostId, connectionId, channelId, data, size, error);

        string deserializedData = CircleVR.Deserialize(data, size);

        if (!connected)
        {
            if (deserializedData == "Connected")
            {
                CircleVR.Instance.Log = "\nConnect Success!";
                connected = true;
                connecting = false;
                return;
            }
            if (deserializedData == "Failed")
            {
                CircleVR.Instance.Log = "\nReceived Connect failed!";
                connecting = false;
                return;
            }
        }

        if(deserializedData.Contains("HostData"))
        {
            HostData hostData = JsonUtility.FromJson<HostData>(deserializedData);

            SetTrackerTransform(hostData);
            return;
        }

        contentClientHandler.OnData(hostId, connectionId, channelId, data, size, error);
    }

    private void SetTrackerTransform(HostData hostData)
    {
        for (int i = 0; i < trackers.Length; i++)
        {
            Debug.Assert(hostData.cvTransforms[i] != null);

            if (i== userId)
            {
                SetTrackerTransform(trackers[i], hostData.cvTransforms[i]);
                continue;
            }

            Vector3 lerpPos = Vector3.Lerp(trackers[i].localPosition, hostData.cvTransforms[i].position, 0.5f);
            Quaternion lerpRot = Quaternion.Lerp(trackers[i].localRotation, hostData.cvTransforms[i].oriented, 0.5f);
            SetTrackerTransform(trackers[i], lerpPos, lerpRot);
        }

        CircleVR.Instance.Position = rig.transform.position.ToString();
        CircleVR.Instance.Oriented = trackers[userId].rotation.ToString();
    }
    private void SetTrackerTransform(Transform transform, Vector3 pos , Quaternion rot)
    {
        transform.localPosition = pos;
        transform.localRotation = rot;
    }

    private void SetTrackerTransform(Transform transform , HostData.CircleVRTransform CVTransform)
    {
        transform.localPosition= CVTransform.position;
        transform.localRotation = CVTransform.oriented;
    }

    private void SendClientData()
    {
        ClientData data = new ClientData(userId, trackerId);
        string json = JsonUtility.ToJson(data);
        CircleVR.SendDataReliable(connectionId , json);
    }

    private void Bound(int userId)
    {
        if (this.userId != userId)
        {
            this.userId = userId;
            SetRigParent(userId);
        }

        CircleVR.Instance.Log = "\nBounded!";
    }

    public void AirVRCameraRigWillBeBound(AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected)
    {
        selected = rig;

        Bound(config.userID);
    }

    public void AirVRCameraRigActivated(AirVRCameraRig cameraRig)
    {
    }

    public void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig)
    {
    }

    public void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig)
    {
        byte error;
        if(!NetworkTransport.Disconnect(circleVRHostId, connectionId, out error))
        {
            CircleVR.Instance.Log = "\n" + ((NetworkError)error).ToString();
            Debug.LogError(((NetworkError)error).ToString());
            return;
        }

        CircleVR.Instance.Log = "\nDisconnected! : onAirVR Unbound";
    }
}
