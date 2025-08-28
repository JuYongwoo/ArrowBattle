using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Skill
{
    Attack,
    Skill1, Skill2, Skill3, Skill4, Skill5
}

public enum SkillProjectileMovingType
{
    Straight,
    Parabola,
    Rain,     // Down N개 스폰하고 부모는 즉시 종료
    Down,     // 머리 위 고정 스폰, 아래로 낙하만
    StraightThreeMultipleShot // 예시: 3연속 직사
}

public class SkillDataBaseManager
{
    public Dictionary<Skill, SkillDataSO> attackSkillData;

    public int rainDownCount = 6;    // Down 개수
    public float downSpawnHeight = 6f;   // 머리 위 높이
    public float downWidth = 4f;   // 가로 분포 폭
    public float headYOffset = 1.0f; // 머리 기준 오프셋
    public float downFallSpeedMul = 1f;   // 낙하 속도 배수

    // StraightThreeMultipleShot
    public int tripleShots = 3;
    public float tripleInterval = 0.12f;

    // Parabola
    public float parabolaHeightBase = 3.5f;

    // 코루틴 발사용
    private MonoBehaviour _runner;

    // 쿨타임
    private readonly Dictionary<Skill, float> _cooldownEnd = new();
    public event Action<Skill, float> CooldownStarted;
    public event Action<Skill> CooldownEnded;
    public Action<int, float> cooldownUI;

    public void OnAwake()
    {
        attackSkillData = Util.MapEnumToAddressablesByLabels<Skill, SkillDataSO>("SkillData");
        _cooldownEnd.Clear();
    }

    public void BindRunner(MonoBehaviour runner) => _runner = runner;

    // === 쿨타임 편의 ===
    public bool CanUse(Skill skill)
    {
        if (!attackSkillData.ContainsKey(skill)) return false;
        return !(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end);
    }

    public bool TryBeginCooldown(Skill skill)
    {
        if (!attackSkillData.TryGetValue(skill, out var so)) return false;

        float now = Time.time;
        float dur = Mathf.Max(so.skillCoolTime, 0f);
        if (_cooldownEnd.TryGetValue(skill, out var end) && now < end) return false;

        _cooldownEnd[skill] = now + dur;
        CooldownStarted?.Invoke(skill, dur);
        if (_runner != null && dur > 0f) _runner.StartCoroutine(CoEmitEndAfter(skill, dur));

        cooldownUI?.Invoke((int)skill, so.skillCoolTime);
        return true;
    }

    private IEnumerator CoEmitEndAfter(Skill skill, float dur)
    {
        yield return new WaitForSeconds(dur);
        if (!(_cooldownEnd.TryGetValue(skill, out var end) && Time.time < end))
            CooldownEnded?.Invoke(skill);
    }

    // === 발사 ===
    public void shoot(CharacterTypeEnumByTag casterType, Vector3 startPosition, Skill skill)
    {
        var so = attackSkillData[skill];
        var proj = UnityEngine.Object.Instantiate(so.skillProjectile, startPosition, Quaternion.identity)
                    .GetComponent<SkillProjectile>();

        var type = so.skillProjectileMovingType;

        // 타입별로 spawn/tick 및 params 구성 (기본 유도 없음)
        switch (type)
        {
            case SkillProjectileMovingType.Rain:
                // Rain은 Down N개를 만들고 부모는 즉시 종료 (타깃 스냅샷만 사용)
                proj.SetProjectile(
                    casterType, so,
                    spawnAction_Rain, tickAction_NoOp,
                    // params: downCount, downHeight, downWidth, headYOffset, fallSpeedMul
                    rainDownCount, downSpawnHeight, downWidth, headYOffset, downFallSpeedMul
                );
                break;

            case SkillProjectileMovingType.Down:
                // Down 단일
                proj.SetProjectile(
                    casterType, so,
                    spawnAction_Down, tickAction_Down,
                    1, downSpawnHeight, 0f, headYOffset, downFallSpeedMul
                );
                break;

            case SkillProjectileMovingType.Straight:
                proj.SetProjectile(casterType, so, spawnAction_Straight, tickAction_Straight);
                break;

            case SkillProjectileMovingType.Parabola:
                proj.SetProjectile(
                    casterType, so, spawnAction_Parabola, tickAction_Parabola,
                    parabolaHeightBase
                );
                break;

            case SkillProjectileMovingType.StraightThreeMultipleShot:
                proj.SetProjectile(casterType, so, spawnAction_Straight, tickAction_Straight);
                if (_runner != null)
                    _runner.StartCoroutine(CoTripleBurst(casterType, startPosition, so));
                break;

            default:
                // 기본 Straight
                proj.SetProjectile(casterType, so, spawnAction_Straight, tickAction_Straight);
                break;
        }
    }

    private IEnumerator CoTripleBurst(CharacterTypeEnumByTag casterType, Vector3 startPos, SkillDataSO so)
    {
        for (int i = 1; i < Mathf.Max(1, tripleShots); i++)
        {
            yield return new WaitForSeconds(tripleInterval);
            var p = UnityEngine.Object.Instantiate(so.skillProjectile, startPos, Quaternion.identity)
                        .GetComponent<SkillProjectile>();
            p.SetProjectile(casterType, so, spawnAction_Straight, tickAction_Straight);
        }
    }

    // --- 공통 유틸 ---
    static Vector3 DirFromTo(Vector3 from, Vector3 to)
    {
        var v = to - from;
        return (v.sqrMagnitude > 1e-6f) ? v.normalized : Vector3.right;
    }

    // ================== Straight (비유도) ==================
    void spawnAction_Straight(SkillProjectile p, object[] args)
    {
        // 스폰 순간의 "타깃 스냅샷" 기준으로 방향 및 최대 이동거리 기록
        Vector3 from = p.transform.position;
        Vector3 to = p.TargetPosStatic;

        Vector3 dir = DirFromTo(from, to);
        float maxDist = Mathf.Max(0.05f, Vector3.Distance(from, to)); // 최소값 안전

        p.State["dir"] = dir;
        p.State["travel"] = 0f;         // 누적 이동 거리
        p.State["maxDist"] = maxDist;    // 초과 시 파괴
    }

    void tickAction_Straight(SkillProjectile p, object[] args)
    {
        Vector3 dir = (Vector3)p.State["dir"];
        float travel = (float)p.State["travel"];
        float maxDist = (float)p.State["maxDist"];

        float step = p.Data.projectileSpeed * Time.deltaTime;
        travel += step;

        // 이동
        Vector3 next = p.transform.position + dir * step;
        p.ApplyMove(next, dir);

        // 목표선 도달/초과 시 즉시 소멸(화살 잔존 방지)
        if (travel >= maxDist + 0.01f)
        {
            p.DestroySelf();
            return;
        }

        p.State["travel"] = travel;
    }

    // ================== Parabola (비유도) ==================
    void spawnAction_Parabola(SkillProjectile p, object[] args)
    {
        // args[0] = parabolaHeightBase (manager 튜닝)
        float heightBase = (args != null && args.Length > 0) ? Convert.ToSingle(args[0]) : 3.5f;

        Vector3 from = p.transform.position;
        Vector3 target = p.TargetPosStatic;

        p.State["from"] = from;
        p.State["targetSnap"] = target;
        p.State["t"] = 0f;
        p.State["initDist"] = Vector3.Distance(from, target);
        p.State["heightBase"] = heightBase;
        p.State["ctrl"] = ComputeCtrl(heightBase, from, target);
    }

    void tickAction_Parabola(SkillProjectile p, object[] args)
    {
        float t = (float)p.State["t"];
        float initDist = (float)p.State["initDist"];
        float heightBase = (float)p.State["heightBase"];
        Vector3 from = (Vector3)p.State["from"];
        Vector3 target = (Vector3)p.State["targetSnap"];

        float baseDist = (initDist > 0.01f) ? initDist : Vector3.Distance(from, target);
        t = Mathf.Clamp01(t + (p.Data.projectileSpeed / baseDist) * Time.deltaTime);
        p.State["t"] = t;

        Vector3 ctrl = ComputeCtrl(heightBase, from, target);
        p.State["ctrl"] = ctrl;

        Vector3 a = Vector3.Lerp(from, ctrl, t);
        Vector3 b = Vector3.Lerp(ctrl, target, t);
        Vector3 pos = Vector3.Lerp(a, b, t);

        float t2 = Mathf.Clamp01(t + 0.02f);
        Vector3 a2 = Vector3.Lerp(from, ctrl, t2);
        Vector3 b2 = Vector3.Lerp(ctrl, target, t2);
        Vector3 fut = Vector3.Lerp(a2, b2, t2);
        Vector3 dir = (fut - pos); if (dir.sqrMagnitude < 1e-6f) dir = (target - pos);

        p.ApplyMove(pos, dir.normalized);

        // 끝에 도달하면 즉시 소멸
        if (Mathf.Approximately(t, 1f))
            p.DestroySelf();
    }

    Vector3 ComputeCtrl(float heightBase, Vector3 from, Vector3 to)
    {
        Vector3 mid = (from + to) * 0.5f;
        float dist = Vector3.Distance(from, to);
        float h = heightBase + 0.35f * dist;
        return new Vector3(mid.x, mid.y + h, mid.z);
    }

    // ================== Rain → Down N ==================
    // args: [0]=count, [1]=spawnHeight, [2]=width, [3]=headYOffset, [4]=fallMul
    void spawnAction_Rain(SkillProjectile p, object[] args)
    {
        int count = (int)args[0];
        float spawnH = Convert.ToSingle(args[1]);
        float width = Convert.ToSingle(args[2]);
        float headYOff = Convert.ToSingle(args[3]);
        float fallMul = Convert.ToSingle(args[4]);

        Vector3 centerBase = p.TargetPosStatic + Vector3.up * headYOff;

        for (int i = 0; i < count; i++)
        {
            float u = (i + 0.5f) / count;                   // 0..1
            float xOffset = Mathf.Lerp(-width * 0.5f, width * 0.5f, u);

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, p.transform.position, Quaternion.identity)
                            .GetComponent<SkillProjectile>();

            // Down용 파라미터 전달: centerX, centerY, spawnHeight, xOffset, fallMul
            child.SetProjectile(
                p.AttackerType, p.Data,
                spawnAction_Down, tickAction_Down,
                centerBase.x, centerBase.y, spawnH, xOffset, fallMul
            );
        }

        // 부모 Rain은 즉시 종료
        p.DestroySelf();
    }

    // ================== Down (단순 낙하) ==================
    // args: [0]=centerX, [1]=centerY, [2]=spawnHeight, [3]=xOffset, [4]=fallMul
    void spawnAction_Down(SkillProjectile p, object[] args)
    {
        float cx = Convert.ToSingle(args[0]);
        float cy = Convert.ToSingle(args[1]);
        float spawnH = Convert.ToSingle(args[2]);
        float xOffset = Convert.ToSingle(args[3]);

        // 스폰 위치: 머리 위 spawnH
        Vector3 spawnPos = new Vector3(cx + xOffset, cy + spawnH, p.transform.position.z);
        p.transform.position = spawnPos;

        // 멈출 기준 Y 저장(머리 높이), X는 고정
        p.State["stopY"] = cy;
        p.State["x"] = cx + xOffset;
    }

    void tickAction_Down(SkillProjectile p, object[] args)
    {
        float fallMul = (args != null && args.Length >= 5) ? Convert.ToSingle(args[4]) : 1f;
        float stopY = (float)p.State["stopY"];
        float x = (float)p.State["x"];

        Vector3 cur = p.transform.position;
        float nextY = cur.y - (p.Data.projectileSpeed * fallMul) * Time.deltaTime;

        Vector3 next = new Vector3(x, nextY, cur.z);
        p.ApplyMove(next, Vector3.down);

        if (p.transform.position.y <= stopY - 0.2f)
            p.DestroySelf();
    }

    // ================== No-Op (Rain 부모 등) ==================
    void tickAction_NoOp(SkillProjectile p, object[] args)
    {
        // 아무 것도 안 함 (부모는 이미 DestroySelf 호출)
    }
}
