using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using System.Text;

[Serializable]
public class HostData
{
    [Serializable]
    public class CircleVRTransform
    {
        public Vector3 position;
        public Quaternion oriented;
    }

    //'cvTransforms' index is 'onAirVR User ID'
    public CircleVRTransform[] cvTransforms;
}

[Serializable]
public struct ClientData
{
    public int userId;
    public string trackerId;

    public ClientData(int userId , string trackerId)
    {
        this.userId = userId;
        this.trackerId = trackerId;
    }
}

[Serializable]
public struct Configuration
{
    public string serverIp;
    public int serverPort;

    public string clientTrackerId;
}

public enum ClientState
{
    Connected,
    NotConnected
}

public class CircleVR : MonoBehaviour
{
    private static CircleVR instance = null;

    public static CircleVR Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<CircleVR>();

            Debug.Assert(instance);

            return instance;
        }
    }

    public const int MAX_CLIENT_COUNT = 4;

    public Vector3 CENTER_ANCHOR_POSITION;
    public Vector3 CENTER_ANCHOR_EULER;

    [SerializeField] private string contentName;
    [SerializeField] private GameObject headModelPrefab;
    [SerializeField] private Transform trackerOrigin;

    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject hostUI;
    [SerializeField] private GameObject clientUI;
    [SerializeField] private Text log;
    [SerializeField] private Text frame;

    //Host UI
    [SerializeField] private Image client1;
    [SerializeField] private Image client2;
    [SerializeField] private Image client3;
    [SerializeField] private Image client4;

    [SerializeField] private Text position;
    [SerializeField] private Text oriented;

    private Configuration config;
    private static CircleVRProtocol protocol = new CircleVRProtocol();
    private static CircleVRTransportManager manager = null;

    public string Log
    {
        get
        {
            return log.text;
        }

        set
        {
            StringBuilder stringBuilder = new StringBuilder(log.text);
            stringBuilder.Append(value);
            log.text = stringBuilder.ToString();
        }
    }
    public string ContentName
    {
        get
        {
            return contentName;
        }

        set
        {
            contentName = value;
        }
    }
    public string Position
    {
        get
        {
            return position.text;
        }

        set
        {
            position.text = value;
        }
    }
    public string Oriented
    {
        get
        {
            return oriented.text;
        }

        set
        {
            oriented.text = value;
        }
    }

    public GameObject HeadModelPrefab
    {
        get
        {
            return headModelPrefab;
        }

        set
        {
            headModelPrefab = value;
        }
    }
    public GameObject HostUI
    {
        get
        {
            return hostUI;
        }

        set
        {
            hostUI = value;
        }
    }
    public GameObject ClientUI
    {
        get
        {
            return clientUI;
        }

        set
        {
            clientUI = value;
        }
    }

    public static void SendDataReliable(int connectionId , string data)
    {
        protocol.SendData(manager.CircleVRTransport.circleVRHostId, data, connectionId, manager.CircleVRTransport.reliableChannel);
    }

    public static void SendDataUnreliable(int connectionId, string data)
    {
        protocol.SendData(manager.CircleVRTransport.circleVRHostId, data, connectionId, manager.CircleVRTransport.unreliableChannel);
    }

    public static void SendDataStateUpdate(int connectionId, string data)
    {
        protocol.SendData(manager.CircleVRTransport.circleVRHostId, data, connectionId, manager.CircleVRTransport.stateUpdateChannel);
    }

    public static string Deserialize(byte[] buffer, int recBufferSize)
    {
        return protocol.Deserialize(buffer, recBufferSize);
    }

    public void SetClientSlot(int userId , ClientState state)
    {
        Debug.Assert(userId >= 0 && userId < MAX_CLIENT_COUNT);

        switch (userId)
        {
            case 0:
                SetSlotColor(client1, state);
                return;

            case 1:
                SetSlotColor(client2, state);
                return;

            case 2:
                SetSlotColor(client3, state);
                return;

            case 3:
                SetSlotColor(client4, state);
                return;
        }
    }

    private void SetSlotColor(Image slot , ClientState state)
    {
        if(state == ClientState.Connected)
        {
            slot.color = Color.green;
            return;
        }

        slot.color = Color.red;
    }

    private void SetClientSlotCount(int count)
    {
        Debug.Assert(count > 0 && count <= 4);

        switch (count)
        {
            case 1:
                client2.gameObject.SetActive(false);
                client3.gameObject.SetActive(false);
                client4.gameObject.SetActive(false);
                return;

            case 2:
                client3.gameObject.SetActive(false);
                client4.gameObject.SetActive(false);
                return;

            case 3:
                client4.gameObject.SetActive(false);
                return;
        }
    }

    private void Start()
    {
        SetClientSlotCount(MAX_CLIENT_COUNT);
        canvas.SetActive(false);

        LoadConfigure();

        manager = new CircleVRTransportManager(config, trackerOrigin);
    }

    private void FrameRateUpdate()
    {
        //TODO : FrameRate update
    }

    private void Update()
    {
        if(Input.GetKeyDown("`"))
            canvas.SetActive(!canvas.activeSelf);

        if(canvas.activeSelf)
            FrameRateUpdate();

        if (manager != null)
            manager.ManualUpdate();
    }

    private void LoadConfigure()
    {
#if UNITY_EDITOR
        config = JsonUtility.FromJson<Configuration>(File.ReadAllText( Application.streamingAssetsPath + "/circleVR.json"));
#endif
#if !UNITY_EDITOR && UNITY_STANDALONE
        config = JsonUtility.FromJson<Configuration>(File.ReadAllText(Application.dataPath + "/../circleVR.json"));
#endif
    }
}