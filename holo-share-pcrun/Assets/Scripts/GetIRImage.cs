// By LI Jiawei
// 2017.6.12

using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// This is for get Image and set texture.
/// </summary>
public class GetIRImage : MonoBehaviour, IInputClickHandler
{
    // 是否正在移动
    bool isMoving = false;

    bool haveNewMessage = false;

    Material mat;
    Texture2D tex;

    CustomMessagesIRImage myMessage;
    CustomMessages240 customMessage;

    byte[] imageData;

    private void Start()
    {
        mat = this.GetComponent<Renderer>().material;
        tex = new Texture2D(1, 1);

        //customMessage = CustomMessages240.Instance;
        myMessage = CustomMessagesIRImage.Instance;

        // 增加一个Callback，触发这个的时候就触发下面的函数
        customMessage.MessageHandlers[CustomMessages240.CustomMessageID.CubePostion] = OnCubePositionReceived;
        myMessage.MessageHandlers[CustomMessagesIRImage.CustomMessageID.IRImage] = OnIRImageReceived;
    }

    private void OnIRImageReceived(NetworkInMessage msg)
    {
        imageData = CustomMessagesIRImage.ReadIRImage(msg);
        haveNewMessage = true;
    }

    private void OnCubePositionReceived(NetworkInMessage msg)
    {
        if (!isMoving)
        {
            transform.position = CustomMessages240.ReadCubePostion(msg);
        }
    }

    // 单击Cube，切换是否移动
    public void OnInputClicked(InputClickedEventData eventData)
    {
        isMoving = !isMoving;
        if (!isMoving)
        {
            customMessage.SendCubePosition(transform.position);
        }
    }

    // 如果Cube为移动状态，让其放置在镜头前2米位置
    void Update()
    {
        if (haveNewMessage)
        {
            haveNewMessage = false;
            tex.LoadImage(imageData);
            tex.Apply();
            mat.SetTexture("_MainTex", tex);
        }
    }
}