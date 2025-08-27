using System.Collections.Generic;

// JYW ��ų �Ŵ����� ��ų�� ���� ������ ����
// ��ų ��Ÿ��, ������, ���� ���� ����
// ������ SO���� ����, ȸ��, ���� ��ȯ ��� ���� ����, �����Ͽ� ���� ���۰��� �ϵ���

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
