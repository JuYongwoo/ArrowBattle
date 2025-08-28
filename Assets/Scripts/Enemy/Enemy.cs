using System;
using System.Collections;
using UnityEngine;

public class Enemy : CharacterBase
{
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Enemy;

    private float moveDuration = 3f;
    private float moveInterval = 2.1f;

    private Coroutine moveLoop;

    public static Action<float, float> setHPinUI;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        moveLoop = StartCoroutine(CoMove());
    }

    private void OnDestroy()
    {
        if (moveLoop != null) StopCoroutine(moveLoop);
    }

    public override void getDamaged(float damageAmount)
    {
        stat.deltaHP(-damageAmount);
        setHPinUI(stat.Current.CurrentHP, stat.Current.MaxHP);
        ManagerObject.audioM.PlayAudioClip(stat.Current.HitSound);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.gameMode.endGame(ResultStateEnum.Victory);
        }
    }

    private IEnumerator CoMove()
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
            setState(CharacterStateEnum.Idle);
            yield return new WaitForSeconds(moveInterval);
            dir = -dir;
        }
    }


}
