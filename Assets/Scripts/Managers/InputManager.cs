using System;
using UnityEngine;

public class InputManager
{
    public Action<float> leftRightMove;
    public Action<Skills> useSkill;
    public Action idle;


    public void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            useSkill?.Invoke(Skills.Skill1);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            useSkill?.Invoke(Skills.Skill2);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            useSkill?.Invoke(Skills.Skill3);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            useSkill?.Invoke(Skills.Skill4);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            useSkill?.Invoke(Skills.Skill5);
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
