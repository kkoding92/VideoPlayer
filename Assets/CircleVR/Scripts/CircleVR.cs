﻿using System;
using System.IO;
using UnityEngine;

[Serializable]
public class Configuration
{
    [Serializable]
    public class HostData
    {
        public string userId;
        public string trackerId;
    }

    public HostData[] data;
    public string serverIp;
    public int serverPort;
}

public enum ClientState
{
    Connected,
    NotConnected
}

public class CircleVR : MonoBehaviour
{
    public const int MAX_CLIENT_COUNT = 4;

    [SerializeField] private string contentName;
    //[SerializeField] private GameObject headModelPrefab;
    [SerializeField] private Transform trackerOrigin;

    private Configuration config;
    //private static CircleVRTransportManager manager = null;

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

    private void Start()
    {
        LoadConfigure();

        //manager = new CircleVRTransportManager(config, trackerOrigin);
    }

    private void LateUpdate()
    {
        //if (manager != null)
        //    manager.ManualUpdate();
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