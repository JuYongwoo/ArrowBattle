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
        if (getCastingSkill() != Skill.Attack) return; //일반 공격 외 다른 스킬 캐스팅 중이라면 어떠한 움직임도 X
        // 스킬 입력 우선
        if (Input.GetKeyDown(KeyCode.Q)) { useSkill?.Invoke(Skill.Skill1); return; }
        if (Input.GetKeyDown(KeyCode.W)) { useSkill?.Invoke(Skill.Skill2); return; }
        if (Input.GetKeyDown(KeyCode.E)) { useSkill?.Invoke(Skill.Skill3); return; }
        if (Input.GetKeyDown(KeyCode.R)) { useSkill?.Invoke(Skill.Skill4); return; }
        if (Input.GetKeyDown(KeyCode.Space)) { useSkill?.Invoke(Skill.Skill5); return; }

        // 이동/Idle
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0f) { leftRightMove?.Invoke(moveX); return; }

        idle?.Invoke();
    }
}
