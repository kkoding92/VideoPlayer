using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Valve.VR;
using System.Text;
using System.Linq;

public class CircleVRHost : CircleVRTransportBase
{
    public class Tracker
    {
        public int deviceId;
        public SteamVR_TrackedObject trackedObj;

        public Tracker(int deviceId , SteamVR_TrackedObject trackedObj)
        {
            this.deviceId = deviceId;
            this.trackedObj = trackedObj;
        }
    }

    public class Connection
    {
        public int id;
        public int userId;
        public string trackerId;
    }

    private readonly Vector3 HOST_CAMERA_ANCHOR_POS = new Vector3(90, 0, 0);

    private CVRSystem _vrSystem;
    private TrackedDevicePose_t[] _poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

    private HostData datas = null;

    private List<Connection> connections = new List<Connection>();

    private Camera[] clientCams;

    private CircleVRServerHandler contentServerHandler;

    private bool initFinished;

    private Dictionary<string, Tracker> trackers = new Dictionary<string, Tracker>();

    public CircleVRHost(CircleVRServerHandler contentServerHandler, int maxConnection, Configuration config, Transform trackerOrigin = null) : base(maxConnection , config.serverPort)
    {
        CircleVR.Instance.ClientUI.SetActive(false);

        OpenVRInit();

        datas = SetHostData(CircleVR.MAX_CLIENT_COUNT);

        SetTrackers(trackerOrigin);

        if (contentServerHandler != null)
        {
            this.contentServerHandler = contentServerHandler;
            this.contentServerHandler.OnInit();
        }

        initFinished = true;
    }

    private HostData SetHostData(int maxClientCount)
    {
        HostData datas = new HostData();

        datas.cvTransforms = new HostData.CircleVRTransform[maxClientCount];

        for (int i = 0; i < datas.cvTransforms.Length; i++)
        {
            datas.cvTransforms[i] = new HostData.CircleVRTransform();
        }

        return datas;
    }

    private void SetClientCamera()
    {
        List<Tracker> listTrackers = trackers.Values.ToList();
        clientCams = new Camera[listTrackers.Count];
        for (int i = 0; i < listTrackers.Count; i++)
        {
            clientCams[i] = listTrackers[i].trackedObj.transform.Find("CameraAnchor").gameObject.AddComponent<Camera>();
            clientCams[i].targetDisplay = i+1;
            //Display.displays[i].SetRenderingResolution(Display.displays[i].systemWidth, Display.displays[i].systemHeight);
        }

        for(int i =0;i <Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        CircleVR.Instance.Log = "\nUsable Display Count : " + Display.displays.Length.ToString();
    }

    private void OpenVRInit()
    {
        var err = EVRInitError.None;
        _vrSystem = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Other);

        if (err != EVRInitError.None)
        {
            CircleVR.Instance.Log = "\nOpenVR Init Error : " + err.ToString();
            Debug.LogError(err.ToString());
        }

        CircleVR.Instance.Log = "\nOpenVR Init Succeed!";
    }

    public Tracker GetTracker(string id)
    {
        if (trackers.ContainsKey(id))
            return trackers[id];

        return null;
    }

    private void SetTrackers(Transform trackerOrigin)
    {
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new StringBuilder(64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            if (result.ToString().Contains("tracker"))
            {
                StringBuilder id = new StringBuilder(64);
                OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String, id, 64, ref error);

                GameObject tracker = new GameObject(id.ToString());
                CreateCameraAnchor(tracker);

                SteamVR_TrackedObject trackedObj = tracker.AddComponent<SteamVR_TrackedObject>();

                trackedObj.SetDeviceIndex((int)i);
                trackedObj.transform.parent = trackerOrigin;

                trackers.Add(id.ToString(), new Tracker((int)i, trackedObj));
            }
        }

        CircleVR.Instance.Log = "\nTracker Count : " + trackers.Count.ToString();

        SetClientCamera();
    }

    private void CreateCameraAnchor(GameObject tracker)
    {
        GameObject cameraAnchor = new GameObject("CameraAnchor");
        cameraAnchor.transform.parent = tracker.transform;
        cameraAnchor.transform.eulerAngles = HOST_CAMERA_ANCHOR_POS;
    }

    private Connection GetConnectionByUserID(int userId)
    {
        foreach (Connection connection in connections)
        {
            if (connection.userId == userId)
                return connection;
        }

        return null;

    }
    private Connection GetConnectionByConnectionID(int connectionId)
    {
        foreach (Connection connection in connections)
        {
            if (connection.id == connectionId)
                return connection;
        }

        return null;
    }

    private bool HasTrackerID(string trackerId)
    {
        foreach(Connection connection in connections)
        {
            if (connection.trackerId == trackerId)
                return true;
        }

        return false;
    }

    private bool TryCreateConnection(int connectionId, ClientData data)
    {
        Debug.Assert(data.userId < CircleVR.MAX_CLIENT_COUNT);

        if (HasUserID(data.userId, connections))
        {
            CircleVR.Instance.Log = "\nCrete Connection Failed\nAlready has User ID : " + data.userId.ToString();
            Debug.LogError("Already has User ID : " + data.userId.ToString());
            return false;
        }

        Connection connection = GetConnectionByConnectionID(connectionId);

        if (connection != null)
        {
            CircleVR.Instance.Log = "\nCrete Connection Failed\nAlready has Connection ID : " + connectionId.ToString();
            Debug.LogError("Already has Connection ID : " + connectionId.ToString());
            return false;
        }

        connection = new Connection();
        connection.id = connectionId;
        connection.userId = data.userId;
        connection.trackerId = data.trackerId;

        CircleVR.Instance.SetClientSlot(connection.userId, ClientState.Connected);

        connections.Add(connection);

        CircleVR.Instance.Log = "\nCreate Connection Succeed!\n"
            + "Player Connection ID : " + connectionId.ToString() + "\n"
            + "Player User ID : " + data.userId.ToString() + "\n"
            + "Player Tracker ID : " + data.trackerId.ToString();

        CircleVR.SendDataReliable(connectionId, "Connected");
        return true;
    }

    private void Disconnect(int hostId , int connectionId , byte error)
    {
        if (!NetworkTransport.Disconnect(hostId, connectionId, out error))
        {
            CircleVR.Instance.Log = "\nDisconnect Error : " + ((NetworkError)error).ToString();
            Debug.LogError(((NetworkError)error).ToString());
        }

        DestroyConnection(connectionId , hostId);
    }

    private void DestroyConnection(int connectionId, int recHostId)
    { 
        CircleVR.Instance.Log = "\nDisconnected";
        Debug.Log("Player " + connectionId.ToString() + " disconnected!");

        Connection connection = GetConnectionByConnectionID(connectionId);

        Debug.Assert(connection != null);

        CircleVR.Instance.SetClientSlot(connection.userId, ClientState.NotConnected);

        connections.Remove(connection);
    }

    private bool HasUserID(int userId, List<Connection> list)
    {
        foreach (Connection connection in list)
        {
            if (connection.userId == userId)
                return true;
        }

        return false;
    }

    private void SendPosition()
    {
        for (int i = 0; i < datas.cvTransforms.Length; i++)
        {
            SetCircleVRTransform(datas.cvTransforms[i], i);
        }

        foreach (Connection connection in connections)
        {
            CircleVR.SendDataStateUpdate(connection.id, JsonUtility.ToJson(datas));
        }
    }

    private void SetCircleVRTransform(HostData.CircleVRTransform target , int userId)
    {
        Connection connection = GetConnectionByUserID(userId);
        if (connection == null)
            return;

        Tracker tracker = GetTracker(connection.trackerId);

        if(tracker == null)
        {
            CircleVR.Instance.Log = "\n Not Found Tracker ID : " + connection.trackerId;
            Debug.LogError("Not Found Tracker ID : " + connection.trackerId);
            return;
        }

        Transform trackerTransform = tracker.trackedObj.transform;

        target.position = trackerTransform.localPosition;
        target.oriented = trackerTransform.localRotation;
    }

    public override void ManualUpdate()
    {
        TrackerUpdate();

        base.ManualUpdate();

        if (initFinished && connections.Count > 0)
            SendPosition();
    }

    private void TrackerUpdate()
    {
        _vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, _poses);
        SteamVR_Events.NewPoses.Send(_poses);
    }

    private void OnDestroy()
    {
        OpenVR.Shutdown();
    }

    protected override void OnDisconnect(int hostId, int connectionId, byte error)
    {
        base.OnDisconnect(hostId, connectionId, error);

        contentServerHandler.OnDisConnect(hostId, connectionId, error);
        DestroyConnection(connectionId, hostId);
    }

    protected override void OnConnect(int hostId, int connectionId, byte error)
    {
        base.OnConnect(hostId, connectionId, error);
        contentServerHandler.OnConnect(hostId , connectionId , error);
    }

    protected override void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        base.OnData(hostId, connectionId, channelId, data, size, error);

        string strData = CircleVR.Deserialize(data ,size);

        if(strData.Contains("ClientData"))
        {
            ClientData clientData = JsonUtility.FromJson<ClientData>(strData);

            if (TryCreateConnection(connectionId, clientData))
                return;

            CircleVR.SendDataReliable(connectionId, "Failed");

            Disconnect(hostId, connectionId, error);
            return;
        }

        contentServerHandler.OnData(hostId, connectionId, channelId, data, size, error);
    }
}