using UnityEngine.LowLevel;

public class Player : CharacterBase
{
    // SO 값으로 교체 예정
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Player;


    protected override void Awake()
    {
        base.Awake();
        ManagerObject.instance.actionManager.UseSkillEvent -= PrepareSkill; // InputManager의 attack 이벤트에 Attack 메서드 구독
        ManagerObject.instance.actionManager.UseSkillEvent += PrepareSkill; // InputManager의 attack 이벤트에 Attack 메서드 구독
        ManagerObject.instance.actionManager.LeftRightMoveEvent -= Move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
        ManagerObject.instance.actionManager.LeftRightMoveEvent += Move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
        ManagerObject.instance.actionManager.SetIdleEvent -= SetIdle;
        ManagerObject.instance.actionManager.SetIdleEvent += SetIdle;
        ManagerObject.instance.actionManager.GetCastingSkillEvent -= GetCastingKill;
        ManagerObject.instance.actionManager.GetCastingSkillEvent += GetCastingKill;
    }

    private void SetIdle()
    {
        SetState(CharacterStateEnum.Idle);
    }
    
    private Skill GetCastingKill()
    {
        return castingSkill;
    }

    private void OnDestroy()
    {
        ManagerObject.instance.actionManager.UseSkillEvent -= PrepareSkill; // InputManager의 attack 이벤트에 Attack 메서드 구독
        ManagerObject.instance.actionManager.LeftRightMoveEvent -= Move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
        ManagerObject.instance.actionManager.SetIdleEvent -= SetIdle;
        ManagerObject.instance.actionManager.GetCastingSkillEvent -= GetCastingKill;
    }

    protected void Update()
    {
        if (state == CharacterStateEnum.Idle) //아무 행동 하지 않을 시 일반 공격
        {
            if (skillCoroutine == null) PrepareSkill(Skill.Attack);
        }
    }

    public override void GetDamaged(float damageAmount)
    {
        base.GetDamaged(damageAmount); // CharacterBase의 getDamaged() 호출
        ManagerObject.instance.actionManager.OnSetPlayerHPInUI(Stat.Current.CurrentHP, Stat.Current.MaxHP);
        if (Stat.Current.CurrentHP <= 0)
        {
            ManagerObject.instance.actionManager.OnEndGame(ResultStateEnum.Defeat);
        }
    }

    protected override bool TryBeginCooldown(Skill skill)
    {
        if (base.TryBeginCooldown(skill)) //부모 실행하고
        {
            //UI 추가실행 후 true 반환
            ManagerObject.instance.actionManager.OnCooldownUI((int)skill, ManagerObject.instance.resourceManager.SkillDatas.Result.GetSkillDataById(skill).skillCoolTime);
            return true;
        }
        else
        {
            return false;
        }
    }

}
