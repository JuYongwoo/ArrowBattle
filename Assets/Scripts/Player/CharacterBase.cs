using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum CharacterStateEnum //Enum으로 상태 관리 //int로 캐스팅하여 사용
{
    Idle, //Attacking을 하기 위한 1틱
    Moving,
    UsingSkill
}

public enum CharacterTypeEnumByTag
{
    Player,
    Enemy
}

//JYW Enemy와 Player의 공통 기능을 묶은 부모 클래스입니다.

public abstract class CharacterBase : MonoBehaviour
{
    protected CharacterStateEnum state = CharacterStateEnum.Moving;
    protected CharacterStatManager stat { get; private set; }
    protected Animator anim;
    protected Rigidbody2D rb;
    private SpriteRenderer sr;
    protected Coroutine skillCoroutine;
    public abstract void getDamaged(float damageAmount); //자식클래스  getDamaged 구현 강제

    protected abstract CharacterTypeEnumByTag CharacterTypeEnum { get; } //Player, Enemy 구분 //추상 프로퍼티 자식 클래스 구현 강제
    protected CharacterTypeEnumByTag OpponentType; //상대방 타입, Awake에서 자동 설정
    protected Skills castingSkill; //현재 시전 중인 스킬

    protected virtual void Awake() //virtual로 선언하여 자식 클래스에서 override 가능하도록 다형성 보장
    {
        stat = new CharacterStatManager($"{Enum.GetName(typeof(CharacterTypeEnumByTag), CharacterTypeEnum)}Data");
        anim = GetComponentInChildren<Animator>(); // Animator 캐싱
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 캐싱
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱
        OpponentType = (CharacterTypeEnum == CharacterTypeEnumByTag.Player) ? CharacterTypeEnumByTag.Enemy : CharacterTypeEnumByTag.Player;


    }

    protected virtual void Start()
    {
        setState(CharacterStateEnum.Idle); //처음 상태는 Idle
    }
    protected virtual void Update()
    {
        if (state == CharacterStateEnum.Idle) //아무 행동 하지 않을 시 일반 공격
        {
            if (skillCoroutine == null) prepareSkill(Skills.Attack);
        }
    }

    public void setState(CharacterStateEnum s)
    {
        if (state == s) return;
        state = s;

        if(state == CharacterStateEnum.Moving || state == CharacterStateEnum.UsingSkill) //이동, 스킬 사용하는 순간 일반 공격 취소
        {
            if (skillCoroutine != null)
            {
                StopCoroutine(skillCoroutine);
                skillCoroutine = null;
            }
        }

        anim.SetInteger("State", (int)s);
    }

    protected void Move(float moveX)
    {
        rb.velocity = new Vector2(moveX * stat.Current.CurrentMoveSpeed, rb.velocity.y);

        // 좌우 플립
        if (moveX > 0.01f) sr.flipX = false;
        else if (moveX < -0.01f) sr.flipX = true;

        setState(CharacterStateEnum.Moving);
    }

    private bool IsOpponentOnLeft()
    {
        var targets = GameObject.FindGameObjectsWithTag(OpponentType.ToString()); // "Player"/"Enemy" 태그 사용
        if (targets == null || targets.Length == 0) return false; // 기본값: 오른쪽

        Transform me = transform;
        Transform nearest = null;
        float bestAbsDx = float.MaxValue;

        for (int i = 0; i < targets.Length; i++)
        {
            float dx = targets[i].transform.position.x - me.position.x;
            float abs = Mathf.Abs(dx);
            if (abs < bestAbsDx)
            {
                bestAbsDx = abs;
                nearest = targets[i].transform;
            }
        }
        if (nearest == null) return false;

        float dxNearest = nearest.position.x - me.position.x;
        return dxNearest < 0f; // 왼쪽이면 true
    }

    private void FaceOpponentLR()
    {
        if (sr == null) return;
        // 프로젝트 컨벤션: 오른쪽을 볼 때 flipX=false, 왼쪽이면 true
        sr.flipX = IsOpponentOnLeft();
    }
    protected void prepareSkill(Skills skill)
    {
        FaceOpponentLR(); //스킬 시전 전 상대방 바라보기

        if (castingSkill != Skills.Attack) return; //스킬 중간 취소 불가 시 return
        if (ManagerObject.skillInfoM.TryBeginCooldown(skill) == false) return;//쿨타임 중이면 스킬 시전 불가

        castingSkill = skill; //현재 시전 중인 스킬 설정

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        setState(CharacterStateEnum.UsingSkill); //여기서 순간적으로 input에서 모든 키 해제, 스킬 시전 중 아무것도 못하도록
        skillCoroutine = StartCoroutine(castSkill(skill));

    }
    protected IEnumerator castSkill(Skills skill)
    {

        yield return new WaitForSeconds(ManagerObject.skillInfoM.attackSkillData[skill].skillCastingTime); //캐스팅 시간 대기

        //스킬의 투사체 프리팹 소환
        //TODO JYW 투사체를 하나 발사하는게 아니라 SkillDataBaseManager에서 설정한 방식으로 발사, shoot(CharacterTypeEnum, Skill) 함수로
        ManagerObject.skillInfoM.shoot(CharacterTypeEnum, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), skill);


        skillCoroutine = null;
        castingSkill = Skills.Attack; //스킬 시전 후 다시 일반 공격 준비 상태로
        prepareSkill(Skills.Attack);

    }
}
