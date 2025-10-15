using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CharacterBase : MonoBehaviour
{
    protected CharacterStateEnum state = CharacterStateEnum.Moving;
    protected CharacterStatManager stat { get; private set; }
    protected Animator anim;
    private SpriteRenderer sr;
    protected Coroutine skillCoroutine;

    private readonly Dictionary<Skill, float> _cooldownEnd = new(); //각 캐릭터마다 가지는 쿨다운 정보 딕셔너리

    protected abstract CharacterTypeEnumByTag CharacterTypeEnum { get; }
    protected CharacterTypeEnumByTag OpponentType;
    public Skill castingSkill;

    protected virtual void Awake()
    {
        stat = new CharacterStatManager(CharacterTypeEnum);
        anim = GetComponentInChildren<Animator>();
        sr = Util.GetObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>();
        OpponentType = (CharacterTypeEnum == CharacterTypeEnumByTag.Player) ? CharacterTypeEnumByTag.Enemy : CharacterTypeEnumByTag.Player;
        _cooldownEnd.Clear();

    }

    protected virtual void Start()
    {
        SetState(CharacterStateEnum.Idle);
    }

    public virtual void GetDamaged(float damageAmount)
    {
        stat.DeltaHP(-damageAmount);
        if(stat.Current.CurrentHP <= 0)
        {
            stat.Current.CurrentHP = 0;
        }
        ManagerObject.instance.audioM.PlayAudioClip(stat.Current.HitSound, 0.3f, false);
    }

    protected virtual void SetState(CharacterStateEnum s)
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

    protected virtual void Move(float moveX)
    {
        Vector2 dir = new Vector2(moveX, 0f).normalized;
        transform.Translate(dir * stat.Current.CurrentMoveSpeed * Time.deltaTime, Space.World);

        if (moveX > 0.01f) sr.flipX = false;
        else if (moveX < -0.01f) sr.flipX = true;

        SetState(CharacterStateEnum.Moving);
    }

    public void prepareSkill(Skill skill)
    {

        sr.flipX = ABUtil.isOpponentOnLeft(gameObject, GameObject.FindGameObjectsWithTag(OpponentType.ToString()));

        if (TryBeginCooldown(skill) == false) return;

        _cooldownEnd[skill] = Time.time + ManagerObject.instance.resourceManager.SkillDatas.Result.GetSkillDataById(skill).skillCoolTime;

        castingSkill = skill;

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        SetState(CharacterStateEnum.UsingSkill);

        skillCoroutine = StartCoroutine(castSkill(skill));
    }

    private IEnumerator castSkill(Skill skill)
    {

        yield return new WaitForSeconds(ManagerObject.instance.resourceManager.SkillDatas.Result.GetSkillDataById(skill).skillCastingTime);

        ManagerObject.instance.skillInfoM.Shoot(CharacterTypeEnum, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), skill);

        skillCoroutine = null;
        prepareSkill(Skill.Attack);
        castingSkill = Skill.Attack;

    }

    protected virtual bool TryBeginCooldown(Skill skill)
    {

        if (_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end) return false; //아직 쿨타임이 안끝남
        else return true;

    }

}
