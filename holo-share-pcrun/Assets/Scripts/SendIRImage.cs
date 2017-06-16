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
    int width = 500;
    int height = 0;
    Color[] lineImgFromTex;

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
        scanIndex = 0;
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

    private void InitImage() {
        width = tex.width;
        height = tex.height;
        Color[] lineImgFromTex;
        texForSend = new Texture2D(width, 20);

    }

    void Update()
    {

        


        if (Time.time - lastTime > timeGap)
        //if (true)
        {
            lastTime = Time.time;

            GetImageFromDir();

            

            if (getNewImage)
            {
                DebugShowImage();
                getNewImage = false;

                InitImage();
                //myMessage.SendIRImageByString(imageData);
                //myMessage.SendIRImageByArray(imageData);

                //myMessage.SendIRImage(imageData);

            }
            Debug.Log("Just sent a image");

            
        }

        if (scanIndex < tex.height - 20)
        {
            

            lineImgFromTex = tex.GetPixels(0, scanIndex, width, 20);
            texForSend.SetPixels(0, 0, width, 20, lineImgFromTex);
            texForSend.Apply();
            byte[] lineImg = texForSend.EncodeToPNG();
            myMessage.SendIRImageByLinescan(lineImg, scanIndex);

            scanIndex += 1;

        }
        else
        {
            scanIndex = 0;
        }

    }
}