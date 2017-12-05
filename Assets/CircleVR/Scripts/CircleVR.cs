using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using System.Text;

[Serializable]
public struct HostData
{
    public string buttonSate;
    public HostData(string buttonSate){
        this.buttonSate = buttonSate;
    }
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

    public string network;
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

    public const int MAX_CLIENT_COUNT = 2;

    [SerializeField] private string contentName;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject display;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject hostUI;
  
    private Configuration config;
    private CircleVRProtocolBase protocol;

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
    
    public CircleVRProtocolBase Protocol
    {
        get
        {
            return protocol;
        }

        set
        {
            protocol = value;
        }
    }

    public GameObject Cam
    {
        get
        {
            return cam;
        }

        set
        {
            cam = value;
        }
    }

    public GameObject Display
    {
        get
        {
            return display;
        }

        set
        {
            display = value;
        }
    }

    private void Start()
    {
        canvas.SetActive(false);

        LoadConfigure();

        if (config.network == "Host")
        {
            Protocol = new CircleVRHost();
        }
        else if(config.network == "client")
        {
            Protocol = new CircleVRClient();
            hostUI.SetActive(false);
        }

        Protocol.Init(config);
    }
    private void Update()
    {
        if(Input.GetKeyDown("`"))
            canvas.SetActive(!canvas.activeSelf);

        if (Protocol == null)
            return;

        Protocol.ManualUpdate();
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