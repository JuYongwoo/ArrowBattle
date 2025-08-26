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
        OnInputReceived?.Invoke(); //�� �̵�, �� �̵�, ����, ����, ��ų �� �Է� �Ҵ�� ���� ����
    }

}
