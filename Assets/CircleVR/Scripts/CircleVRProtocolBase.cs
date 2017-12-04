using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public abstract class CircleVRProtocolBase
{
    public static CircleVREventHandler Delegate;

    protected readonly Vector3 CENTER_ANCHOR_POSITION = new Vector3(0.0f, 0.0f, 0.1f);
    protected readonly Vector3 CENTER_ANCHOR_EULER = new Vector3(90.0f, 0.0f, 0.0f);

    private const int REC_BUFFER_SIZE = 1024;

    protected int hostID;
    protected int reliableChannel;
    protected int unreliableChannel;

    public abstract void Init(Configuration config);

    private HostTopology TransportInit(out int reliableChannelID, out int unreliableChannelID, int maxConnection)
    {
        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();
        connectionConfig.FragmentSize = 1000;
        connectionConfig.PacketSize = 1470;
        reliableChannelID = connectionConfig.AddChannel(QosType.Reliable);
        unreliableChannelID = connectionConfig.AddChannel(QosType.StateUpdate);
        return new HostTopology(connectionConfig, maxConnection);
    }

    protected void CreateHost(out int hostID, out int reliableChannelID, out int unreliableChannelID, int maxConnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(out reliableChannelID, out unreliableChannelID, maxConnection));
 
    }

    protected void CreateHost(out int hostID, out int reliableChannelID, out int unreliableChannelID, int port, int maxConnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(out reliableChannelID, out unreliableChannelID, maxConnection), port);
    }

    protected void SendData(int hostId, string str, int connectionId, int channelId)
    {
        byte error;
        byte[] buffer = StringToByte(str);

        Debug.Assert(sizeof(byte) * buffer.Length <= REC_BUFFER_SIZE);

        NetworkTransport.Send(hostId, connectionId, channelId, buffer, buffer.Length, out error);

        //Debug.Log("Send : " + str + "\n" + "BufferSize : " + buffer.Length);
    }

    protected string Deserialize(byte[] buffer , int recBufferSize)
    {
        byte[] recSizeBuffer = new byte[recBufferSize];

        for (int i = 0; i < recBufferSize; i++)
        {
            recSizeBuffer[i] = buffer[i];
        }

        return ByteToString(recSizeBuffer);
    }

    protected string ByteToString(byte[] strByte)
    {
        string str = Encoding.UTF8.GetString(strByte);

        return str;
    }

    private byte[] StringToByte(string str)
    {
        byte[] strByte = Encoding.UTF8.GetBytes(str);

        return strByte;
    }

    public virtual void ManualUpdate()
    {
        int outHostId;
        int outConnectionId;
        int outChannelId;
        int outDataSize;

        byte[] buffer = new byte[REC_BUFFER_SIZE];

        byte error;

        NetworkEventType networkEvent = NetworkEventType.DataEvent;

        do
        {
            networkEvent = NetworkTransport.ReceiveFromHost(hostID, out outConnectionId, out outChannelId, buffer, REC_BUFFER_SIZE, out outDataSize, out error);
             
            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    OnConnect(hostID, outConnectionId, error);
                    break;

                case NetworkEventType.DataEvent:
                    OnData(hostID, outConnectionId, outChannelId, buffer, outDataSize, error);
                    break;

                case NetworkEventType.DisconnectEvent:
                    OnDisconnect(hostID, outConnectionId, error);
                    break;
            }

        } while (networkEvent != NetworkEventType.Nothing);
    }

    protected virtual void OnConnect(int hostId, int connectionId, byte error)
    {
        Debug.Log("OnConnect(hostId = " + hostId + ", connectionId = "
            + connectionId + ", error = " + error.ToString() + ")");
    }

    protected virtual void OnDisconnect(int hostId, int connectionId, byte error)
    {
        Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = "
            + connectionId + ", error = " + error.ToString() + ")");
    }

    protected virtual void OnBroadcast(int hostId, byte[] data, int size, byte error)
    {
        Debug.Log("OnBroadcast(hostId = " + hostId + ", data = "
            + data + ", size = " + size + ", error = " + error.ToString() + ")");
    }

    protected virtual void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        //Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = "
        //    + connectionId + ", channelId = " + channelId + ", data = "
        //    + data + ", size = " + size + ", error = " + ((NetworkError)error).ToString() + ")");
    }
}
