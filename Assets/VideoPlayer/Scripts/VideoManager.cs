using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public enum VideoPlayerPacket : Byte
{
    VideoPlayer = 50,
    Play,
    Pause,
    Back,
    Front,
    Name,
}

[Serializable]
public struct Config
{
    public string serverIp;
    public int serverPort;

    public string network;
}

public class VideoManager : MonoBehaviour {

    private static VideoManager instance = null;

    public static VideoManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<VideoManager>();
            
            return instance;
        }
    }
    
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject display;
    [SerializeField] private VideoClip[] clip;
    [SerializeField] private int frameVal;
    [SerializeField] private string[] ContentName;

    private Config config;
    private CircleVRTransportBase transportBase;

    public Camera Cam
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

    public VideoClip[] Clip
    {
        get
        {
            return clip;
        }

        set
        {
            clip = value;
        }
    }

    public CircleVRTransportBase TransportBase
    {
        get
        {
            return transportBase;
        }

        set
        {
            transportBase = value;
        }
    }

    public int FrameVal
    {
        get
        {
            return frameVal;
        }

        set
        {
            frameVal = value;
        }
    }

    public string[] ContentName1
    {
        get
        {
            return ContentName;
        }

        set
        {
            ContentName = value;
        }
    }

    private void Start()
    {
        LoadConfigure();

        TransportBase = new VideoPlayerClient(config);
    }

    private void Update()
    {
        if (TransportBase != null)
            TransportBase.ManualUpdate();
    }

    private void LoadConfigure()
    {
#if UNITY_EDITOR
        config = JsonUtility.FromJson<Config>(File.ReadAllText(Application.streamingAssetsPath + "/VideoPlayer.json"));
#endif
#if !UNITY_EDITOR && UNITY_STANDALONE
        config = JsonUtility.FromJson<Configuration>(File.ReadAllText(Application.dataPath + "/../VideoPlayer.json"));
#endif
    }
}
