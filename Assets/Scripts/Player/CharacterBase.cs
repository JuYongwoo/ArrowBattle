using UnityEngine;
using UnityEngine.Playables;

public enum CharacterAnimationEnum //Enum으로 상태 관리 //int로 캐스팅하여 사용
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

    protected abstract CharacterTypeEnum CharacterTypeEnum { get; } //Player, Enemy 구분 //추상 프로퍼티로 선언하여 자식 클래스에서 반드시 구현하도록 강제

    protected virtual void Awake() //Awake는 virtual로 선언하여 자식 클래스에서 override 가능하도록 다형성 보장
    {
        if(CharacterTypeEnum == CharacterTypeEnum.Player) stat = new CharacterStatManager("PlayerData");
        else if (CharacterTypeEnum == CharacterTypeEnum.Enemy) stat = new CharacterStatManager("EnemyData");
        anim = GetComponentInChildren<Animator>(); // Animator 캐싱
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 캐싱
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱



    }
    public void getDamaged(float damageAmount) //SkillProjectile이 어떤 캐릭터든 공동 호출
    {
        stat.deltaHP(-damageAmount);
        ManagerObject.audioM.PlayAudioClip(stat.Current.HitSound);
    }
    protected void SetCharacterMotion(CharacterAnimationEnum s)
    {
        if (state == s) return; //이미 같은 상태라면 변경하지 않음
        state = s;
        int animState = (int)s;

        anim.SetInteger("State", animState);
    }

    protected void Move(float moveX)
    {
        rb.velocity = new Vector2(moveX * stat.Current.CurrentMoveSpeed, rb.velocity.y);

        // 좌우 플립
        if (moveX > 0.01f) sr.flipX = false;
        else if (moveX < -0.01f) sr.flipX = true;

        SetCharacterMotion(CharacterAnimationEnum.Moving);
    }

    protected void Skill(Skills skill)
    {
        //스킬의 투사체 프리팹 소환
        GameObject projectile = Instantiate(ManagerObject.skillInfoM.attackSkillData[skill].skillProjectile, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), Quaternion.identity);
        projectile.GetComponent<SkillProjectile>().SetProjectile(CharacterTypeEnum, skill);
        SetCharacterMotion(CharacterAnimationEnum.UsingSkill);

    }
}
