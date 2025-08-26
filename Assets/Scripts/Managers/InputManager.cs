using System;
using UnityEngine;

public class InputManager
{
    public Action<float> leftRightMove;
    public Action<Skills> useSkill;

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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            useSkill?.Invoke(Skills.Attack);
        }

    }

}
