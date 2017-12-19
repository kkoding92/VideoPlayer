using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;


public class CircleVRUI : MonoBehaviour
{
    private static CircleVRUI instance = null;

    public static CircleVRUI Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<CircleVRUI>();

            Debug.Assert(instance);

            return instance;
        }
    }

    public ICircleVRUIEventHandler Delegate;

    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject hostUI;
    [SerializeField] private GameObject clientUI;
    [SerializeField] private Text log;
    [SerializeField] private Text frame;

    //Host UI
    [SerializeField] private Image client1;
    [SerializeField] private Image client2;
    [SerializeField] private Image client3;
    [SerializeField] private Image client4;

    [SerializeField] private Text position;
    [SerializeField] private Text oriented;

    public string Log
    {
        get
        {
            return log.text;
        }

        set
        {
            StringBuilder stringBuilder = new StringBuilder(log.text);
            stringBuilder.Append(value);
            log.text = stringBuilder.ToString();
        }
    }
    public string Position
    {
        get
        {
            return position.text;
        }

        set
        {
            position.text = value;
        }
    }
    public string Oriented
    {
        get
        {
            return oriented.text;
        }

        set
        {
            oriented.text = value;
        }
    }

    public GameObject HostUI
    {
        get
        {
            return hostUI;
        }

        set
        {
            hostUI = value;
        }
    }
    public GameObject ClientUI
    {
        get
        {
            return clientUI;
        }

        set
        {
            clientUI = value;
        }
    }

    public void SetClientSlot(int userId, ClientState state)
    {
        Debug.Assert(userId >= 0 && userId < CircleVR.MAX_CLIENT_COUNT);

        switch (userId)
        {
            case 0:
                SetSlotColor(client1, state);
                return;

            case 1:
                SetSlotColor(client2, state);
                return;

            case 2:
                SetSlotColor(client3, state);
                return;

            case 3:
                SetSlotColor(client4, state);
                return;
        }
    }

    private void SetSlotColor(Image slot, ClientState state)
    {
        if (state == ClientState.Connected)
        {
            slot.color = Color.green;
            return;
        }

        slot.color = Color.red;
    }

    private void SetClientSlotCount(int count)
    {
        Debug.Assert(count > 0 && count <= 4);

        switch (count)
        {
            case 1:
                client2.gameObject.SetActive(false);
                client3.gameObject.SetActive(false);
                client4.gameObject.SetActive(false);
                return;

            case 2:
                client3.gameObject.SetActive(false);
                client4.gameObject.SetActive(false);
                return;

            case 3:
                client4.gameObject.SetActive(false);
                return;
        }
    }

    public void OnPlayButtonClick()
    {
        if(Delegate != null)
            Delegate.OnPlay();
    }

    public void OnStopButtonClick()
    {
        if (Delegate != null)
            Delegate.OnStop();
    }

    public void OnPauseButtonClick()
    {
        if (Delegate != null)
            Delegate.OnPause();
    }

    private void Awake()
    {
        SetClientSlotCount(CircleVR.MAX_CLIENT_COUNT);
        canvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown("`"))
            canvas.SetActive(!canvas.activeSelf);

        if (canvas.activeSelf)
            FrameRateUpdate();
    }

    private void FrameRateUpdate()
    {
        //TODO : FrameRate update
    }
}
