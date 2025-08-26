using UnityEngine;
using UnityEngine.Playables;

public enum CharacterAnimationEnum //Enum���� ���� ���� //int�� ĳ�����Ͽ� ���
{
    Idle,
    Moving,
    UsingSkill
}

public enum CharacterTypeEnum
{
    Player,
    Enemy
}

public abstract class CharacterBase : MonoBehaviour
{
    protected CharacterAnimationEnum state;
    protected CharacterStatManager stat;
    protected Animator anim;
    protected Rigidbody2D rb;
    private SpriteRenderer sr;

    protected abstract CharacterTypeEnum CharacterTypeEnum { get; } //Player, Enemy ���� //�߻� ������Ƽ�� �����Ͽ� �ڽ� Ŭ�������� �ݵ�� �����ϵ��� ����

    protected virtual void Awake() //Awake�� virtual�� �����Ͽ� �ڽ� Ŭ�������� override �����ϵ��� ������ ����
    {
        if(CharacterTypeEnum == CharacterTypeEnum.Player) stat = new CharacterStatManager("PlayerData");
        else if (CharacterTypeEnum == CharacterTypeEnum.Enemy) stat = new CharacterStatManager("EnemyData");
        anim = GetComponentInChildren<Animator>(); // Animator ĳ��
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D ĳ��
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer ĳ��



    }
    public void getDamaged(float damageAmount) //SkillProjectile�� � ĳ���͵� ���� ȣ��
    {
        stat.deltaHP(-damageAmount);
        ManagerObject.audioM.PlayAudioClip(stat.Current.HitSound);
    }
    protected void SetCharacterMotion(CharacterAnimationEnum s)
    {
        if (state == s) return; //�̹� ���� ���¶�� �������� ����
        state = s;
        int animState = (int)s;

        anim.SetInteger("State", animState);
    }

    protected void Move(float moveX)
    {
        rb.velocity = new Vector2(moveX * stat.Current.CurrentMoveSpeed, rb.velocity.y);

        // �¿� �ø�
        if (moveX > 0.01f) sr.flipX = false;
        else if (moveX < -0.01f) sr.flipX = true;

        SetCharacterMotion(CharacterAnimationEnum.Moving);
    }

    protected void Skill(Skills skill)
    {
        //��ų�� ����ü ������ ��ȯ
        GameObject projectile = Instantiate(ManagerObject.skillInfoM.attackSkillData[skill].skillProjectile, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), Quaternion.identity);
        projectile.GetComponent<SkillProjectile>().SetProjectile(CharacterTypeEnum, skill);
        SetCharacterMotion(CharacterAnimationEnum.UsingSkill);

    }
}
