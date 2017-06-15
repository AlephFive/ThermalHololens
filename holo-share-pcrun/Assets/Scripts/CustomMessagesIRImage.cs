using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System;

public class CustomMessagesIRImage : Singleton<CustomMessagesIRImage>
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
        // 在 SharingManagerConnected 里增加
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

        // 根据每一个customeMessageID 添加监听器
        for (byte index = (byte)MessageID.UserMessageIDStart; index < (byte)CustomMessageID.Max; ++index)
        {
            serverConnection.AddListener(index, connectionAdapter);
        }
    }

    private void ConnectionAdapter_MessageReceivedCallback(NetworkConnection connection, NetworkInMessage msg)
    {
        byte messageType = msg.ReadByte();
        // 在字典里查询这个messageType
        MessageCallback messageHandler = MessageHandlers[(CustomMessageID)messageType]; //?????
        if (messageHandler != null)
        {
            // 用对应类型的messageHandler（MessageCallback）处理这个msg
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
    
    /// <summary>
    /// 第一位是message Type
    /// 第二位是LocalUserID
    /// 这个不用改，所有的消息都是这样的
    /// </summary>
    /// <param name="messageType"></param>
    /// <returns></returns>
    private NetworkOutMessage CreateMessage(byte messageType)
    {
        NetworkOutMessage msg = serverConnection.CreateMessage(messageType);
        msg.Write(messageType);
        msg.Write(LocalUserID);
        return msg; 
    }

    /// <summary>
    /// 广播cube的message
    /// </summary>
    /// <param name="positon"></param>
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

    /// <summary>
    /// 发送image的array
    /// </summary>
    /// <param name="imageBytes"></param>
    public void SendIRImage(byte[] imageBytes)
    {
        if (serverConnection != null && serverConnection .IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)CustomMessageID.IRImage);

            msg.Write(imageBytes.Length);

            for (int i = 0; i < imageBytes.Length; i++)
            {
                msg.Write(imageBytes[i]);
            }

            serverConnection.Broadcast(msg, MessagePriority.Immediate, MessageReliability.UnreliableSequenced, MessageChannel.Default);
        }
    }

    public void SendIRImageByString(byte[] imageBytes)
    {
        if (serverConnection != null && serverConnection.IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)CustomMessageID.IRImage);

            msg.Write(imageBytes.Length);

            XString tempString = new XString(System.Text.Encoding.UTF8.GetString(imageBytes));

            msg.Write(tempString);

            serverConnection.Broadcast(msg, MessagePriority.Immediate, MessageReliability.UnreliableSequenced, MessageChannel.Default);
        }
    }

    public void SendIRImageByArray(byte[] imageBytes)
    {
        if (serverConnection != null && serverConnection.IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)CustomMessageID.IRImage);

            msg.Write(imageBytes.Length);

            msg.WriteArray(imageBytes, Convert.ToUInt32(imageBytes.Length));

            serverConnection.Broadcast(msg, MessagePriority.Immediate, MessageReliability.ReliableOrdered, MessageChannel.Default);
        }
    }

    public void SendIRImageByLinescan(byte[] imageBytes, Int32 scanIndex)
    {
        if (serverConnection != null && serverConnection.IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)CustomMessageID.IRImage);

            //send the current scan index
            msg.Write(scanIndex);

            //send length data
            msg.Write(imageBytes.Length);
            
            //send image
            msg.WriteArray(imageBytes, Convert.ToUInt32(imageBytes.Length));

            serverConnection.Broadcast(msg, MessagePriority.Immediate, MessageReliability.ReliableOrdered, MessageChannel.Default);
        }
    }

    /// <summary>
    /// 读取 msg 里的bytes
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static byte[] ReadIRImage(NetworkInMessage msg)
    {
        byte[] tempImage;
        int length = 0;
        msg.ReadInt64();
        length = msg.ReadInt32();
        tempImage = new byte[length];
        for (int i = 0; i < length; i++)
        {
            tempImage[i] = msg.ReadByte();
        }
        return tempImage;
    }

    public static byte[] ReadIRImageByString(NetworkInMessage msg)
    {
        byte[] tempImage;
        msg.ReadInt64();
        msg.ReadInt32();

        tempImage = System.Convert.FromBase64String(msg.ReadString());

        return tempImage;
    }

    public static byte[] ReadIRImageByArray(NetworkInMessage msg)
    {
        byte[] tempImage;
        int length = 0;
        msg.ReadInt64();
        length = msg.ReadInt32();
        tempImage = new byte[length];

        msg.ReadArray(tempImage, Convert.ToUInt32(length));

        return tempImage;
    }

    public static Vector3 ReadCubePostion(NetworkInMessage msg)
    {
        msg.ReadInt64();

        return new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
    }
}
