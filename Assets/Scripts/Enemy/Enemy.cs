using System.Collections;
using UnityEngine;

public class Enemy : CharacterBase
{
    protected override CharacterTypeEnum CharacterTypeEnum => CharacterTypeEnum.Enemy;

    private float moveDuration = 3f;
    private float moveInterval = 2.1f;

    private Coroutine moveLoop;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        moveLoop = StartCoroutine(CoPingPongMove());
    }

    private void OnDestroy()
    {
        if (moveLoop != null) StopCoroutine(moveLoop);
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
            setState(CharacterStateEnum.Idle);
            yield return new WaitForSeconds(moveInterval);
            dir = -dir;
        }
    }


}
