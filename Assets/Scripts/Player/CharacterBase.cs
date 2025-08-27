using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum CharacterStateEnum //Enum���� ���� ���� //int�� ĳ�����Ͽ� ���
{
    Idle, //Attacking�� �ϱ� ���� 1ƽ
    Moving,
    UsingSkill
}

public enum CharacterTypeEnumByTag
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

    protected abstract CharacterTypeEnumByTag CharacterTypeEnum { get; } //Player, Enemy ���� //�߻� ������Ƽ �ڽ� Ŭ���� ���� ����
    protected CharacterTypeEnumByTag OpponentType; //���� Ÿ��, Awake���� �ڵ� ����
    protected Skills castingSkill; //���� ���� ���� ��ų

    protected virtual void Awake() //virtual�� �����Ͽ� �ڽ� Ŭ�������� override �����ϵ��� ������ ����
    {
        stat = new CharacterStatManager($"{Enum.GetName(typeof(CharacterTypeEnumByTag), CharacterTypeEnum)}Data");
        anim = GetComponentInChildren<Animator>(); // Animator ĳ��
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D ĳ��
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer ĳ��
        OpponentType = (CharacterTypeEnum == CharacterTypeEnumByTag.Player) ? CharacterTypeEnumByTag.Enemy : CharacterTypeEnumByTag.Player;


    }

    protected virtual void Start()
    {
        setState(CharacterStateEnum.Idle); //ó�� ���´� Idle
    }
    protected virtual void Update()
    {
        if (state == CharacterStateEnum.Idle) //�ƹ� �ൿ ���� ���� �� �Ϲ� ����
        {
            if (skillCoroutine == null) prepareSkill(Skills.Attack);
        }
    }

    public void setState(CharacterStateEnum s)
    {
        if (state == s) return;
        state = s;

        if(state == CharacterStateEnum.Moving || state == CharacterStateEnum.UsingSkill) //�̵�, ��ų ����ϴ� ���� �Ϲ� ���� ���
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

    private bool IsOpponentOnLeft()
    {
        var targets = GameObject.FindGameObjectsWithTag(OpponentType.ToString()); // "Player"/"Enemy" �±� ���
        if (targets == null || targets.Length == 0) return false; // �⺻��: ������

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
        return dxNearest < 0f; // �����̸� true
    }

    private void FaceOpponentLR()
    {
        if (sr == null) return;
        // ������Ʈ ������: �������� �� �� flipX=false, �����̸� true
        sr.flipX = IsOpponentOnLeft();
    }
    protected void prepareSkill(Skills skill)
    {
        FaceOpponentLR(); //��ų ���� �� ���� �ٶ󺸱�

        if (castingSkill != Skills.Attack) return; //��ų �߰� ��� �Ұ� �� return
        if (ManagerObject.skillInfoM.TryBeginCooldown(skill) == false) return;//��Ÿ�� ���̸� ��ų ���� �Ұ�

        castingSkill = skill; //���� ���� ���� ��ų ����

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        setState(CharacterStateEnum.UsingSkill); //���⼭ ���������� input���� ��� Ű ����, ��ų ���� �� �ƹ��͵� ���ϵ���
        skillCoroutine = StartCoroutine(castSkill(skill));

    }
    protected IEnumerator castSkill(Skills skill)
    {

        yield return new WaitForSeconds(ManagerObject.skillInfoM.attackSkillData[skill].skillCastingTime); //ĳ���� �ð� ���

        //��ų�� ����ü ������ ��ȯ
        //TODO JYW ����ü�� �ϳ� �߻��ϴ°� �ƴ϶� SkillDataBaseManager���� ������ ������� �߻�, shoot(CharacterTypeEnum, Skill) �Լ���
        ManagerObject.skillInfoM.shoot(CharacterTypeEnum, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), skill);


        skillCoroutine = null;
        castingSkill = Skills.Attack; //��ų ���� �� �ٽ� �Ϲ� ���� �غ� ���·�
        prepareSkill(Skills.Attack);

    }
}
