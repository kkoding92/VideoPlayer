using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public struct Configuration
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
    
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject display;
    [SerializeField] private VideoClip[] clip;

    private Configuration config;
    private CircleVRTransportBase protocol;

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

    public CircleVRTransportBase Protocol
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

    private void Start()
    {
        LoadConfigure();

        Protocol = new VideoPlayerClient();
        Protocol.Init(config);
    }

    private void Update()
    {
        if (Protocol == null)
            return;

        Protocol.ManualUpdate();
    }

    private void LoadConfigure()
    {
#if UNITY_EDITOR
        config = JsonUtility.FromJson<Configuration>(File.ReadAllText(Application.streamingAssetsPath + "/VideoPlayer.json"));
#endif
#if !UNITY_EDITOR && UNITY_STANDALONE
        config = JsonUtility.FromJson<Configuration>(File.ReadAllText(Application.dataPath + "/../VideoPlayer.json"));
#endif
    }
}
