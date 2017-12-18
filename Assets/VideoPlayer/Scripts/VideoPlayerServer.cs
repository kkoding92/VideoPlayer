using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerServer : CircleVRServerHandler
{
    [SerializeField] private GameObject HostUI;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button pauseBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button frontBtn;

    private CircleVR circleVR;

    private void Start()
    {
        playBtn.onClick.AddListener(delegate { MenuButton(playBtn.name); });
        pauseBtn.onClick.AddListener(delegate { MenuButton(pauseBtn.name); });
        backBtn.onClick.AddListener(delegate { MenuButton(backBtn.name); });
        backBtn.onClick.AddListener(delegate { MenuButton(frontBtn.name); });
        HostUI.SetActive(false);

        circleVR = GameObject.Find("CircleVR").GetComponent<CircleVR>();
    }
    
    public override void OnManualUpdate()
    {
        base.OnManualUpdate();

        if (Input.GetKeyDown("1"))
            HostUI.SetActive(!HostUI.activeSelf);
    }

    private void MenuButton(string name)
    {
        if (name.Equals("PlayBtn"))
            SendReliable("Play");
        else if (name.Equals("PauseBtn"))
            SendReliable("Pause");
        else if (name.Equals("backBtn"))
            SendReliable("Back");
        else
            SendReliable("Front");
    }

    public override void OnData(string data)
    {
        if (data.Equals("VideoPlayer"))
        {
            SendReliable(circleVR.ContentName);
        }
    }

    public override void OnRequestContentData(out string contentServerData)
    {
        throw new NotImplementedException();
    }
}
