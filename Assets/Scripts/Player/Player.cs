using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : CharacterBase
{
    // SO 값으로 교체 예정
    protected override CharacterTypeEnum CharacterTypeEnum => CharacterTypeEnum.Player;


    protected override void Awake()
    {
        base.Awake(); // CharacterStatBase의 Awake() 호출
        mapOtherAction();
    }



    private void mapOtherAction()
    {
        ManagerObject.inputM.useSkill = startSkill; // InputManager의 attack 이벤트에 Attack 메서드 구독
        ManagerObject.inputM.leftRightMove = Move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
        ManagerObject.inputM.idle = () => { setState(CharacterStateEnum.Idle); };

    }
}
