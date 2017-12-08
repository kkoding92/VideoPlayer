
using UnityEngine;
using UnityEngine.Networking;

public abstract class CircleVRTransportBase
{
    public readonly int circleVRHostId;
    public readonly int reliableChannel;
    public readonly int unreliableChannel;

    protected ICircleVRTransportEventHandler eventHandler;

    public CircleVRTransportBase(ICircleVRTransportEventHandler eventHandler , int maxConnection)
    {
        this.eventHandler = eventHandler;
        CreateHost(out circleVRHostId, out reliableChannel, out unreliableChannel, maxConnection);
    }

    public CircleVRTransportBase(ICircleVRTransportEventHandler eventHandler, int maxConnection, int port)
    {
        this.eventHandler = eventHandler;
        CreateHost(out circleVRHostId, out reliableChannel, out unreliableChannel, port, maxConnection);
    }

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

    private void CreateHost(out int hostID, out int reliableChannelID, out int unreliableChannelID, int maxConnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(out reliableChannelID, out unreliableChannelID, maxConnection));
 
    }

    private void CreateHost(out int hostID, out int reliableChannelID, out int unreliableChannelID, int port, int maxConnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(out reliableChannelID, out unreliableChannelID, maxConnection), port);
    }

    public virtual void ManualUpdate()
    {
        int outConnectionId;
        int outChannelId;
        int outDataSize;

        byte[] buffer = new byte[CircleVRProtocol.REC_BUFFER_SIZE];

        byte error;

        NetworkEventType networkEvent = NetworkEventType.DataEvent;

        do
        {
            networkEvent = NetworkTransport.ReceiveFromHost(circleVRHostId, out outConnectionId, out outChannelId, buffer, CircleVRProtocol.REC_BUFFER_SIZE, out outDataSize, out error);
             
            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    OnConnect(circleVRHostId, outConnectionId, error);
                    if(eventHandler != null)
                        eventHandler.OnConnect(circleVRHostId, outConnectionId, error);
                    break;

                case NetworkEventType.DataEvent:
                    OnData(circleVRHostId, outConnectionId, outChannelId, buffer, outDataSize, error);
                    if (eventHandler != null)
                        eventHandler.OnData(circleVRHostId, outConnectionId, outChannelId, buffer, outDataSize, error);
                    break;

                case NetworkEventType.DisconnectEvent:
                    OnDisconnect(circleVRHostId, outConnectionId, error);
                    if (eventHandler != null)
                        eventHandler.OnDisConnect(circleVRHostId, outConnectionId, error);
                    break;
            }

        } while (networkEvent != NetworkEventType.Nothing);
    }

    protected virtual void OnConnect(int hostId, int connectionId, byte error)
    {
        Debug.Log("OnConnect(hostId = " + hostId + ", connectionId = "
            + connectionId + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    protected virtual void OnDisconnect(int hostId, int connectionId, byte error)
    {
        Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = "
            + connectionId + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    protected virtual void OnBroadcast(int hostId, byte[] data, int size, byte error)
    {
        Debug.Log("OnBroadcast(hostId = " + hostId + ", data = "
            + data + ", size = " + size + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    protected virtual void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, byte error)
    {
        //Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = "
        //    + connectionId + ", channelId = " + channelId + ", data = "
        //    + data + ", size = " + size + ", error = " + ((NetworkError)error).ToString() + ")");
    }
}

