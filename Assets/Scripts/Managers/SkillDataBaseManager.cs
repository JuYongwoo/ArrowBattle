using System;
using System.Collections.Generic;
using UnityEngine;

public enum Skills
{
    Attack,
    Skill1, Skill2, Skill3, Skill4, Skill5
}

public class SkillDataBaseManager
{
    // 매핑된 스킬 데이터 (이미 사용 중)
    public Dictionary<Skills, SkillDataSO> attackSkillData;

    // 쿨다운 종료 시각(Time.time 기준)
    private readonly Dictionary<Skills, float> _cooldownEnd = new();

    // 쿨다운 이벤트
    public event Action<Skills, float> CooldownStarted; // (skill, durationSec)
    public event Action<Skills> CooldownEnded;

    // (선택) 쿨다운 끝 이벤트를 코루틴으로 쏘고 싶을 때 바인딩
    private MonoBehaviour _runner;

    public Action<int, float> cooldownUI;

    public void OnAwake()
    {
        attackSkillData = Util.MapEnumToAddressablesByLabels<Skills, SkillDataSO>("SkillData");
        _cooldownEnd.Clear();
    }

    /// <summary> Coroutine 이벤트 발행이 필요하면 한 번만 바인딩 </summary>
    public void BindRunner(MonoBehaviour runner) => _runner = runner;

    /// <summary> 지금 사용 가능? (쿨 종료 시각 <= 현재 시각) </summary>
    public bool CanUse(Skills skill)
    {
        if (!attackSkillData.ContainsKey(skill)) return false;
        return !(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end);
    }

    /// <summary> 남은 쿨타임(초). 없으면 0 </summary>
    public float GetRemaining(Skills skill)
    {
        if (_cooldownEnd.TryGetValue(skill, out var end))
            return Mathf.Max(0f, end - Time.time);
        return 0f;
    }

    /// <summary> 0~1 정규화된 남은 비율(1=쿨 중, 0=사용 가능). skillCoolTime이 0이면 0 </summary>
    public float GetRemaining01(Skills skill)
    {
        if (!attackSkillData.TryGetValue(skill, out var so)) return 0f;
        float dur = Mathf.Max(so.skillCoolTime, 0f);
        if (dur <= 0f) return 0f;
        return Mathf.Clamp01(GetRemaining(skill) / dur);
    }

    /// <summary>
    /// 사용 시도: 가능하면 쿨 시작하고 true 반환. 불가(쿨 중)면 false.
    /// 성공 시 CooldownStarted 이벤트를 즉시 발행.
    /// </summary>
    public bool TryBeginCooldown(Skills skill)
    {
        if (!attackSkillData.TryGetValue(skill, out var so)) return false;

        float now = Time.time;
        float dur = Mathf.Max(so.skillCoolTime, 0f);

        if (_cooldownEnd.TryGetValue(skill, out var end) && now < end)
            return false; // 아직 쿨 중

        float newEnd = now + dur;
        _cooldownEnd[skill] = newEnd;

        // 시작 이벤트(예: SkillPanel.StartCooldown로 연결)
        CooldownStarted?.Invoke(skill, dur);

        // (선택) 끝 이벤트를 쓰고 싶다면 러너가 있을 때만 코루틴으로 발행
        if (_runner != null && dur > 0f)
            _runner.StartCoroutine(CoEmitEndAfter(skill, dur));


        cooldownUI((int)skill, so.skillCoolTime);
        return true;
    }

    public void ResetCooldown(Skills skill)
    {
        _cooldownEnd.Remove(skill);
        CooldownEnded?.Invoke(skill);
    }

    public void ResetAll()
    {
        _cooldownEnd.Clear();
        // 필요하다면 모든 스킬에 대해 End 이벤트 쏘고 싶을 때 반복 발행 가능
    }

    private System.Collections.IEnumerator CoEmitEndAfter(Skills skill, float dur)
    {
        yield return new WaitForSeconds(dur);
        // 여전히 쿨이 끝났는지 확인(중간에 리셋 가능)
        if (GetRemaining(skill) <= 0f) CooldownEnded?.Invoke(skill);
    }
}
