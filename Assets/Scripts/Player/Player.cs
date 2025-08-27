using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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


    public override void getDamaged(float damageAmount)
    {
        stat.deltaHP(-damageAmount);
        setHPinUI(stat.Current.CurrentHP, stat.Current.MaxHP);
        ManagerObject.audioM.PlayAudioClip(stat.Current.HitSound);
    }
    private void mapOtherAction()
    {
        ManagerObject.inputM.useSkill = prepareSkill; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
        ManagerObject.inputM.leftRightMove = Move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
        ManagerObject.inputM.idle = () => { setState(CharacterStateEnum.Idle); };

    }
}
