using System;

public class Player : CharacterBase
{
    // SO 값으로 교체 예정
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Player;


    public static Action<float, float> setHPinUI;

    protected override void Awake()
    {
        base.Awake(); // CharacterStatBase의 Awake() 호출
        mapOtherAction();
    }


    public override void getDamaged(float damageAmount)
    {
        stat.deltaHP(-damageAmount);
        setHPinUI(stat.Current.CurrentHP, stat.Current.MaxHP);
        ManagerObject.audioM.PlayAudioClip(stat.Current.HitSound);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.gameMode.endGame(ResultStateEnum.Defeat);
        }
    }
    private void mapOtherAction()
    {
        ManagerObject.inputM.useSkill = prepareSkill; // InputManager의 attack 이벤트에 Attack 메서드 구독
        ManagerObject.inputM.leftRightMove = Move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
        ManagerObject.inputM.idle = () => { setState(CharacterStateEnum.Idle); };
        ManagerObject.inputM.getCastingSkill = () => castingSkill;

    }
}
