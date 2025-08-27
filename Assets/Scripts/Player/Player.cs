using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : CharacterBase
{
    // SO ������ ��ü ����
    protected override CharacterTypeEnum CharacterTypeEnum => CharacterTypeEnum.Player;


    protected override void Awake()
    {
        base.Awake(); // CharacterStatBase�� Awake() ȣ��
        mapOtherAction();
    }



    private void mapOtherAction()
    {
        ManagerObject.inputM.useSkill = startSkill; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
        ManagerObject.inputM.leftRightMove = Move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
        ManagerObject.inputM.idle = () => { setState(CharacterStateEnum.Idle); };

    }
}
