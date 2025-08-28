using System;

public class Player : CharacterBase
{
    // SO ������ ��ü ����
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Player;


    public static Action<float, float> setHPinUI;

    protected override void Awake()
    {
        base.Awake(); // CharacterStatBase�� Awake() ȣ��
        mapOtherAction();
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
        setHPinUI(stat.Current.CurrentHP, stat.Current.MaxHP);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.gameMode.endGame(ResultStateEnum.Defeat);
        }
    }
    private void mapOtherAction()
    {
        ManagerObject.inputM.useSkill = prepareSkill; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
        ManagerObject.inputM.leftRightMove = move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
        ManagerObject.inputM.idle = () => { setState(CharacterStateEnum.Idle); };
        ManagerObject.inputM.getCastingSkill = () => castingSkill;

    }
}
