// By LI Jiawei
// 2017.6.12

using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using System.IO;

/// <summary>
/// This is for broadcast image.
/// </summary>
public class SendIRImage : MonoBehaviour
{
    float timeGap = 1f / 30f;
    float lastTime;


    int scanIndex = 42;


    string ImageDir = @"IRCamera\output.png";

    Texture2D texForSend;

    bool haveNewMessage = false;

    Material mat;
    Texture2D tex;

    CustomMessagesIRImage myMessage;
    CustomMessages240 customMessage;

    byte[] imageData;
    private bool getNewImage;

    private void Start()
    {
        mat = this.GetComponent<Renderer>().material;
        tex = new Texture2D(1, 1);

        //customMessage = CustomMessages240.Instance;
        myMessage = CustomMessagesIRImage.Instance;
    }

    private void GetImageFromDir()
    {
        imageData = File.ReadAllBytes(ImageDir);
        Debug.Log("the size of imageData is " + imageData.Length);
        getNewImage = true;
    }

    private void DebugShowImage()
    {
        tex.LoadImage(imageData);
        tex.Apply();
        mat.SetTexture("_MainTex", tex);
        Debug.Log("更新了material");
    }

    void Update()
    {
        if (Time.time - lastTime > timeGap)
        {
            lastTime = Time.time;

            GetImageFromDir();

            if (getNewImage)
            {
                getNewImage = false;

                //myMessage.SendIRImageByString(imageData);
                myMessage.SendIRImageByArray(imageData);
                myMessage.SendIRImageByLinescan(imageData , scanIndex);
                //myMessage.SendIRImage(imageData);

                DebugShowImage();
            }
            Debug.Log("Just sent a image");
        }
    }
}