using System.Collections;
using UnityEngine;

public enum CharacterStateEnum //Enum으로 상태 관리 //int로 캐스팅하여 사용
{
    Idle, //Attacking을 하기 위한 1틱
    Moving,
    Attacking,
    UsingSkill
}

public enum CharacterTypeEnum
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

    protected abstract CharacterTypeEnum CharacterTypeEnum { get; } //Player, Enemy 구분 //추상 프로퍼티 자식 클래스 구현 강제

    protected virtual void Awake() //virtual로 선언하여 자식 클래스에서 override 가능하도록 다형성 보장
    {
        if(CharacterTypeEnum == CharacterTypeEnum.Player) stat = new CharacterStatManager("PlayerData");
        else if (CharacterTypeEnum == CharacterTypeEnum.Enemy) stat = new CharacterStatManager("EnemyData");
        anim = GetComponentInChildren<Animator>(); // Animator 캐싱
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 캐싱
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱


    }

    protected virtual void Start()
    {
        setState(CharacterStateEnum.Idle); //처음 상태는 Idle
    }

    public void setState(CharacterStateEnum s)
    {
        if (state == s || (state == CharacterStateEnum.Attacking && s == CharacterStateEnum.Idle && skillCoroutine != null)) return; //이미 같은 상태거나 Attacking 상태에서 공격중 일 때 Idle로 바뀌는 경우 무시
        state = s;


        if (state == CharacterStateEnum.Idle) //아무 행동 하지 않을 시 일반 공격
        {
            if (skillCoroutine == null) startSkill(Skills.Attack);
        }
        else if(state == CharacterStateEnum.Moving || state == CharacterStateEnum.UsingSkill) //이동, 스킬 사용하는 순간 일반 공격 취소
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


    protected void startSkill(Skills skill)
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }
        skillCoroutine = StartCoroutine(castSkill(skill));

    }
    protected IEnumerator castSkill(Skills skill)
    {
        if(skill == Skills.Attack)
        {
            setState(CharacterStateEnum.Attacking);
        }else
        {
            setState(CharacterStateEnum.UsingSkill);
            //여기서 순간적으로 input에서 모든 키 해제, 스킬 시전 중 아무것도 못하도록
        }
        yield return new WaitForSeconds(ManagerObject.skillInfoM.attackSkillData[skill].skillCastingTime); //캐스팅 시간 대기

        //스킬의 투사체 프리팹 소환
        GameObject projectile = Instantiate(ManagerObject.skillInfoM.attackSkillData[skill].skillProjectile, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), Quaternion.identity);
        projectile.GetComponent<SkillProjectile>().SetProjectile(CharacterTypeEnum, skill);
        skillCoroutine = null;
        setState(CharacterStateEnum.Idle);

    }
}
