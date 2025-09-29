using System;

public class Player : CharacterBase
{
    // SO ������ ��ü ����
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Player;



    protected override void Awake()
    {
        base.Awake(); // CharacterStatBase�� Awake() ȣ��
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
        ManagerObject.instance.actionManager.setPlayerHPinUI(stat.Current.CurrentHP, stat.Current.MaxHP);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.instance.actionManager.endGame(ResultStateEnum.Defeat);
        }
    }

    protected override bool tryBeginCooldown(Skill skill)
    {
        if (base.tryBeginCooldown(skill)) //�θ� �����ϰ�
        {
            //UI �߰����� �� true ��ȯ
            ManagerObject.instance.actionManager.CooldownUI?.Invoke((int)skill, ManagerObject.instance.resourceManager.attackSkillData[skill].Result.skillCoolTime);
            return true;
        }
        else
        {
            return false;
        }
    }

}
