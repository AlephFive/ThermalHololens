using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class CubeMove : MonoBehaviour, IInputClickHandler
{
    // 是否正在移动
    bool isMoving = false;

    CustomMessages240 customMessage;

    private void Start()
    {
        customMessage = CustomMessages240.Instance;

        // 增加一个Callback，触发这个的时候就触发下面的函数
        customMessage.MessageHandlers[CustomMessages240.CustomMessageID.CubePostion] = OnCubePositionReceived;
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
        if (isMoving)
        {
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        }
    }
}