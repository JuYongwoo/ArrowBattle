using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStateEnum //Enum���� ���� ���� //int�� ĳ�����Ͽ� ���
{
    Idle, //Attacking�� �ϱ� ���� 1ƽ
    Moving,
    UsingSkill
}



//JYW Enemy�� Player�� ���� ����� ���� �θ� Ŭ�����Դϴ�.

public abstract class CharacterBase : MonoBehaviour
{
    protected CharacterStateEnum state = CharacterStateEnum.Moving;
    protected CharacterStatManager stat { get; private set; }
    protected Animator anim;
    protected Rigidbody2D rb;
    private SpriteRenderer sr;
    protected Coroutine skillCoroutine;

    // ��Ÿ��
    private readonly Dictionary<Skill, float> _cooldownEnd = new(); //각 캐릭터마다 가지는 쿨다운 정보 딕셔너리



    protected abstract CharacterTypeEnumByTag CharacterTypeEnum { get; }
    protected CharacterTypeEnumByTag OpponentType;
    public Skill castingSkill;

    protected virtual void Awake()
    {
        stat = new CharacterStatManager(CharacterTypeEnum);
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = Util.GetObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>();
        OpponentType = (CharacterTypeEnum == CharacterTypeEnumByTag.Player) ? CharacterTypeEnumByTag.Enemy : CharacterTypeEnumByTag.Player;
        _cooldownEnd.Clear();

    }

    protected virtual void Start()
    {
        setState(CharacterStateEnum.Idle);
    }


    public virtual void getDamaged(float damageAmount)
    {
        stat.deltaHP(-damageAmount);
        if(stat.Current.CurrentHP <= 0)
        {
            stat.Current.CurrentHP = 0;
        }
        ManagerObject.instance.audioM.PlayAudioClip(stat.Current.HitSound, 0.3f, false);
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

    public void move(float moveX)
    {
        rb.linearVelocity = new Vector2(moveX * stat.Current.CurrentMoveSpeed, rb.linearVelocity.y);

        // �¿� �ø�
        if (moveX > 0.01f) sr.flipX = false;
        else if (moveX < -0.01f) sr.flipX = true;

        setState(CharacterStateEnum.Moving);
    }

    private bool isOpponentOnLeft()
    {
        var targets = GameObject.FindGameObjectsWithTag(OpponentType.ToString());
        if (targets == null || targets.Length == 0) return false;

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
        return dxNearest < 0f;
    }

    public void prepareSkill(Skill skill)
    {
        sr.flipX = isOpponentOnLeft();

        if (tryBeginCooldown(skill) == false) return;

        castingSkill = skill;

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        setState(CharacterStateEnum.UsingSkill);
        skillCoroutine = StartCoroutine(castSkill(skill));

    }
    protected IEnumerator castSkill(Skill skill)
    {

        yield return new WaitForSeconds(ManagerObject.instance.resourceManager.attackSkillData[skill].Result.skillCastingTime);

        ManagerObject.instance.skillInfoM.shoot(CharacterTypeEnum, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), skill);


        skillCoroutine = null;
        castingSkill = Skill.Attack;
        prepareSkill(Skill.Attack);

    }

    public bool isCanUse(Skill skill)
    {
        if (!ManagerObject.instance.resourceManager.attackSkillData.ContainsKey(skill)) return false;
        return !(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end);
    }

    protected virtual bool tryBeginCooldown(Skill skill)
    {
        if (!ManagerObject.instance.resourceManager.attackSkillData.TryGetValue(skill, out var so)) return false;

        float now = Time.time;
        float dur = Mathf.Max(so.Result.skillCoolTime, 0f);
        if (_cooldownEnd.TryGetValue(skill, out var end) && now < end) return false;

        _cooldownEnd[skill] = now + dur;
        ManagerObject.instance.actionManager.CooldownStarted?.Invoke(skill, dur);
        if (dur > 0f) ManagerObject.instance.skillInfoM.GetRunner().StartCoroutine(coEmitEndAfter(skill, dur));

        //ManagerObject.instance.actionManager.CooldownUI?.Invoke((int)skill, so.Result.skillCoolTime);
        return true;
    }

    private IEnumerator coEmitEndAfter(Skill skill, float dur)
    {
        yield return new WaitForSeconds(dur);
        if (!(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end))
            ManagerObject.instance.actionManager.CooldownEnded?.Invoke(skill);
    }
}
