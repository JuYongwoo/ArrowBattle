using System;
using UnityEngine;

public class InputManager
{
    public Action<float> leftRightMove;
    public Action<Skill> useSkill;
    public Action idle;
    public Func<Skill> getCastingSkill;

    public void OnUpdate()
    {
        if (getCastingSkill() != Skill.Attack) return; //�Ϲ� ���� �� �ٸ� ��ų ĳ���� ���̶�� ��� �����ӵ� X
        // ��ų �Է� �켱
        if (Input.GetKeyDown(KeyCode.Q)) { useSkill?.Invoke(Skill.Skill1); return; }
        if (Input.GetKeyDown(KeyCode.W)) { useSkill?.Invoke(Skill.Skill2); return; }
        if (Input.GetKeyDown(KeyCode.E)) { useSkill?.Invoke(Skill.Skill3); return; }
        if (Input.GetKeyDown(KeyCode.R)) { useSkill?.Invoke(Skill.Skill4); return; }
        if (Input.GetKeyDown(KeyCode.Space)) { useSkill?.Invoke(Skill.Skill5); return; }

        // �̵�/Idle
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0f) { leftRightMove?.Invoke(moveX); return; }

        idle?.Invoke();
    }
}
