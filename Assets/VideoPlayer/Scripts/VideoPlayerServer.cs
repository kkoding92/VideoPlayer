using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerServer : MonoBehaviour, ICircleVRTransportEventHandler
{
    [SerializeField] private GameObject HostUI;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button pauseBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button frontBtn;

    private int conID = -1;
    private bool checkVideo = false;
    private CircleVR circleVR;

    private void Start()
    {
        circleVR = GameObject.Find("CircleVR").GetComponent<CircleVR>();
        playBtn.onClick.AddListener(delegate { MenuButton(playBtn.name); });
        pauseBtn.onClick.AddListener(delegate { MenuButton(pauseBtn.name); });
        backBtn.onClick.AddListener(delegate { MenuButton(backBtn.name); });
        backBtn.onClick.AddListener(delegate { MenuButton(frontBtn.name); });
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
        if(conID == connectionId)
        {
            checkVideo = false;
            conID = -1;
        }
    }

    public void OnManualUpdate()
    {
        if (Input.GetKeyDown("1"))
            HostUI.SetActive(!HostUI.activeSelf);
    }

    private void MenuButton(string name)
    {
        if (name.Equals("PlayBtn"))
            SendReliable((Byte)VideoPlayerPacket.Play);
        else if (name.Equals("PauseBtn"))
            SendReliable((Byte)VideoPlayerPacket.Pause);
        else if (name.Equals("backBtn"))
            SendReliable((Byte)VideoPlayerPacket.Back);
        else
            SendReliable((Byte)VideoPlayerPacket.Front);
    }

    public void OnData(byte key, byte[] data)
    {
        VideoPlayerPacket type = (VideoPlayerPacket)key;

        if(type == VideoPlayerPacket.VideoPlayer)
        {
            checkVideo = true;
            SendReliable((Byte)VideoPlayerPacket.Name, StringToByte(circleVR.ContentName));
        }
    }

    public void SendReliable(byte key, byte[] data)
    {
    }

    public void SendUnreliable(byte key, byte[] data)
    {
    }

    public void SendStateUpdate(byte key, byte[] data)
    {
    }

    public void SendReliable(byte key)
    {
    }

    public void SendUnreliable(byte key)
    {
    }

    public void SendStateUpdate(byte key)
    {
    }

    public void SendReliable(int connectionId, byte key, byte[] data)
    {
    }

    public void SendUnreliable(int connectionId, byte key, byte[] data)
    {
    }

    public void SendStateUpdate(int connectionId, byte key, byte[] data)
    {
    }

    public void SendReliable(int connectionId, byte key)
    {
    }

    public void SendUnreliable(int connectionId, byte key)
    {
    }

    public void SendStateUpdate(int connectionId, byte key)
    {
    }

    public string ByteToString(byte[] strByte)
    {
        throw new NotImplementedException();
    }

    public byte[] StringToByte(string str)
    {
        throw new NotImplementedException();
    }
}
