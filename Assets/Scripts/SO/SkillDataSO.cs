using UnityEngine;

[System.Serializable]




[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/SkillData")]
public class SkillDataSO : ScriptableObject //이 SO를 이용하여 세 개의 스킬 데이터를 할당
{

    public GameObject skillProjectile; //스킬 투사체 프리팹
    public Sprite skillIcon; //스킬 아이콘 이미지
    public AudioClip skillSound; //스킬 사운드
    public float skillDamage; //스킬 데미지
    public float skillCoolTime; //스킬 쿨타임
    public float skillCastingTime; //스킬 시전 시간
    public float projectileSpeed;// 투사체 속도

    public SkillProjectileMovingType skillProjectileMovingType; //스킬 투사체 이동 타입, 일직선, 포물선

}