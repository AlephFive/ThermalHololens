using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System;

public class CustomMessages240 : Singleton<CustomMessages240>
{
    NetworkConnection serverConnection;

    NetworkConnectionAdapter connectionAdapter;
    
    // need custom
    public enum CustomMessageID : byte
    {
        CubePostion = MessageID.UserMessageIDStart,
        IRImage = MessageID.UserMessageIDStart + 1,
        Max
    }

    public delegate void MessageCallback(NetworkInMessage msg);

    public Dictionary<CustomMessageID, MessageCallback> MessageHandlers { get; private set; }

    public long LocalUserID { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        MessageHandlers = new Dictionary<CustomMessageID, MessageCallback>();
        for (byte index = (byte)MessageID.UserMessageIDStart; index < (byte)CustomMessageID.Max; index++)
        {
            if (!MessageHandlers.ContainsKey((CustomMessageID)index))
            {
                MessageHandlers.Add((CustomMessageID)index, null);
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        SharingStage.Instance.SharingManagerConnected += Instance_SharingManagerConnected;
    }

    private void Instance_SharingManagerConnected(object sender, EventArgs e)
    {
        InitializeMessageHandlers();
    }

    private void InitializeMessageHandlers()
    {
        SharingStage sharingStage = SharingStage.Instance;

        if (sharingStage == null)
        {
            return;
        }

        serverConnection = sharingStage.Manager.GetServerConnection();
        if (serverConnection == null)
        {
            return;
        }

        connectionAdapter = new NetworkConnectionAdapter();
        connectionAdapter.MessageReceivedCallback += ConnectionAdapter_MessageReceivedCallback;

        LocalUserID = sharingStage.Manager.GetLocalUser().GetID();

        for (byte index = (byte)MessageID.UserMessageIDStart; index < (byte)CustomMessageID.Max; ++index)
        {
            serverConnection.AddListener(index, connectionAdapter);
        }
    }

    private void ConnectionAdapter_MessageReceivedCallback(NetworkConnection connection, NetworkInMessage msg)
    {
        byte messageType = msg.ReadByte();
        MessageCallback messageHandler = MessageHandlers[(CustomMessageID)messageType]; //?????
        if (messageHandler != null)
        {
            messageHandler(msg);
        }
    }

    protected override void OnDestroy()
    {
        if (serverConnection != null)
        {
            for (byte index = (byte)MessageID.UserMessageIDStart; index < (byte)CustomMessageID.Max; ++index)
            {
                serverConnection.RemoveListener(index, connectionAdapter);
            }
            connectionAdapter.MessageReceivedCallback -= ConnectionAdapter_MessageReceivedCallback;
        }
        base.OnDestroy();
    }

    private NetworkOutMessage CreateMessage(byte messageType)
    {
        NetworkOutMessage msg = serverConnection.CreateMessage(messageType);
        msg.Write(messageType);
        msg.Write(LocalUserID);
        return msg;
    }

    public void SendCubePosition(Vector3 positon)
    {
        if (serverConnection != null && serverConnection.IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)CustomMessageID.CubePostion);

            msg.Write(positon.x);
            msg.Write(positon.y);
            msg.Write(positon.z);

            serverConnection.Broadcast(msg, MessagePriority.Immediate, MessageReliability.ReliableOrdered, MessageChannel.Default);
        }
    }

    public static Vector3 ReadCubePostion(NetworkInMessage msg)
    {
        msg.ReadInt64();

        return new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
    }
}
