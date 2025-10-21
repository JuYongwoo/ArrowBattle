using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/SkillData")]
public class SkillDatasSO : ScriptableObject
{
    [SerializeField]
    private SkillData[] skillDatas;

    public SkillData GetSkillDataById(Skill skillEnum)
    {
        foreach (var data in skillDatas)
        {
            if (data.SkillEnum == skillEnum)
                return data;
        }
        return null;
    }
}

[System.Serializable]
public class SkillData
{
    [SerializeField]
    private Skill skillEnum; // ¿¹: "Fireball", "IceBlast"
    [SerializeField]
    private GameObject skillProjectile;
    [SerializeField]
    private Sprite skillIcon;
    [SerializeField]
    private AudioClip skillSound;
    [SerializeField]
    private float skillDamage;
    [SerializeField]
    private float skillCoolTime;
    [SerializeField]
    private float skillCastingTime;
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private SkillProjectileMovingType skillProjectileMovingType;

    public Skill SkillEnum => skillEnum;
    public GameObject SkillProjectile => skillProjectile;
    public Sprite SkillIcon => skillIcon;
    public AudioClip SkillSound => skillSound;
    public float SkillDamage => skillDamage;
    public float SkillCoolTime => skillCoolTime;
    public float SkillCastingTime => skillCastingTime;
    public float ProjectileSpeed => projectileSpeed;
    public SkillProjectileMovingType SkillProjectileMovingType => skillProjectileMovingType;
}
