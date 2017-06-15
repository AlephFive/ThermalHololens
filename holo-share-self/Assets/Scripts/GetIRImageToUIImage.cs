// By LI Jiawei
// 2017.6.12

using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is for get Image and set texture.
/// </summary>
public class GetIRImageToUIImage : MonoBehaviour, IInputClickHandler
{
    // 是否正在移动
    bool isMoving = false;

    bool haveNewMessage = false;

    Material mat;
    Texture2D tex;

    CustomMessagesIRImage myMessage;
    CustomMessages240 customMessage;

    byte[] imageData;

    TextMesh myTextMesh;
    public GameObject debugGOJ;
    float jiangeTime;
    float countStartTime;

    int counter = 0;

    Sprite mySprite;
    Image myImage;
    RawImage myRawImage;

    float textureJiangeTime = 1f / 20f;
    float textureLastTime = 0f;

    private void Start()
    {
        myRawImage = this.GetComponent<RawImage>();

        myTextMesh = debugGOJ.GetComponent<TextMesh>();

        tex = new Texture2D(256, 256);

        //customMessage = CustomMessages240.Instance;
        myMessage = CustomMessagesIRImage.Instance;

        // 增加一个Callback，触发这个的时候就触发下面的函数
        //customMessage.MessageHandlers[CustomMessages240.CustomMessageID.CubePostion] = OnCubePositionReceived;
        myMessage.MessageHandlers[CustomMessagesIRImage.CustomMessageID.IRImage] = OnIRImageReceived;
    }

    private void OnIRImageReceived(NetworkInMessage msg)
    {
        countStartTime = Time.time;

        //imageData = CustomMessagesIRImage.ReadIRImage(msg);
        //imageData = CustomMessagesIRImage.ReadIRImageByString(msg);
        imageData = CustomMessagesIRImage.ReadIRImageByArray(msg);

        haveNewMessage = true;
        jiangeTime = Time.time - countStartTime;
        //Debug.Log("接收消息处理时间为：" + jiangeTime.ToString("F2"));
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
            myTextMesh.text = (counter++).ToString();
        }

        //UpdataTexture();
    }

    /// <summary>
    /// 按照控制的时间频率来更新texture
    /// </summary>
    private void UpdataTexture()
    {
        if (Time.time - textureLastTime > textureJiangeTime)
        {
            textureLastTime = Time.time;
            LoadDataToTexture();
        }
    }

    private void LoadDataToTexture()
    {
        tex.LoadImage(imageData);
        //tex.Apply();
        //mat.SetTexture("_MainTex", tex);

        myRawImage.texture = tex;
    }
}