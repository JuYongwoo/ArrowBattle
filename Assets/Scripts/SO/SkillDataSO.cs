using UnityEngine;


[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/SkillData")]
public class SkillDataSO : ScriptableObject //이 SO를 이용하여 세 개의 스킬 데이터를 할당
{
    public string skillName; //스킬 이름
    public float skillDamager; //스킬 데미지
    public float skillCooldown; //스킬 쿨타임
    public Sprite skillIcon; //스킬 아이콘

}