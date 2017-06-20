// By LI Jiawei
// 2017.6.12

using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity.InputModule;
using U3D.Threading.Tasks;


using UnityEngine;
using UnityEngine.UI;

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
    Texture2D lineTex;
    Color[] lineColors;
    int scanIndex = 0;

    TextMesh myTextMesh;
    public GameObject debugGOJ;
    float jiangeTime;
    float countStartTime;

    int counter = 0;

    float textureJiangeTime = 1f / 30f;
    float textureLastTime = 0f;

    float screenX = 0;
    float screenY = 0;

    
    Vector3 moveVec = new Vector3(0, -1.16f / 120, 0);
    Vector3 initPos = new Vector3(0, 0.58f, 5);









    private void Start()
    {
        myTextMesh = debugGOJ.GetComponent<TextMesh>();

        mat = this.GetComponent<Renderer>().material;
        tex = new Texture2D(256, 256);
        lineTex = new Texture2D(256, 20);
        lineColors = new Color[5121];
        //customMessage = CustomMessages240.Instance;
        myMessage = CustomMessagesIRImage.Instance;

        // 增加一个Callback，触发这个的时候就触发下面的函数
        //customMessage.MessageHandlers[CustomMessages240.CustomMessageID.CubePostion] = OnCubePositionReceived;
        myMessage.MessageHandlers[CustomMessagesIRImage.CustomMessageID.IRImage] = OnIRImageReceived;

        screenX = GameObject.Find("Plane").transform.localPosition.x;
        screenY = GameObject.Find("Plane").transform.localPosition.y;
        
    }

    private void OnIRImageReceived(NetworkInMessage msg)
    {
        countStartTime = Time.time;

        //imageData = CustomMessagesIRImage.ReadIRImage(msg);
        //imageData = CustomMessagesIRImage.ReadIRImageByString(msg);
        //imageData = CustomMessagesIRImage.ReadIRImageByArray(msg);

        //imageData = CustomMessagesIRImage.ReadIRImageByLinescan(msg);

#if HASTASKS
    imageData = CustomMessagesIRImage.ReadIRImageByLinescanAsync(msg);

#else
    imageData = CustomMessagesIRImage.ReadIRImageByLinescan(msg);
#endif

        scanIndex = CustomMessagesIRImage.scanIndex;
        Debug.Log("scanIndex is " + scanIndex);
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
            UpdataTexture();
        }
        

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
            //LoadLinescanDataToTexture();
        }
    }

    private void LoadDataToTexture()
    {
        tex.LoadImage(imageData);
        //tex.LoadRawTextureData(imageData);

        Debug.Log("image loaded");

        tex.Apply();
        mat.SetTexture("_MainTex", tex);
        

        
            Vector3 calculatedPos = new Vector3(0, 0.60f + scanIndex*(-1.16f / 94), 5);
            this.gameObject.transform.position = calculatedPos;



            //this.gameObject.transform.Translate(moveVec, Space.World);
        
        
    }

    private void LoadLinescanDataToTexture() {
        lineTex.LoadImage(imageData);
        tex.LoadImage(imageData);
        tex.Apply();
        lineTex.Apply();
        lineColors = lineTex.GetPixels(0, 0, lineTex.width, 5);
        

        

        tex.SetPixels(0, scanIndex, tex.width, 5, lineColors);
        tex.Apply();
        mat.SetTexture("_MainTex", tex);
    }
}


