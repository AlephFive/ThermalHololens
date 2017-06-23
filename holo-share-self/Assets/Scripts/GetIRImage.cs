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
    TextMesh targetTextMesh;
    public GameObject debugGOJ;
    public GameObject oTarget;
    float jiangeTime;
    float countStartTime;

    int counter = 0;

    float textureJiangeTime = 1f / 30f;
    float textureLastTime = 0f;

    float screenX = 0;
    float screenY = 0;

    
    Vector3 moveVec = new Vector3(0, -1.16f / 120, 0);
    Vector3 initPos = new Vector3(0, 0.58f, 5);


    float temperature = 0;

    bool onlyGetTemperature = false;
    bool enableLinescan = false;




    private void Start()
    {
        myTextMesh = debugGOJ.GetComponent<TextMesh>();
        targetTextMesh = oTarget.GetComponent<TextMesh>();

        mat = this.GetComponent<Renderer>().material;
        tex = new Texture2D(256, 256);
        lineTex = new Texture2D(256, 20);
        lineColors = new Color[5121];
        //customMessage = CustomMessages240.Instance;
        myMessage = CustomMessagesIRImage.Instance;

        // 增加一个Callback，触发这个的时候就触发下面的函数
        //customMessage.MessageHandlers[CustomMessages240.CustomMessageID.CubePostion] = OnCubePositionReceived;
        myMessage.MessageHandlers[CustomMessagesIRImage.CustomMessageID.IRImage] = OnIRImageReceived;

        if (onlyGetTemperature)
        {
            this.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
            myTextMesh.color = new Color(1f, 1f, 1f);
            myTextMesh.text = "Initialising TempOnly Mode";
        }
        else if (enableLinescan)
        {
            this.gameObject.transform.localScale = new Vector3(0.2f, 1f, 0.02500017f);
            myTextMesh.color = new Color(1f, 1f, 1f);
            myTextMesh.text = "Initialising Linescan Mode";
            
        }
        else {
            this.gameObject.transform.localPosition = new Vector3(0f, 0f, 5f);
            this.gameObject.transform.localScale = new Vector3(0.2f, 1f, 0.1500001f);
            myTextMesh.color = new Color(1f, 1f, 1f);
            myTextMesh.text = "Initialising Normal Mode";
        }
        

    }

    private void OnIRImageReceived(NetworkInMessage msg)
    {
        countStartTime = Time.time;

        



        if (onlyGetTemperature)
        {
            temperature = CustomMessagesIRImage.ReadTemperature(msg);
        }
        else if (enableLinescan)
        {
            imageData = CustomMessagesIRImage.ReadIRImageByLinescan(msg);
            scanIndex = CustomMessagesIRImage.scanIndex;
        }
        else
        {
            imageData = CustomMessagesIRImage.ReadIRImageAndTemperature(msg);
            temperature = CustomMessagesIRImage.temperature;
        }
            //imageData = CustomMessagesIRImage.ReadIRImage(msg);
            //imageData = CustomMessagesIRImage.ReadIRImageByString(msg);
            //imageData = CustomMessagesIRImage.ReadIRImageByArray(msg);

            //imageData = CustomMessagesIRImage.ReadIRImageByLinescan(msg);



        
        
        Debug.Log("scanIndex is " + scanIndex);
        haveNewMessage = true;
        jiangeTime = Time.time - countStartTime;
        //Debug.Log("接收消息处理时间为：" + jiangeTime.ToString("F2"));
    }
    /*
    private void ReadIRImage(NetworkInMessage msg) {
        if (onlyGetTemperature) {
            temperature = CustomMessagesIRImage.ReadTemperature(msg);
        }
        else if (enableLinescan) {
            imageData = CustomMessagesIRImage.ReadIRImageByLinescan(msg);
            scanIndex = CustomMessagesIRImage.scanIndex;
        }        
        else{
            imageData = CustomMessagesIRImage.ReadIRImageAndTemperature(msg);
            temperature = CustomMessagesIRImage.temperature;
        }

    }
    */
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
            
            UpdataTexture();

            //myTextMesh.text = (counter++).ToString();
            

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

        if (onlyGetTemperature)
        {
            myTextMesh.text = temperature.ToString() + "*C";
        }
        else
        {
            tex.LoadImage(imageData);
            //tex.LoadRawTextureData(imageData);

            

            Debug.Log("image loaded");

            tex.Apply();
            mat.SetTexture("_MainTex", tex);

            if (enableLinescan)
            {
                Vector3 calculatedPos = new Vector3(0, 0.60f + scanIndex * (-1.16f / 94), 5);
                //this.gameObject.transform.position = calculatedPos;
                this.gameObject.transform.localPosition = calculatedPos;
                myTextMesh.text = scanIndex.ToString();
            }
            else {
                myTextMesh.text = temperature.ToString() + "*C"; ;
            }
        }

        
        
            



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


