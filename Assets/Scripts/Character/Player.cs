using UnityEngine.LowLevel;

public class Player : CharacterBase
{
    // SO 값으로 교체 예정
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Player;


    protected override void Awake()
    {
        base.Awake();
        ManagerObject.instance.actionManager.useSkill = prepareSkill; // InputManager의 attack 이벤트에 Attack 메서드 구독
        ManagerObject.instance.actionManager.leftRightMove = move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
        ManagerObject.instance.actionManager.idle = () => { setState(CharacterStateEnum.Idle); };
        ManagerObject.instance.actionManager.getCastingSkill = () => castingSkill;
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
        ManagerObject.instance.actionManager.setPlayerHPinUI?.Invoke(stat.Current.CurrentHP, stat.Current.MaxHP);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.instance.actionManager.endGame?.Invoke(ResultStateEnum.Defeat);
        }
    }

    protected override bool TryBeginCooldown(Skill skill)
    {
        if (base.TryBeginCooldown(skill)) //부모 실행하고
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
