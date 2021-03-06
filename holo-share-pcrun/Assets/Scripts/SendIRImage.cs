﻿// By LI Jiawei
// 2017.6.12

using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity.InputModule;

using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

/// <summary>
/// This is for broadcast image.
/// </summary>
public class SendIRImage : MonoBehaviour
{
    float timeGapForSend = 0.25f;
    float timeGapForReceive = 1f / 64f;
    float lastTime;
    float lastTime2;
    float lastTime3;


    int scanIndex = 42;
    int numImagesSent = 0;

    string ImageDir = @"IRCamera\output.png";
    
    string TempDir = @"IRCamera\tempoutput.txt";
    //string ImageDir = @"IRCamera\tinypic.png";

    Texture2D texForSend;
    Texture2D texForSendAfterCompression;
    int width = 500;
    int height = 0;
    Color[] lineImgFromTex;
    Color empty = new Color(0,0,0,0);

    bool haveNewMessage = false;

    Material mat;
    Texture2D tex;

    CustomMessagesIRImage myMessage;
    CustomMessages240 customMessage;

    byte[] imageData;
    private bool getNewImage;


    float temperature = 0;




    /// <summary>
    /// function enable/disable
    /// </summary>
    bool enableAlternateOpacity = false;
    bool enableThreshholdImage = true;
    bool enableScanline = false;
    bool enableOnlySendTemp = false;
    


    // all ipc stuff












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
        //Debug.Log("the size of imageData is " + imageData.Length);
        string tempString = File.ReadAllText(TempDir);
        temperature = float.Parse(tempString);
        Debug.Log("temperature is: " + temperature);

        getNewImage = true;
    }
    
    private void DebugShowImage()
    {
        tex.LoadImage(imageData);
        //tex.Resize(80, 60);
        tex.Apply();
        
        //Debug.Log("更新了material");
    }

    private void InitImage() {
        width = tex.width;
        height = tex.height;
        Color[] lineImgFromTex;
        texForSend = new Texture2D(width, 20);
        texForSendAfterCompression = new Texture2D(width, 20, TextureFormat.DXT5, false);

    }

    private void ThreshholdImage() {
        for (int i = 0; i < tex.height; i++)
        {
            for (int j = 0; j < tex.width; j++)
            {
                if ((tex.GetPixel(j, i).r + tex.GetPixel(j, i).g + tex.GetPixel(j, i).b)/3 < 0.5) {
                    tex.SetPixel(j, i, empty);
                }
            }
        }
        tex.Apply();
        Debug.Log("Thresholded image");
    }

    private void AlternateOpacity()
    {
        for (int i = 0; i < tex.height; i++)
        {
            for (int j = 0; j < tex.width; j++)
            {
                tex.SetPixel(j, i, new Color(tex.GetPixel(j, i).r, tex.GetPixel(j, i).g, tex.GetPixel(j, i).b, (Mathf.Sin(Time.time) / 2) + 0.5f));
            }
        }
        tex.Apply();
        Debug.Log("Alternated Opacity of Image to: " + ((Mathf.Sin(Time.time) / 2) + 0.5f) + "Time is: " + Time.time);


    }


    void Update()
    {


        if (Time.time - lastTime2 > 1)
        {
            lastTime2 = Time.time;
            Debug.Log("sent " + numImagesSent + "in 1 second");
            numImagesSent = 0;
        }

        if (Time.time - lastTime > timeGapForReceive)
        //if (true)
        {
            lastTime = Time.time;

            GetImageFromDir();

            

            if (getNewImage)
            {
                
                DebugShowImage();
                


                if (enableAlternateOpacity) {
                    AlternateOpacity();
                }

                if (enableThreshholdImage) {
                    ThreshholdImage();
                }
                

                mat.SetTexture("_MainTex", tex);

                getNewImage = false;

                InitImage();
                //myMessage.SendIRImageByString(imageData);
                //myMessage.SendIRImageByArray(imageData);

                //myMessage.SendIRImage(imageData);
                numImagesSent++;
            }
            //Debug.Log("Just sent a image");

            



        }

        if (Time.time - lastTime3 > timeGapForSend) {
            lastTime3 = Time.time;

            if (enableOnlySendTemp)
            {
                myMessage.SendTempTarget(temperature);
            }
            else if (enableScanline)
            {
                if (scanIndex < tex.height - 20)
                {


                    lineImgFromTex = tex.GetPixels(0, scanIndex, width, 20);

                    texForSend.SetPixels(0, 0, width, 20, lineImgFromTex);
                    texForSend.Apply();
                    texForSendAfterCompression.LoadRawTextureData(texForSend.GetRawTextureData());
                    texForSendAfterCompression.Compress(false);
                    texForSendAfterCompression.Apply();
                    //byte[] lineImg = texForSendAfterCompression.GetRawTextureData();
                    byte[] lineImg = texForSend.EncodeToPNG();
                    myMessage.SendIRImageByLinescan(lineImg, scanIndex);
                    Debug.Log("sent packet size (in bytes): " + lineImg.Length + " scanIndex is: " + scanIndex);
                    scanIndex += 1;

                }
                else
                {
                    scanIndex = 0;
                }
            }
            else
            {
                byte[] imgData = tex.EncodeToPNG();
                //myMessage.SendIRImageByArray(imgData);
                myMessage.SendTempTargetWithImage(imgData, temperature);
                Debug.Log("sent packet size (in bytes): " + imgData.Length);
            }
        }




    }






}