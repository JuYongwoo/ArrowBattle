using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CharacterBase
{
    protected override CharacterTypeEnumByTag CharacterTypeEnum => CharacterTypeEnumByTag.Enemy;

    private float moveDuration = 3f;
    private float moveInterval = 3.1f;

    private Coroutine moveLoop;


    protected override void Start()
    {
        base.Start();
        moveLoop = StartCoroutine(CoMove());
        InvokeRepeating(nameof(randomSkill), 0f, 1.1f); //랜덤 스킬 사용
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
        if( state == CharacterStateEnum.Moving) return; //움직일땐 스킬 사용 안함
        List<int> skillsList = new List<int>() { 1, 2, 3, 4, 5 }; //Attack, Skill1, Skill2
        System.Random random = new System.Random();
        Skill randomSkill = (Skill)random.Next(skillsList.Count);
        prepareSkill(randomSkill);
    }

    public override void getDamaged(float damageAmount)
    {
        base.getDamaged(damageAmount); // CharacterBase의 getDamaged() 호출
        ManagerObject.instance.actionManager.OnSetEnemyHPInUI(stat.Current.CurrentHP, stat.Current.MaxHP);
        if (stat.Current.CurrentHP <= 0)
        {
            ManagerObject.instance.actionManager.OnEndGame(ResultStateEnum.Victory);
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
