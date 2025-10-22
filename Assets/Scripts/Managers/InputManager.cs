using UnityEngine;

namespace JYW.ArrowBattle.Managers
{

    public class InputManager
    {

        public void OnUpdate()
        {


            if (ManagerObject.instance.eventManager.OnGetCastingSkill() != SkillType.Attack) return; //일반 공격 외 다른 스킬 캐스팅 중이라면 어떠한 움직임도 X
                                                                                                 // 스킬 입력 우선
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

            // 이동/Idle
            float moveX = Input.GetAxisRaw("Horizontal");
            if (moveX != 0f)
            {
                ManagerObject.instance.eventManager.OnLeftRightMove(moveX); return;
            }

            ManagerObject.instance.eventManager.OnSetIdle();


        }
    }
}
