using System;
using UnityEngine;

public class InputManager
{
    public Action<float> leftRightMove;
    public Action<Skills> useSkill;
    public Action idle;


    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            useSkill?.Invoke(Skills.Attack);
        }
        else
        {

            float moveX = Input.GetAxisRaw("Horizontal");
            if (moveX != 0)
            {
                leftRightMove?.Invoke(moveX);
            }
            else
            {
                idle?.Invoke();
            }
        }


    }

}
