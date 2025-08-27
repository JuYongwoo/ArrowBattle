using System.Collections.Generic;

// JYW 스킬 매니저는 스킬에 대한 정보를 관리
// 스킬 쿨타임, 데미지, 연출 등을 관리
// 연출은 SO에서 유도, 회전, 다중 소환 등등 선택 가능, 조합하여 연출 제작가능 하도록

public enum Skills
{
    Attack,
    Skill1,
    Skill2,
    Skill3,
    Skill4,
    Skill5
}
public class SkillDataBaseManager
{

    public Dictionary<Skills,SkillDataSO> attackSkillData;

    public void OnAwake()
    {
        attackSkillData = Util.MapEnumToAddressablesByLabels<Skills, SkillDataSO>("SkillData");

    }

}
