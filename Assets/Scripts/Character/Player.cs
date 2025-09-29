using System;

public class Player : CharacterBase
{
    // SO 값으로 교체 예정
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Player;



    protected override void Awake()
    {
        base.Awake(); // CharacterStatBase의 Awake() 호출
    }
    protected void Update()
    {
        if (state == CharacterStateEnum.Idle) //아무 행동 하지 않을 시 일반 공격
        {
            if (skillCoroutine == null) prepareSkill(Skill.Attack);
        }
    }

    public override void getDamaged(float damageAmount)
    {
        base.getDamaged(damageAmount); // CharacterBase의 getDamaged() 호출
        ManagerObject.instance.actionManager.setPlayerHPinUI(stat.Current.CurrentHP, stat.Current.MaxHP);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.instance.actionManager.endGame(ResultStateEnum.Defeat);
        }
    }

    protected override bool tryBeginCooldown(Skill skill)
    {
        if (base.tryBeginCooldown(skill)) //부모 실행하고
        {
            //UI 추가실행 후 true 반환
            ManagerObject.instance.actionManager.CooldownUI?.Invoke((int)skill, ManagerObject.instance.resourceManager.attackSkillData[skill].Result.skillCoolTime);
            return true;
        }
        else
        {
            return false;
        }
    }

}
