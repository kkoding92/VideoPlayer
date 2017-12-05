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
    private List<Connection> connections = new List<Connection>();
    private List<GameObject> camList = new List<GameObject>();
    private bool initFinished;

    public override void Init(Configuration config)
    {
        CreateHost(out hostID, out unreliableChannel, out reliableChannel, config.serverPort, CircleVR.MAX_CLIENT_COUNT);
        
        CreateCameras();
        CreateVideoPanel();

        if (Delegate != null)
            Delegate.OnHostInit();
        
        initFinished = true;
    }
    
    private void CreateCameras(){
        for(int i=0; i< 4; i++){
            GameObject camObj = GameObject.Instantiate(CircleVR.Instance.Cam);
            camList.Add(camObj);
        }
        SetCamera();
    }
    private void SetCamera(){
        float frustumHeight = 2.0f * 2.0f* Mathf.Tan(camList[0].GetComponent<Camera>().fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * camList[0].GetComponent<Camera>().aspect;
        frustumHeight = frustumWidth / camList[0].GetComponent<Camera>().aspect;
        
        camList[0].transform.position = new Vector3(-frustumWidth * 0.5f, frustumHeight * 0.5f, 0);
        camList[1].transform.position = new Vector3(frustumWidth * 0.5f, frustumHeight * 0.5f, 0);
        camList[2].transform.position = new Vector3(frustumWidth * 0.5f, -frustumHeight * 0.5f, 0);
        camList[3].transform.position = new Vector3(-frustumWidth * 0.5f, -frustumHeight * 0.5f, 0);
    }

    private void CreateVideoPanel(){
        GameObject panelObj = GameObject.Instantiate(CircleVR.Instance.Display);
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
            //SendData(hostID, JsonUtility.ToJson(datas), connection.id, unreliableChannel);
        }
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();
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