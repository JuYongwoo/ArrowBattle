using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/SkillData")]
public class SkillDatasSO : ScriptableObject
{
    public SkillData[] skillDatas;

    public SkillData GetSkillDataById(Skill skillEnum)
    {
        foreach (var data in skillDatas)
        {
            if (data.skillEnum == skillEnum)
                return data;
        }
        return null;
    }
}

[System.Serializable]
public class SkillData
{
    public Skill skillEnum; // ¿¹: "Fireball", "IceBlast"
    public GameObject skillProjectile;
    public Sprite skillIcon;
    public AudioClip skillSound;
    public float skillDamage;
    public float skillCoolTime;
    public float skillCastingTime;
    public float projectileSpeed;
    public SkillProjectileMovingType skillProjectileMovingType;
}
