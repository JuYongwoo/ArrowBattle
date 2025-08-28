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
        InvokeRepeating(nameof(randomSkill), 3f, 3f); //랜덤 스킬 사용
    }
    protected void Update()
    {
        if (state == CharacterStateEnum.Idle) //아무 행동 하지 않을 시 일반 공격
        {
            if (skillCoroutine == null) prepareSkill(Skill.Attack);
        }
    }
    private void OnDestroy()
    {
        if (moveLoop != null) StopCoroutine(moveLoop);
    }
    
    private void randomSkill()
    {
        Array skills = Enum.GetValues(typeof(Skill));
        System.Random random = new System.Random();
        Skill randomSkill = (Skill)skills.GetValue(random.Next(skills.Length));
        prepareSkill(randomSkill);
    }

    public override void getDamaged(float damageAmount)
    {
        base.getDamaged(damageAmount); // CharacterBase의 getDamaged() 호출
        setHPinUI(stat.Current.CurrentHP, stat.Current.MaxHP);
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
                move(dir);
                yield return waitFixed;
                t += Time.fixedDeltaTime;
            }
            setState(CharacterStateEnum.Idle);
            yield return new WaitForSeconds(moveInterval);
            dir = -dir;
        }
    }


}
