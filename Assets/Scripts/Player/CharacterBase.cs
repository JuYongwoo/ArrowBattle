using System.Collections;
using UnityEngine;

public enum CharacterStateEnum //Enum���� ���� ���� //int�� ĳ�����Ͽ� ���
{
    Idle, //Attacking�� �ϱ� ���� 1ƽ
    Moving,
    Attacking,
    UsingSkill
}

public enum CharacterTypeEnum
{
    Player,
    Enemy
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
    public abstract void getDamaged(float damageAmount); //�ڽ�Ŭ����  getDamaged ���� ����

    protected abstract CharacterTypeEnum CharacterTypeEnum { get; } //Player, Enemy ���� //�߻� ������Ƽ �ڽ� Ŭ���� ���� ����

    protected virtual void Awake() //virtual�� �����Ͽ� �ڽ� Ŭ�������� override �����ϵ��� ������ ����
    {
        if(CharacterTypeEnum == CharacterTypeEnum.Player) stat = new CharacterStatManager("PlayerData");
        else if (CharacterTypeEnum == CharacterTypeEnum.Enemy) stat = new CharacterStatManager("EnemyData");
        anim = GetComponentInChildren<Animator>(); // Animator ĳ��
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D ĳ��
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer ĳ��


    }

    protected virtual void Start()
    {
        setState(CharacterStateEnum.Idle); //ó�� ���´� Idle
    }

    public void setState(CharacterStateEnum s)
    {
        if (state == s || (state == CharacterStateEnum.Attacking && s == CharacterStateEnum.Idle && skillCoroutine != null)) return; //�̹� ���� ���°ų� Attacking ���¿��� ������ �� �� Idle�� �ٲ�� ��� ����
        state = s;


        if (state == CharacterStateEnum.Idle) //�ƹ� �ൿ ���� ���� �� �Ϲ� ����
        {
            if (skillCoroutine == null) startSkill(Skills.Attack);
        }
        else if(state == CharacterStateEnum.Moving || state == CharacterStateEnum.UsingSkill) //�̵�, ��ų ����ϴ� ���� �Ϲ� ���� ���
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

        // �¿� �ø�
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
            //���⼭ ���������� input���� ��� Ű ����, ��ų ���� �� �ƹ��͵� ���ϵ���
        }
        yield return new WaitForSeconds(ManagerObject.skillInfoM.attackSkillData[skill].skillCastingTime); //ĳ���� �ð� ���

        //��ų�� ����ü ������ ��ȯ
        GameObject projectile = Instantiate(ManagerObject.skillInfoM.attackSkillData[skill].skillProjectile, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), Quaternion.identity);
        projectile.GetComponent<SkillProjectile>().SetProjectile(CharacterTypeEnum, skill);
        skillCoroutine = null;
        setState(CharacterStateEnum.Idle);

    }
}
