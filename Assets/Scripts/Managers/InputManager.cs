using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InputManager
{
    public Action OnInputReceived;

    public void OnAwake()
    {

    }

    public void OnUpdate()
    {
        OnInputReceived?.Invoke(); //좌 이동, 우 이동, 점프, 공격, 스킬 등 입력 할당된 것을 실행
    }

}
