using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class CircleVRHost : CircleVRProtocolBase
{
    public class Connection
    {
        public int id;
        public int userId;
        public string trackerId;
    }
    
    private HostData datas = new HostData();

    private List<Connection> connections = new List<Connection>();

    private Camera[] clientCams;

    private bool initFinished;

    public override void Init(Configuration config)
    {
        CreateHost(out hostID, out unreliableChannel, out reliableChannel, config.serverPort, CircleVR.MAX_CLIENT_COUNT);
        
        if (Delegate != null)
            Delegate.OnHostInit();
        
        initFinished = true;
    }
    
    private Connection GetConnectionByConnectionID(int connectionId)
    {
        foreach (Connection connection in connections)
        {
            if (connection.id == connectionId)
                return connection;
        }
        return null;
    }

    private bool HasTrackerID(string trackerId)
    {
        foreach(Connection connection in connections)
        {
            if (connection.trackerId == trackerId)
                return true;
        }

        return false;
    }

    private bool TryCreateConnection(int connectionId, ClientData data)
    {
        //if (HasUserID(data.userId, connections))
        //{
        //    return false;
        //}

        Connection connection = GetConnectionByConnectionID(connectionId);

        if (connection != null)
        {
            return false;
        }

        connection = new Connection();
        connection.id = connectionId;
        connection.userId = data.userId;
        connection.trackerId = data.trackerId;

        connections.Add(connection);
        
        SendData(hostID, "Connected", connectionId, reliableChannel);
        return true;
    }

    private void Disconnect(int hostId , int connectionId , byte error)
    {
        if (!NetworkTransport.Disconnect(hostId, connectionId, out error))
        {
            Debug.LogError(((NetworkError)error).ToString());
        }

        DestroyConnection(connectionId , hostId);
    }

    private void DestroyConnection(int connectionId, int recHostId)
    { 
        Debug.Log("Player " + connectionId.ToString() + " disconnected!");

        Connection connection = GetConnectionByConnectionID(connectionId);
        connections.Remove(connection);
    }

    private void SendPosition()
    {
        foreach (Connection connection in connections)
        {
            SendData(hostID, JsonUtility.ToJson(datas), connection.id, unreliableChannel);
        }
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (initFinished && connections.Count > 0)
            SendPosition();
    }

    protected override void OnDisconnect(int hostId, int connectionId, byte error)
    {
        base.OnDisconnect(hostId, connectionId, error);

        DestroyConnection(connectionId, hostId);
    }

    protected override void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        base.OnData(hostId, connectionId, channelId, data, size, error);

        string strData = Deserialize(data ,size);
        ClientData clientData = JsonUtility.FromJson<ClientData>(strData);

        if (TryCreateConnection(connectionId, clientData))
            return;

        SendData(hostID, "Failed", connectionId, reliableChannel);

        Disconnect(hostId , connectionId ,error);
    }
}