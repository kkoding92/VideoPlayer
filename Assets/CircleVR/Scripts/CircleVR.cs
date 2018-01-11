using System;
using System.IO;
using System.Collections;
using UnityEngine;

[Serializable]
public class Configuration {
    [Serializable]
    public class TrackerAndUserIDPairs
    {
        public string userID;
        public string trackerID;
    }

    public int gcsPort;
    public TrackerAndUserIDPairs[] pairs;
    public string serverIp;
    public int serverPort;

    private void parseCommandLineArgs() {
        string[] args = Environment.GetCommandLineArgs();
        foreach (var arg in args) {
            int splitIndex = arg.IndexOf("=");
            if (splitIndex <= 0) {
                continue;
            }

            string name = arg.Substring(0, splitIndex);
            string value = arg.Substring(splitIndex + 1);
            if (name.Equals("server_port")) {
                int.TryParse(value, out serverPort);
            }
            else if (name.Equals("onairvr_gcs_port")) {
                int.TryParse(value, out gcsPort);
            }
        }
    }

    private void setTrackers(CloudVRTracker[] trackers) {
        pairs = new TrackerAndUserIDPairs[trackers.Length];
        for (int i = 0; i < trackers.Length; i++) {
            pairs[i] = new TrackerAndUserIDPairs();

            if (trackers[i].userID != pairs[i].userID)
            {
                pairs[i].userID = trackers[i].userID;
            }
            pairs[i].trackerID = trackers[i].trackerID;
        }
    }

    public bool isHost {
        get {
            return gcsPort > 0;
        }
    }

    public IEnumerator Load(MonoBehaviour caller) {
        parseCommandLineArgs();

        if (isHost) {
            serverIp = "127.0.0.1";

            CloudVRClient cloudVRClient = new CloudVRClient(serverIp, gcsPort);
            yield return caller.StartCoroutine(cloudVRClient.GetTrackers(caller,
                (trackers) => {
                    setTrackers(trackers);
                },
                (errorCode, error) => {
                    Debug.Log("CloudVRClient.GetTrackers failed : " + errorCode + " : " + error);
                }
            ));
        }
        else {
            AirVRServer.LoadOnce();

            serverIp = AirVRServer.serverParams.groupServer;
            JsonUtility.FromJsonOverwrite(AirVRServer.serverParams.userData, this);
        }
    }
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
    private static CircleVRTransportManager manager = null;

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

    private IEnumerator Start()
    {
        yield return StartCoroutine(LoadConfigure());

        manager = new CircleVRTransportManager(config, trackerOrigin);
    }

    private void LateUpdate()
    {
        if (manager != null)
            manager.ManualUpdate();
    }

    private IEnumerator LoadConfigure()
    {
        if (Application.isEditor) {
            config = JsonUtility.FromJson<Configuration>(File.ReadAllText(Application.streamingAssetsPath + "/circleVR.json"));
        }
        else {
            config = new Configuration();
            yield return StartCoroutine(config.Load(this));
        }
    }
}