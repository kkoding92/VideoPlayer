using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

public class Server : MonoBehaviour {
    private int connectionID;
    private int maxConnections = 5;
    private int reliableChannelID;
    private int hostID;
    private int socketPort = 8888;
    private byte error;

    private List<int> players = new List<int>();

    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.ReliableSequenced);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);
    }

    void Update ()
    {
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
                players.Add(recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                break;
            case NetworkEventType.DisconnectEvent:
                break;
        }
	}

    public void sendMessage(string message)
    {
        Debug.Log("server send: " + message);
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        foreach(int id in players)
            NetworkTransport.Send(hostID, id, reliableChannelID, buffer, message.Length * sizeof(char), out error);
    }
    
    public void Play()
    {
        sendMessage("Play|");
    }

    public void Pause()
    {
        sendMessage("Pause|");
    }

    public void Sand()
    {
        sendMessage("Change|"+"0");
    }

    public void Baekje()
    {
        sendMessage("Change|"+"1");
    }

    public void fameMove(string temp){
        sendMessage("Frame|"+ temp);
    }
}
