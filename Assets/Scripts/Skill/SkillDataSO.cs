using UnityEngine;


    [System.Serializable]
public enum SkillProjectileMovingType
{
    Straight,   // 일직선
    Parabola,   // 포물선(2차 베지어)
    SineWave,   // 사인 웨이브(횡진동)
    ZigZag,     // 지그재그(삼각파)
    Boomerang,   // 왕복(타깃 방향으로 갔다가 원점 복귀)
    Rain
}

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
    public float projectileCount;
    public bool isGuided;// 유도인가
    public bool isShowPortrait;// 사용 시 유닛 초상화 연출

}