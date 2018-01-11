using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Valve.VR;
using System.Text;
using System.Linq;
using System;

public class CircleVRHost : CircleVRTransportBase
{
    public class Connection
    {
        public string userID;
        public int connectionId;
        public Connection(string userId , int connectionId)
        {
            this.userID = userId;
            this.connectionId = connectionId;
        }
    }

    private readonly Vector3 HOST_CAMERA_ANCHOR_POS = new Vector3(90, 0, 0);

    private TrackingData trackingData = null;
    private List<Configuration.TrackerAndUserIDPairs> pairs = new List<Configuration.TrackerAndUserIDPairs>();
    private Dictionary<string, SteamVR_TrackedObject> trackers = new Dictionary<string, SteamVR_TrackedObject>();
    private List<Connection> connections = new List<Connection>();

    private CircleVRServerHandler contentServerHandler;
    private VideoPlayerServer videoServer;

    private bool initFinished;

    private Transform trackerOrigin;

    public CircleVRHost(CircleVRServerHandler contentServerHandler, VideoPlayerServer videoServer, int maxConnection, Configuration config, Transform trackerOrigin = null) : base(maxConnection , config.serverPort)
    {
        this.trackerOrigin = trackerOrigin;

        CircleVRUI.Instance.ClientUI.SetActive(false);

        SetTrackers(trackerOrigin);

        SetPairData(config);

        UpdateTrackingData();

        if (contentServerHandler != null)
        {
            this.contentServerHandler = contentServerHandler;
            this.contentServerHandler.OnInit(this);
        }

        this.videoServer = videoServer;
        this.videoServer.OnInit(this);

        initFinished = true;
    }

    private void SetPairData(Configuration config)
    {
        foreach(Configuration.TrackerAndUserIDPairs pair in config.pairs)
        {
            if(HasTrackerIDInPairs(pair.trackerID))
            {
                CircleVRUI.Instance.Log = "\nAlready Has TrackerID : " + pair.trackerID;
                Debug.LogError("[ERROR] Already Has TrackerID : " + pair.trackerID);
                continue;
            }

            if (HasUserIDInPairs(pair.userID))
            {
                CircleVRUI.Instance.Log = "\nAlready Has UserID : " + pair.userID;
                Debug.LogError("[ERROR] Already Has UserID : " + pair.userID);
                Debug.LogError("[ERROR] [" + pair.userID + "] / [" + pair.trackerID + "]");
                continue;
            }

            if (!GetTracker(pair.trackerID))
            {
                CircleVRUI.Instance.Log = "\nNot Found Tracker ID In SteamVR : " + pair.trackerID;
                Debug.LogError("[ERROR] Not Found Tracker ID In SteamVR : " + pair.trackerID);
                continue;
            }

            pairs.Add(pair);
        }
    }

    private void UpdateTrackingData()
    {
        trackingData = new TrackingData();
        trackingData.cvTransforms = new List<TrackingData.CircleVRTransform>();

        foreach (Configuration.TrackerAndUserIDPairs pair in pairs)
        {
            TrackingData.CircleVRTransform cvTransform = new TrackingData.CircleVRTransform();
            cvTransform.onAirVRUserID = pair.userID;
            trackingData.cvTransforms.Add(cvTransform);
        }
    }

    private Configuration.TrackerAndUserIDPairs GetPairData(string userID)
    {
        foreach(Configuration.TrackerAndUserIDPairs pair in pairs)
        {
            if (pair.userID == userID)
                return pair;
        }

        return null;
    }

    private bool HasUserIDInPairs(string userID)
    {
        foreach (Configuration.TrackerAndUserIDPairs pair in pairs)
        {
            if (pair.userID == userID)
                return true;
        }

        return false;
    }

    private bool HasTrackerIDInPairs(string trackerID)
    {
        foreach (Configuration.TrackerAndUserIDPairs pair in pairs)
        {
            if (pair.trackerID == trackerID)
                return true;
        }

        return false;
    }

    private string GetUserID(string trackerID)
    {
        foreach(Configuration.TrackerAndUserIDPairs pair in pairs)
        {
            if (pair.trackerID == trackerID)
                return pair.userID;
        }

        CircleVRUI.Instance.Log = "\nNot Found User ID By Tracker ID : " + trackerID;
        Debug.Log("[INFO] Not Found User ID By Tracker ID : " + trackerID);
        return "";
    }

    private void SetClientCamera()
    {
        List<SteamVR_TrackedObject> listTrackers = trackers.Values.ToList();
        for (int i = 0; i < listTrackers.Count; i++)
        {
            Camera cam = listTrackers[i].transform.Find("CameraAnchor").gameObject.GetComponent<Camera>();
            if(!cam)
                cam = listTrackers[i].transform.Find("CameraAnchor").gameObject.AddComponent<Camera>();

            cam.targetDisplay = i + 1;
        }

        for(int i =0;i <Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        CircleVRUI.Instance.Log = "\nUsable Display Count : " + Display.displays.Length.ToString();
    }

    public SteamVR_TrackedObject GetTracker(string id)
    {
        SteamVR_TrackedObject tmp;
        if (trackers.TryGetValue(id , out tmp))
            return trackers[id];

        return null;
    }

    private void SetTrackers(Transform trackerOrigin)
    {
        List<string> currentIDs = new List<string>();
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new StringBuilder(64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);

            if (result.ToString().Contains("tracker"))
            {
                StringBuilder id = new StringBuilder(64);
          
                OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String, id, 64, ref error);

                currentIDs.Add(id.ToString());

                if (HasTracker(id.ToString()))
                    continue;

                GameObject tracker = new GameObject(id.ToString());
                
                CreateCameraAnchor(tracker);

                SteamVR_TrackedObject trackedObj = tracker.AddComponent<SteamVR_TrackedObject>();

                trackedObj.SetDeviceIndex((int)i);
                trackedObj.transform.parent = trackerOrigin;

                trackers.Add(id.ToString(), trackedObj);
                CircleVRUI.Instance.Log = "\nTracker Count : " + trackers.Count.ToString();
                SetClientCamera();
            }
        }

        foreach(string id in trackers.Keys)
        {
            if(!currentIDs.Contains(id))
            {
                GameObject.Destroy(trackers[id]);
                trackers.Remove(id);
                CircleVRUI.Instance.Log = "\nTracker Count : " + trackers.Count.ToString();
            }
        }

    }

    private void CreateCameraAnchor(GameObject tracker)
    {
        GameObject cameraAnchor = new GameObject("CameraAnchor");
        cameraAnchor.transform.parent = tracker.transform;
        cameraAnchor.transform.eulerAngles = HOST_CAMERA_ANCHOR_POS;
    }

    private Connection GetConnectionByUserID(string userId)
    {
        foreach (Connection connection in connections)
        {
            if (connection.userID == userId)
                return connection;
        }

        return null;
    }
    private Connection GetConnectionByConnectionID(int connectionId)
    {
        foreach (Connection connection in connections)
        {
            if (connection.connectionId == connectionId)
                return connection;
        }

        return null;
    }

    private bool HasTracker(string id)
    {
        return trackers.ContainsKey(id);
    }

    private bool CreateConnection(int connectionId, string userId)
    {
        Connection connection = GetConnectionByUserID(userId);
        if(connection != null)
        {
            CircleVRUI.Instance.Log = "\nAlready host has User ID : " + userId.ToString();
            Debug.Log("[INFO] Already Host Has User ID : " + userId.ToString());
            return false;
        }

        connection = new Connection(userId , connectionId);

        CircleVRUI.Instance.SetClientSlot(connection.userID, ClientState.Connected);

        connections.Add(connection);

        CircleVRUI.Instance.Log = "\nCreate Connection Succeed! User ID : " + userId.ToString();
        Debug.Log("[INFO] Create Connection Succeed! \nConnection ID : " + connectionId.ToString() + " / User ID : " + userId.ToString());

        SendReliable(connectionId , CircleVRPacketType.Connected);
        return true;
    }

    private void Disconnect(int hostId , int connectionId , byte error)
    {
        if (!NetworkTransport.Disconnect(hostId, connectionId, out error))
        {
            CircleVRUI.Instance.Log = "\nDisconnect Error : " + ((NetworkError)error).ToString();
            Debug.LogError(((NetworkError)error).ToString());
        }

        DestroyConnection(connectionId);
    }

    private void DestroyConnection(int connectionId)
    {
        Connection connection = GetConnectionByConnectionID(connectionId);

        CircleVRUI.Instance.Log = "\nDisconnected";
        Debug.Log("[INFO] UserID [ " + connection.userID + " ] Disconnected!");

        Debug.Assert(connection != null);

        CircleVRUI.Instance.SetClientSlot(connection.userID, ClientState.NotConnected);

        connections.Remove(connection);
    }

    private void SendTrackingData()
    {
        foreach(TrackingData.CircleVRTransform cvTransform in trackingData.cvTransforms)
        {
            UpdateTrackingData(cvTransform);
        }

        foreach (Connection connection in connections)
        {
            SendStateUpdate(connection.connectionId, CircleVRPacketType.TrackingData , StringToByte(JsonUtility.ToJson(trackingData)));
        }
    }

    private void UpdateTrackingData(TrackingData.CircleVRTransform cvTransform)
    {
        if(!HasUserIDInPairs(cvTransform.onAirVRUserID))
        {
            //TODO : must be notify in gui
            return;
        }

        Configuration.TrackerAndUserIDPairs pair = GetPairData(cvTransform.onAirVRUserID);

        SteamVR_TrackedObject tracker = GetTracker(pair.trackerID);

        if(tracker == null)
        {
            //TODO : must be notify in gui
            return;
        }

        cvTransform.position = tracker.transform.localPosition;
        cvTransform.oriented = tracker.transform.localRotation;
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (initFinished && connections.Count > 0)
            SendTrackingData();

        if (contentServerHandler)
            contentServerHandler.OnManualUpdate();

        videoServer.OnManualUpdate();

        SetTrackers(trackerOrigin);
    }

    protected override void OnDisconnect(int connectionId, byte error)
    {
        base.OnDisconnect(connectionId, error);
        DestroyConnection(connectionId);
    }

    protected override void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
        base.OnData(connectionId, channelId, key, data, error);

        CircleVRPacketType type = (CircleVRPacketType)key;

        if(type == CircleVRPacketType.ClientData)
        {
            ClientData clientData = JsonUtility.FromJson<ClientData>(ByteToString(data));

            if (CreateConnection(connectionId, clientData.userId))
                return;

            SendReliable(connectionId, CircleVRPacketType.Failed);
            Disconnect(circleVRHostId, connectionId, error);
            return;
        }

        if(contentServerHandler && type == CircleVRPacketType.RequestHostData)
        {
            string contentServerStatus = JsonUtility.ToJson(contentServerHandler.GetContentServerStatus());
            SendReliable(connectionId, CircleVRPacketType.HostData, StringToByte(contentServerStatus));
            Debug.Log("[INFO] Send Content Server Status");
            return;
        }

        if (contentServerHandler && type == CircleVRPacketType.RequestServerContent)
        {
            contentServerHandler.OnRequestContentData(connectionId);
            Debug.Log("[INFO] Send Content Server Data");
            return;
        }

        if (contentServerHandler != null)
            contentServerHandler.OnData(key ,data);

        videoServer.OnData(key, data);
    }
}