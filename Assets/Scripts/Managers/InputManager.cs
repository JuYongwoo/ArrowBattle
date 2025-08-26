using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InputManager
{
    public Action<float> leftRightMove;

    public void OnAwake()
    {

    }

    public void OnUpdate()
    {

        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0)
        {
            leftRightMove?.Invoke(moveX);
        }

    }

}
