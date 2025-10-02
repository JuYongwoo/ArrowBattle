using UnityEngine.LowLevel;

public class Player : CharacterBase
{
    // SO ������ ��ü ����
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Player;


    protected override void Awake()
    {
        base.Awake();
        ManagerObject.instance.actionManager.useSkill -= prepareSkill; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
        ManagerObject.instance.actionManager.useSkill += prepareSkill; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
        ManagerObject.instance.actionManager.leftRightMove -= move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
        ManagerObject.instance.actionManager.leftRightMove += move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
        ManagerObject.instance.actionManager.idle -= setIdle;
        ManagerObject.instance.actionManager.idle += setIdle;
        ManagerObject.instance.actionManager.getCastingSkill -= getCastingKill;
        ManagerObject.instance.actionManager.getCastingSkill += getCastingKill;
    }

    private void setIdle()
    {
        setState(CharacterStateEnum.Idle);
    }
    
    private Skill getCastingKill()
    {
        return castingSkill;
    }

    private void OnDestroy()
    {
        ManagerObject.instance.actionManager.useSkill -= prepareSkill; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
        ManagerObject.instance.actionManager.leftRightMove -= move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
        ManagerObject.instance.actionManager.idle -= setIdle;
        ManagerObject.instance.actionManager.getCastingSkill -= getCastingKill;
    }

    protected void Update()
    {
        if (state == CharacterStateEnum.Idle) //�ƹ� �ൿ ���� ���� �� �Ϲ� ����
        {
            if (skillCoroutine == null) prepareSkill(Skill.Attack);
        }
    }

    public override void getDamaged(float damageAmount)
    {
        base.getDamaged(damageAmount); // CharacterBase�� getDamaged() ȣ��
        ManagerObject.instance.actionManager.setPlayerHPinUIM(stat.Current.CurrentHP, stat.Current.MaxHP);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.instance.actionManager.endGameM(ResultStateEnum.Defeat);
        }
    }

    protected override bool TryBeginCooldown(Skill skill)
    {
        if (base.TryBeginCooldown(skill)) //�θ� �����ϰ�
        {
            //UI �߰����� �� true ��ȯ
            ManagerObject.instance.actionManager.CooldownUIM((int)skill, ManagerObject.instance.resourceManager.attackSkillData[skill].Result.skillCoolTime);
            return true;
        }
        else
        {
            return false;
        }
    }

}
