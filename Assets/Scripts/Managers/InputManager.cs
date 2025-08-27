using System;
using UnityEngine;

public class InputManager
{
    public Action<float> leftRightMove;
    public Action<Skills> useSkill;
    public Action idle;

    public void OnUpdate()
    {
        // ��ų �Է� �켱
        if (Input.GetKeyDown(KeyCode.Q)) { useSkill?.Invoke(Skills.Skill1); return; }
        if (Input.GetKeyDown(KeyCode.W)) { useSkill?.Invoke(Skills.Skill2); return; }
        if (Input.GetKeyDown(KeyCode.E)) { useSkill?.Invoke(Skills.Skill3); return; }
        if (Input.GetKeyDown(KeyCode.R)) { useSkill?.Invoke(Skills.Skill4); return; }
        if (Input.GetKeyDown(KeyCode.Space)) { useSkill?.Invoke(Skills.Skill5); return; }

        // �̵�/Idle
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0f) { leftRightMove?.Invoke(moveX); return; }

        idle?.Invoke();
    }
}
