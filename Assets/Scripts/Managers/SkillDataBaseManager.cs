using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;

// JYW ��ų �Ŵ����� ��ų�� ���� ������ ����
// ��ų ��Ÿ��, ������, ���� ���� ����
// ������ SO���� ����, ȸ��, ���� ��ȯ ��� ���� ����, �����Ͽ� ���� ���۰��� �ϵ���

public enum Skills
{
    Attack,
    Skill1,
    Skill2,
    Skill3
}
public class SkillDataBaseManager
{

    public Dictionary<Skills,SkillDataSO> attackSkillData;

    public void OnAwake()
    {
        attackSkillData = Util.MapEnumToAddressablesByLabels<Skills, SkillDataSO>("SkillData");

    }

}
