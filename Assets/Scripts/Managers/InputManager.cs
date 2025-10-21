using UnityEngine;

public class InputManager
{

    public void OnUpdate()
    {


        if (ManagerObject.instance.eventManager.OnGetCastingSkill() != Skill.Attack) return; //�Ϲ� ���� �� �ٸ� ��ų ĳ���� ���̶�� ��� �����ӵ� X
                                                                                            // ��ų �Է� �켱
        if (Input.GetKeyDown(KeyCode.Q)) {
            ManagerObject.instance.eventManager.OnUseSkill(Skill.Skill1); return;
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            ManagerObject.instance.eventManager.OnUseSkill(Skill.Skill2); return;
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            ManagerObject.instance.eventManager.OnUseSkill(Skill.Skill3); return;
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            ManagerObject.instance.eventManager.OnUseSkill(Skill.Skill4); return;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            ManagerObject.instance.eventManager.OnUseSkill(Skill.Skill5); return;
        }

        // �̵�/Idle
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0f) {
            ManagerObject.instance.eventManager.OnLeftRightMove(moveX); return;
        }

        ManagerObject.instance.eventManager.OnSetIdle();


    }
}
