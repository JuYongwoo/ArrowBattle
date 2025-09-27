using System;
using UnityEngine;

public class InputManager
{

    public void OnUpdate()
    {
        ManagerObject.instance.actionManager.mainSceneInput?.Invoke();



    }
}
