using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using HoloToolkit.Unity;
using System.IO;

public class ClickToOpenServer : MonoBehaviour {

    string OptrisDir = @"IRCamera\Optris PI Connect.lnk";
    string IPC2SoftDir = @"IRCamera\Start IPC2 Win32.exe";
    private bool serverOpened = false;

    // Use this for initialization
    void Start () {
		
	}

    /// <summary>
    /// 检查并开启两个程序
    /// </summary>
    private void CheckAndStartServer()
    {
        if (!File.Exists(OptrisDir))
        {
            UnityEngine.Debug.LogError("没有摄像头程序快捷方式");
        }
        else
        {
            Process.Start(OptrisDir);
        }

        if (!File.Exists(IPC2SoftDir))
        {
            UnityEngine.Debug.LogError("没有IPC2程序");
        }
        else
        {
            Process.Start(IPC2SoftDir);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10,10,100,100), "start server"))
        {
            if (!serverOpened)
            {
                CheckAndStartServer();
            }
        }
    }
}
