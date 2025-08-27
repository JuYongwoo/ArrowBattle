using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public enum Skill
{
    Attack,
    Skill1, Skill2, Skill3, Skill4, Skill5
}

public class SkillDataBaseManager
{
    // ���ε� ��ų ������ (�̹� ��� ��)
    public Dictionary<Skill, SkillDataSO> attackSkillData;

    // ��ٿ� ���� �ð�(Time.time ����)
    private readonly Dictionary<Skill, float> _cooldownEnd = new();

    // ��ٿ� �̺�Ʈ
    public event Action<Skill, float> CooldownStarted; // (skill, durationSec)
    public event Action<Skill> CooldownEnded;

    // (����) ��ٿ� �� �̺�Ʈ�� �ڷ�ƾ���� ��� ���� �� ���ε�
    private MonoBehaviour _runner;

    public Action<int, float> cooldownUI;

    public void OnAwake()
    {
        attackSkillData = Util.MapEnumToAddressablesByLabels<Skill, SkillDataSO>("SkillData");
        _cooldownEnd.Clear();
    }

    /// <summary> Coroutine �̺�Ʈ ������ �ʿ��ϸ� �� ���� ���ε� </summary>
    public void BindRunner(MonoBehaviour runner) => _runner = runner;

    /// <summary> ���� ��� ����? (�� ���� �ð� <= ���� �ð�) </summary>
    public bool CanUse(Skill skill)
    {
        if (!attackSkillData.ContainsKey(skill)) return false;
        return !(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end);
    }

    /// <summary> ���� ��Ÿ��(��). ������ 0 </summary>
    public float GetRemaining(Skill skill)
    {
        if (_cooldownEnd.TryGetValue(skill, out var end))
            return Mathf.Max(0f, end - Time.time);
        return 0f;
    }

    /// <summary> 0~1 ����ȭ�� ���� ����(1=�� ��, 0=��� ����). skillCoolTime�� 0�̸� 0 </summary>
    public float GetRemaining01(Skill skill)
    {
        if (!attackSkillData.TryGetValue(skill, out var so)) return 0f;
        float dur = Mathf.Max(so.skillCoolTime, 0f);
        if (dur <= 0f) return 0f;
        return Mathf.Clamp01(GetRemaining(skill) / dur);
    }

    /// <summary>
    /// ��� �õ�: �����ϸ� �� �����ϰ� true ��ȯ. �Ұ�(�� ��)�� false.
    /// ���� �� CooldownStarted �̺�Ʈ�� ��� ����.
    /// </summary>
    public bool TryBeginCooldown(Skill skill)
    {
        if (!attackSkillData.TryGetValue(skill, out var so)) return false;

        float now = Time.time;
        float dur = Mathf.Max(so.skillCoolTime, 0f);

        if (_cooldownEnd.TryGetValue(skill, out var end) && now < end)
            return false; // ���� �� ��

        float newEnd = now + dur;
        _cooldownEnd[skill] = newEnd;

        // ���� �̺�Ʈ(��: SkillPanel.StartCooldown�� ����)
        CooldownStarted?.Invoke(skill, dur);

        // (����) �� �̺�Ʈ�� ���� �ʹٸ� ���ʰ� ���� ���� �ڷ�ƾ���� ����
        if (_runner != null && dur > 0f)
            _runner.StartCoroutine(CoEmitEndAfter(skill, dur));


        cooldownUI((int)skill, so.skillCoolTime);
        return true;
    }

    public void ResetCooldown(Skill skill)
    {
        _cooldownEnd.Remove(skill);
        CooldownEnded?.Invoke(skill);
    }

    public void ResetAll()
    {
        _cooldownEnd.Clear();
        // �ʿ��ϴٸ� ��� ��ų�� ���� End �̺�Ʈ ��� ���� �� �ݺ� ���� ����
    }

    private System.Collections.IEnumerator CoEmitEndAfter(Skill skill, float dur)
    {
        yield return new WaitForSeconds(dur);
        // ������ ���� �������� Ȯ��(�߰��� ���� ����)
        if (GetRemaining(skill) <= 0f) CooldownEnded?.Invoke(skill);
    }

    public void shoot(CharacterTypeEnumByTag CharacterTypeEnum, Vector3 startPosition, Skill skill)
    {
        GameObject projectile = MonoBehaviour.Instantiate(ManagerObject.skillInfoM.attackSkillData[skill].skillProjectile, startPosition, Quaternion.identity);
        projectile.GetComponent<SkillProjectile>().SetProjectile(CharacterTypeEnum, skill);
    }
}
