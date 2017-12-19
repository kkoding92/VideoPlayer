using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class VideoPlayerClient : CircleVRTransportBase {

    private string serverIP;
    private int serverPort;

    private bool connecting;
    private bool connected;

    private bool initFinished;

    private int connectionId;
    private List<Camera> camList = new List<Camera>();
    private VideoPlayer vp;

    private string currentContentName = null;
    private string contentName;

    public VideoPlayerClient(Config config) : base(1)
    {
        serverIP = config.serverIp;
        serverPort = config.serverPort;

        CreateCameras();
        CreateVideoPanel();

        initFinished = true;
    }

    private void CreateCameras()
    {
        for (int i = 0; i < 4; i++)
        {
            Camera camObj = GameObject.Instantiate(VideoManager.Instance.Cam);
            camList.Add(camObj);
        }
        SetCamera();
    }

    private void SetCamera()
    {
        float frustumHeight = 2.0f * 2.0f * Mathf.Tan(camList[0].fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * camList[0].aspect;

        frustumHeight = frustumWidth / camList[0].GetComponent<Camera>().aspect;

        camList[0].transform.position = new Vector3(-frustumWidth * 0.5f, frustumHeight * 0.5f, 0);
        camList[1].transform.position = new Vector3(frustumWidth * 0.5f, frustumHeight * 0.5f, 0);
        camList[2].transform.position = new Vector3(frustumWidth * 0.5f, -frustumHeight * 0.5f, 0);
        camList[3].transform.position = new Vector3(-frustumWidth * 0.5f, -frustumHeight * 0.5f, 0);

        SetDisplay();
    }

    private void SetDisplay()
    {
        Display.onDisplaysUpdated += OnDisplaysUpdated;
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Debug.Log(Display.displays.Length);
            camList[i].targetDisplay = i; //Set the Display in which to render the camera to
            Display.displays[i].Activate(); //Enable the display
        }
    }

    void OnDisplaysUpdated()
    {
        Debug.Log("New Display Connected. Show Display Option Menu....");
    }

    private void CreateVideoPanel()
    {
        GameObject panel = GameObject.Instantiate(VideoManager.Instance.Display);
        vp = panel.GetComponent<VideoPlayer>();

        SetVideoPanel();
    }

    private void SetVideoPanel()
    {
        vp.clip = VideoManager.Instance.Clip[0];

        vp.playOnAwake = true;
        vp.isLooping = true;

        SetVideoClip();
    }

    private void Connect()
    {
        if (!initFinished)
            return;

        if (connecting)
            return;

        byte error;

        connectionId = NetworkTransport.Connect(circleVRHostId, serverIP, serverPort, 0, out error);

        if (error != 0)
        {
            return;
        }

        connecting = true;
    }

    private bool ButtonState(string data)
    {
        if (data.Equals("Play"))
            vp.Play();
        else if (data.Equals("Pause"))
            vp.Pause();
        else if (data.Equals("Back"))
            vp.frame -= VideoManager.Instance.FrameVal;
        else if (data.Equals("Front"))
            vp.frame += VideoManager.Instance.FrameVal;
        else
            return false;

        return true;
    }

    private void SetVideoClip()
    {
        if (currentContentName.Equals(null))
            vp.clip = VideoManager.Instance.Clip[0];
        else if (currentContentName.Equals(VideoManager.Instance.ContentName1[0]))
            vp.clip = VideoManager.Instance.Clip[1];
        else if(currentContentName.Equals(VideoManager.Instance.ContentName1[1]))
            vp.clip = VideoManager.Instance.Clip[2];

        vp.Play();
    }

    private bool SetContentName(string msg)
    {
        if (msg.Equals(VideoManager.Instance.ContentName1[0]))
            contentName = VideoManager.Instance.ContentName1[0];
        else if(msg.Equals(VideoManager.Instance.ContentName1[1]))
            contentName = VideoManager.Instance.ContentName1[1];

        if (currentContentName.Equals(contentName))
            return false;

        currentContentName = contentName;

        return true;
    }

    public override void ManualUpdate()
    {
        if (!initFinished)
            return;

        if (!connected)
            Connect();

        base.ManualUpdate();
    }
    
    protected override void OnConnect(int connectionId, byte error)
    {
        base.OnConnect(connectionId, error);

        SendReliable(connectionId, "VideoPlayer");
    }
    
    protected override void OnData(int connectionId, int channelId, byte[] data, int size, byte error)
    {
        base.OnData(connectionId, channelId, data, size, error);

        string msg = Deserialize(data, size);

        if (ButtonState(msg))
            return;

        if (SetContentName(msg))
            return;

        SetVideoClip();
    }

    protected override void OnDisconnect(int connectionId, byte error)
    {
        base.OnDisconnect(connectionId, error);

        connected = false;
        connecting = false;

        currentContentName = null;
        SetVideoClip();
    }
}
