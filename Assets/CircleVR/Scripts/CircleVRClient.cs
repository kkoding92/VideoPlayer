using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CircleVRClient : CircleVRTransportBase, AirVRCameraRigManager.EventHandler
{
    private GameObject headPrefab;
    private string serverIP;
    private int serverPort;

    private bool connecting;
    private bool connected;

    private int connectionId;

    private float autoRequestTime = 2.0f;
    private float autoRequestNowTime;

    private int userId = -1;

    private Dictionary<int, Transform> trackers = new Dictionary<int, Transform>();

    private Transform trackerOrigin;

    private AirVRStereoCameraRig rig;

    private CircleVRClientHandler contentClientHandler;

    private bool initFinished;

    public CircleVRClient(GameObject headPrefab, CircleVRClientHandler contentClientHandler , Configuration config , Transform trackerOrigin = null) : base(1)
    {
        this.headPrefab = headPrefab;
        CircleVRUI.Instance.HostUI.SetActive(false);

        CircleVRUI.Instance.Log = "\nIP : " + Network.player.ipAddress.ToString();

        this.trackerOrigin = trackerOrigin;

        rig = CreateAirVRCameraRig();

        Debug.Assert(rig);

        serverIP = config.serverIp;
        serverPort = config.serverPort;

        if (contentClientHandler != null)
        {
            this.contentClientHandler = contentClientHandler;
            this.contentClientHandler.OnInit(rig);
        }

        initFinished = true;
    }

    private Transform GetTrackerTransform(int userId)
    {
        Transform tmp;
        if (trackers.TryGetValue(userId, out tmp))
            return tmp;

        CircleVRUI.Instance.Log = "\nNot Found Tracker User ID : " + userId.ToString();
        return null;
    }

    private Transform AddTracker(Transform trackerOrigin , int userId)
    {
        Transform tracker = CreateTracker("User ID " + userId.ToString() + " Tracker");
        tracker.parent = trackerOrigin;
        trackers.Add(userId, tracker);

        if (this.userId == userId)
            SetExternalTracker(trackerOrigin , tracker);

        return tracker;
    }

    private Transform CreateTracker(string name)
    {
        GameObject tracker = new GameObject(name);

        GameObject head = GameObject.Instantiate(headPrefab);

        GameObject centerAnchor = new GameObject("CenterAnchor");
        centerAnchor.transform.parent = tracker.transform;
        centerAnchor.transform.localPosition = head.transform.localPosition;
        centerAnchor.transform.localEulerAngles = head.transform.localEulerAngles;

        head.transform.parent = tracker.transform;

        CircleVRUI.Instance.Log = "\nCreated Tracker User ID : " + name;
        Debug.Log("Created Tracker User ID : " + name);
        return tracker.transform;
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

        CircleVRUI.Instance.Log = "\nonAirVR Port : " + onAirVRPort.ToString();

        AirVRCameraRigManager.managerOnCurrentScene.Delegate = this;

        return rig;
    }

 

    private void Connect()
    {
        if (!initFinished)
            return;

        if (connecting)
            return;

        CircleVRUI.Instance.Log = "\nTry Connect!";
        Debug.Log("Try Connect!");

        byte error;

        connectionId = NetworkTransport.Connect(circleVRHostId, serverIP, serverPort, 0, out error);

        if (error != 0)
        {
            Debug.LogError(error);
            return;
        }

        connecting = true;
    }

    private void SetExternalTracker(Transform origin , Transform tracker)
    {
        Debug.Assert(tracker);

        Transform centerAnchor = tracker.Find("CenterAnchor");
        rig.externalTrackingOrigin = origin;
        rig.externalTracker = centerAnchor;

        Debug.Log("Set External Tracker : " + tracker.name);
        CircleVRUI.Instance.Log = "\nSet External Tracker : " + tracker.name;
    }

    private void AutoConnect()
    {
        if (connecting)
            return;

        if(autoRequestNowTime >= autoRequestTime)
        {
            autoRequestNowTime = 0.0f;
            Connect();
            return;
        }

        autoRequestNowTime += Time.deltaTime;
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

        if (contentClientHandler)
            contentClientHandler.OnManualUpdate();
    }

    protected override void OnDisconnect(int connectionId, byte error)
    {
        base.OnDisconnect(connectionId, error);

        CircleVRUI.Instance.Log = "\nDisconnected!";
        Debug.Log("Disconnected");
        connected = false;
        connecting = false;
    }

    protected override void OnConnect(int connectionId, byte error)
    {
        base.OnConnect(connectionId, error);
        CircleVRUI.Instance.Log = "\nUnity Connect Succeed!";
        Debug.Log("Unity Connect Succeed!");
        SendClientData();
    }

    protected override void OnData(int connectionId, int channelId, byte[] data, int size, byte error)
    {
        base.OnData(connectionId, channelId, data, size, error);

        string deserializedData = CircleVR.Deserialize(data, size);

        if (!connected)
        {
            if (deserializedData == "Connected")
            {
                CircleVRUI.Instance.Log = "\nCircle VR Connect Succeed!";
                Debug.Log("Circle VR Connect Succeed!");
                connected = true;
                connecting = false;
                SendReliable(connectionId, "GetHostStatus");
                return;
            }
            if (deserializedData == "Failed")
            {
                CircleVRUI.Instance.Log = "\nCircle VR Connect Failed!";
                Debug.Log("Circle VR Connect Failed!");
                connecting = false;
                return;
            }
        }

        if(contentClientHandler && deserializedData.Contains("elapseTime"))
        {
            Debug.Log("[INFO] Receive Content Server Status");

            ContentServerStatus contentServerStatus = JsonUtility.FromJson<ContentServerStatus>(deserializedData);
            if (contentServerStatus.elapseTime > 0.0f)
            {
                Debug.Log("[INFO] Content Client Enter");
                contentClientHandler.OnEnter(contentServerStatus);
            }

            return;
        }

        if (deserializedData.Contains("cvTransforms"))
        {
            TrackingData hostData = JsonUtility.FromJson<TrackingData>(deserializedData);
            SetTrackerTransform(hostData);
            return;
        }
        if (contentClientHandler && deserializedData.Contains("Play"))
        {
            contentClientHandler.OnPlay();
            return;
        }

        if (contentClientHandler && deserializedData == "Pause")
        {
            contentClientHandler.OnPause();
            return;
        }

        if (contentClientHandler && deserializedData == "Stop")
        {
            contentClientHandler.OnStop();
            return;
        }

        if (contentClientHandler != null)
            contentClientHandler.OnData(deserializedData);
    }

    private void SetTrackerTransform(TrackingData hostData)
    {
        for (int i = 0; i < hostData.cvTransforms.Length; i++)
        {
            Debug.Assert(hostData.cvTransforms[i] != null);
            SetTrackerTransform(hostData.cvTransforms[i]);
        }
    }

    private void SetTrackerTransform(TrackingData.CircleVRTransform cvTransform)
    {
        Transform trackerTransform = GetTrackerTransform(cvTransform.onAirVRUserId);

        if (!trackerTransform)
           trackerTransform = AddTracker(trackerOrigin , cvTransform.onAirVRUserId);

        trackerTransform.localPosition = Vector3.Lerp(trackerTransform.localPosition, cvTransform.position, 0.25f);
        trackerTransform.localRotation = Quaternion.Lerp(trackerTransform.localRotation, cvTransform.oriented, 0.25f);
    }

    private void SendClientData()
    {
        ClientData data = new ClientData(userId);
        string json = JsonUtility.ToJson(data);
        SendReliable(connectionId , json);
    }

    private void Bound(int userId)
    {
        if (this.userId != userId)
            this.userId = userId;

        CircleVRUI.Instance.Log = "\nBounded!";
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
            CircleVRUI.Instance.Log = "\n" + ((NetworkError)error).ToString();
            Debug.LogError(((NetworkError)error).ToString());
            return;
        }

        CircleVRUI.Instance.Log = "\nDisconnected! : onAirVR Unbound";
        Debug.Log("Disconnected! : onAirVR Has Been Unbound");
    }
}
