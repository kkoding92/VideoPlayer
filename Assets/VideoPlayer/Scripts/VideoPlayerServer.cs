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

    private void Start()
    {
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

    public void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        string msg = CircleVR.Deserialize(data, CircleVRProtocol.REC_BUFFER_SIZE);
        Debug.Log("server:" + msg);
        if (msg.Equals("VideoPlayer"))
        {
            checkVideo = true;
            conID = connectionId;
            CircleVR.SendDataReliable(conID, CircleVR.Instance.ContentName);
        }
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
            CircleVR.SendDataReliable(conID, "Play");
        else if (name.Equals("PauseBtn"))
            CircleVR.SendDataReliable(conID, "Pause");
        else if (name.Equals("backBtn"))
            CircleVR.SendDataReliable(conID, "Back");
        else
            CircleVR.SendDataReliable(conID, "Front");
    }
}
