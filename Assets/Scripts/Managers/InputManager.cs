using UnityEngine;

namespace JYW.ArrowBattle.Managers
{

    public class InputManager
    {

        public void OnUpdate()
        {


            if (ManagerObject.instance.eventManager.OnGetCastingSkill() != SkillType.Attack) return; //�Ϲ� ���� �� �ٸ� ��ų ĳ���� ���̶�� ��� �����ӵ� X
                                                                                                 // ��ų �Է� �켱
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ManagerObject.instance.eventManager.OnUseSkill(SkillType.Skill1); return;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                ManagerObject.instance.eventManager.OnUseSkill(SkillType.Skill2); return;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                ManagerObject.instance.eventManager.OnUseSkill(SkillType.Skill3); return;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ManagerObject.instance.eventManager.OnUseSkill(SkillType.Skill4); return;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ManagerObject.instance.eventManager.OnUseSkill(SkillType.Skill5); return;
            }

            // �̵�/Idle
            float moveX = Input.GetAxisRaw("Horizontal");
            if (moveX != 0f)
            {
                ManagerObject.instance.eventManager.OnLeftRightMove(moveX); return;
            }

            ManagerObject.instance.eventManager.OnSetIdle();


        }
    }
}
