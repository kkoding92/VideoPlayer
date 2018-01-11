using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerServer : CircleVRServerHandler
{
    [SerializeField] private GameObject HostUI;

    private int conID = -1;
    private bool checkVideo = false;
    private CircleVR circleVR;

    private void Start()
    {
        circleVR = GameObject.Find("CircleVR").GetComponent<CircleVR>();
        HostUI.SetActive(false);
    }

    public void OnConnect(int hostId, int connectionId, byte error)
    {
        if (!checkVideo)
            return;

        if (conID < 0)
            return;
    }

    public void OnDisConnect(int hostId, int connectionId, byte error)
    {
        if (conID == connectionId)
        {
            checkVideo = false;
            conID = -1;
        }
    }

    public override void OnManualUpdate()
    {
        base.OnManualUpdate();
        if (Input.GetKeyDown("1"))
            HostUI.SetActive(!HostUI.activeSelf);
    }

    public override void OnData(byte key, byte[] data)
    {
        VideoPlayerPacket type = (VideoPlayerPacket)key;

        if (type == VideoPlayerPacket.VideoPlayer)
        {
            checkVideo = true;
            SendReliable((Byte)VideoPlayerPacket.Name, StringToByte(circleVR.ContentName));
        }
    }

    public override void OnRequestContentData(int connectionId)
    {
    }
}
