using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

public class Client : MonoBehaviour {

    private int connectionID;
    private int maxConnections = 5;
    private int reliableChannelID;
    private int hostID;
    private int socketPort = 8888;
    private byte error;
    private DisplayManager _disManager;
   
    [SerializeField] private string _ipAddress;

	void Start () {
        NetworkTransport.Init();
        Connect();
        _disManager = GameObject.Find("DisplayMAnager").GetComponent<DisplayManager>();
	}
	
	void Update () {
        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int datasize;
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out datasize, out error);
        switch (recNetworkEvent)
        {
            case NetworkEventType.ConnectEvent:
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, datasize);
                Debug.Log("Receving: " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    case "Play":
                        PlayFun();
                        break;
                    case "Pause":
                         PauseFun();
                         break;
                    case "Change":
                        ChangeFun(splitData[1]);
                        break;
                    case "Frame":
                        Frame(splitData[1]);
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent:
                break;
        }
    }

    public void Connect()
    {
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.ReliableSequenced);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, 0);
        connectionID = NetworkTransport.Connect(hostID, _ipAddress, socketPort, 0, out error);
    }

    public void DisConnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }

    public void sendMessage(string message)
    {
        Debug.Log("client send: " + message);
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, message.Length * sizeof(char), out error);
    }
    private void PlayFun()
    {
        _disManager._vp.Play();
    }
    private void PauseFun()
    {
        _disManager._vp.Pause();
    }
    private void ChangeFun(string temp)
    {
        int index = int.Parse(temp);
        _disManager.ChangeVideo(index);
    }

    private void Frame(string temp){
        if(temp.Equals("b")){
            _disManager._vp.frame -= _disManager.FrameVal;
        }else if(temp.Equals("f")){
            _disManager._vp.frame += _disManager.FrameVal;
        }
    }
}
