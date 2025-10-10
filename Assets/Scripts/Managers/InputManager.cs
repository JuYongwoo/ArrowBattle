using UnityEngine;

public class InputManager
{

    public void OnUpdate()
    {


        if (ManagerObject.instance.actionManager.OnGetCastingSkill() != Skill.Attack) return; //일반 공격 외 다른 스킬 캐스팅 중이라면 어떠한 움직임도 X
                                                                                            // 스킬 입력 우선
        if (Input.GetKeyDown(KeyCode.Q)) {
            ManagerObject.instance.actionManager.OnUseSkill(Skill.Skill1); return;
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            ManagerObject.instance.actionManager.OnUseSkill(Skill.Skill2); return;
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            ManagerObject.instance.actionManager.OnUseSkill(Skill.Skill3); return;
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            ManagerObject.instance.actionManager.OnUseSkill(Skill.Skill4); return;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            ManagerObject.instance.actionManager.OnUseSkill(Skill.Skill5); return;
        }

        // 이동/Idle
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0f) {
            ManagerObject.instance.actionManager.OnLeftRightMove(moveX); return;
        }

        ManagerObject.instance.actionManager.OnIdle();


    }
}
