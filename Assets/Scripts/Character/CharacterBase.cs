using System;
using System.Collections;
using System.Collections.Generic;
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
    private AudioClip hitSound;

    // 쿨타임
    private readonly Dictionary<Skill, float> _cooldownEnd = new();
    public event Action<Skill, float> CooldownStarted;
    public event Action<Skill> CooldownEnded;
    public Action<int, float> cooldownUI;


    protected abstract CharacterTypeEnumByTag CharacterTypeEnum { get; } //Player, Enemy 구분 //추상 프로퍼티 자식 클래스 구현 강제
    protected CharacterTypeEnumByTag OpponentType; //상대방 타입, Awake에서 자동 설정
    protected Skill castingSkill; //현재 시전 중인 스킬

    protected virtual void Awake() //virtual로 선언하여 자식 클래스에서 override 가능하도록 다형성 보장
    {
        stat = new CharacterStatManager($"{Enum.GetName(typeof(CharacterTypeEnumByTag), CharacterTypeEnum)}Data");
        anim = GetComponentInChildren<Animator>(); // Animator 캐싱
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 캐싱
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱
        OpponentType = (CharacterTypeEnum == CharacterTypeEnumByTag.Player) ? CharacterTypeEnumByTag.Enemy : CharacterTypeEnumByTag.Player;
        _cooldownEnd.Clear();

    }

    protected virtual void Start()
    {
        setState(CharacterStateEnum.Idle); //처음 상태는 Idle
    }


    public virtual void getDamaged(float damageAmount)
    {
        stat.deltaHP(-damageAmount);
        if(stat.Current.CurrentHP <= 0)
        {
            stat.Current.CurrentHP = 0;
            return;
        }
        ManagerObject.audioM.PlayAudioClip(stat.Current.HitSound);
    }

    public void setState(CharacterStateEnum s)
    {
        if (state == s) return;
        state = s;

        if(state == CharacterStateEnum.Moving || state == CharacterStateEnum.UsingSkill) 
            
        {
            if (skillCoroutine != null)
            {
                StopCoroutine(skillCoroutine);
                skillCoroutine = null;
            }
        }

        anim.SetInteger("State", (int)s);
    }

    protected void move(float moveX)
    {
        rb.velocity = new Vector2(moveX * stat.Current.CurrentMoveSpeed, rb.velocity.y);

        // 좌우 플립
        if (moveX > 0.01f) sr.flipX = false;
        else if (moveX < -0.01f) sr.flipX = true;

        setState(CharacterStateEnum.Moving);
    }

    private bool isOpponentOnLeft()
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

    protected void prepareSkill(Skill skill)
    {
        sr.flipX = isOpponentOnLeft(); //스킬 시전 전 상대방 바라보기

        if (tryBeginCooldown(skill) == false) return;//쿨타임 중이면 스킬 시전 불가

        castingSkill = skill; //현재 시전 중인 스킬 설정

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        setState(CharacterStateEnum.UsingSkill); //여기서 순간적으로 input에서 모든 키 해제, 스킬 시전 중 아무것도 못하도록
        skillCoroutine = StartCoroutine(castSkill(skill));

    }
    protected IEnumerator castSkill(Skill skill)
    {

        yield return new WaitForSeconds(ManagerObject.skillInfoM.attackSkillData[skill].skillCastingTime); //캐스팅 시간 대기

        //스킬의 투사체 프리팹 소환
        //TODO JYW 투사체를 하나 발사하는게 아니라 SkillDataBaseManager에서 설정한 방식으로 발사, shoot(CharacterTypeEnum, Skill) 함수로
        ManagerObject.skillInfoM.shoot(CharacterTypeEnum, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), skill);


        skillCoroutine = null;
        castingSkill = Skill.Attack; //스킬 시전 후 다시 일반 공격 준비 상태로
        prepareSkill(Skill.Attack);

    }

    public bool isCanUse(Skill skill)
    {
        if (!ManagerObject.skillInfoM.attackSkillData.ContainsKey(skill)) return false;
        return !(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end);
    }

    public bool tryBeginCooldown(Skill skill)
    {
        if (!ManagerObject.skillInfoM.attackSkillData.TryGetValue(skill, out var so)) return false;

        float now = Time.time;
        float dur = Mathf.Max(so.skillCoolTime, 0f);
        if (_cooldownEnd.TryGetValue(skill, out var end) && now < end) return false;

        _cooldownEnd[skill] = now + dur;
        CooldownStarted?.Invoke(skill, dur);
        if (dur > 0f) ManagerObject.skillInfoM.GetRunner().StartCoroutine(coEmitEndAfter(skill, dur));

        cooldownUI?.Invoke((int)skill, so.skillCoolTime);
        return true;
    }

    private IEnumerator coEmitEndAfter(Skill skill, float dur)
    {
        yield return new WaitForSeconds(dur);
        if (!(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end))
            CooldownEnded?.Invoke(skill);
    }
}
