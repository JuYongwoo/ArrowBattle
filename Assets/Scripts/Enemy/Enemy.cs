using System.Collections;
using UnityEngine;

public class Enemy : CharacterBase
{
    protected override CharacterTypeEnum CharacterTypeEnum => CharacterTypeEnum.Enemy;

    private float firstDelay = 2f;
    private float interval = 2f;
    private float moveDuration = 3f;
    private float moveInterval = 2f;

    private Coroutine skillLoop, moveLoop;
    private WaitForSeconds wFirstAttack, wAttackInterval, wMoveDuration, wMoveInterval;

    protected override void Awake()
    {
        base.Awake();
        wFirstAttack = new WaitForSeconds(firstDelay);
        wAttackInterval = new WaitForSeconds(interval);
        wMoveDuration = new WaitForSeconds(moveDuration);
        wMoveInterval = new WaitForSeconds(moveInterval);
    }

    private void Start()
    {
        skillLoop = StartCoroutine(CoSkillLoop());
        moveLoop = StartCoroutine(CoPingPongMove());
    }

    private void OnDestroy()
    {
        if (skillLoop != null) StopCoroutine(skillLoop);
        if (moveLoop != null) StopCoroutine(moveLoop);
        skillLoop = moveLoop = null;
    }

    private IEnumerator CoSkillLoop()
    {
        if (firstDelay > 0f) yield return wFirstAttack;
        while (true)
        {
            Skill(Skills.Attack);
            yield return wAttackInterval;
        }
    }

    private IEnumerator CoPingPongMove()
    {
        float dir = -1f;
        WaitForFixedUpdate waitFixed = new WaitForFixedUpdate();

        while (true)
        {
            float t = 0f;
            while (t < moveDuration)
            {
                Move(dir);
                yield return waitFixed;
                t += Time.fixedDeltaTime;
            }
            SetCharacterMotion(CharacterAnimationEnum.Idle);
            yield return wMoveInterval;
            dir = -dir;
        }
    }


}
