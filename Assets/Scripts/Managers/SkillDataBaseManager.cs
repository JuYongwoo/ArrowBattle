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
    // 기존
    Straight,
    Parabola,
    Rain,     // Down N개 스폰하고 부모는 즉시 종료
    Down,     // 머리 위 고정 스폰, 아래로 낙하만
    StraightThreeMultipleShot, // 3연속 직사

    // 추가 연출 (스폰형/컨트롤러형/무빙형)
    FanBurst,             // 시작 지점에서 부채꼴 N발 확산(직선)
    RingBurstOut,         // 시작 지점에서 원형 N발 바깥으로
    RingBurstInToTarget,  // 타깃 머리 주변 원형 N발 안쪽으로
    LineVerticalForward,  // 시작 지점에서 상하 라인 N개 → 직선 돌진
    LineHorizontalForward,// 시작 지점에서 좌우 라인 N개 → 직선 돌진

    RandomDownRainDuration, // 5초 등, 랜덤 낙하를 지속적으로 생성(컨트롤러)
    SweepDownRainDuration,  // 좌→우로 스윕하며 낙하 지속 생성(컨트롤러)

    SineStraight,         // 직선 진행 + 사인 횡진동
    ZigZagStraight,       // 직선 진행 + 지그재그(삼각파)
    ScatterSplit,          // 잠시 비행 후 다수 파편으로 분열

    SpinningStraight  // 직선 이동 + 스프라이트 지속 회전

}

public class SkillDataBaseManager
{
    public Dictionary<Skill, SkillDataSO> attackSkillData;

    // ====== 매니저 튜닝(전부 여기 둠) ======
    // 공통
    public float headYOffset = 1.0f;

    // StraightThreeMultipleShot
    public int tripleShots = 3;
    public float tripleInterval = 0.12f;

    // Parabola
    public float parabolaHeightBase = 3.5f;

    // Rain/Down (단발)
    public int rainDownCount = 6;
    public float downSpawnHeight = 6f;
    public float downWidth = 4f;
    public float downFallSpeedMul = 1f;

    // RandomDownRainDuration
    public float randomRainDuration = 5.0f; // 초
    public float randomRainRatePerSec = 8.0f; // 초당 생성수
    public float randomRainWidth = 6.0f; // 가로 폭
    public float randomRainSpawnH = 6.0f; // 머리 위 높이
    public float randomRainFallMul = 1.0f;

    // SweepDownRainDuration
    public float sweepRainDuration = 5.0f;
    public float sweepRainRatePerSec = 8.0f;
    public float sweepRainWidth = 6.0f; // 스윕 범위(좌우)
    public float sweepRainSpeed = 3.0f; // 스윕 속도(유닛/초)
    public float sweepRainSpawnH = 6.0f;
    public float sweepRainFallMul = 1.0f;

    // FanBurst
    public int fanCount = 7;
    public float fanAngle = 60f; // 총각도

    // RingBurst
    public int ringCount = 12;
    public float ringRadius = 2.0f;

    // LineForward
    public int lineCount = 5;
    public float lineSpacing = 0.6f;

    // Sine/ZigZag
    public float sineAmplitude = 0.6f;
    public float sineFrequencyHz = 2.5f;
    public float zigzagAmplitude = 0.6f;
    public float zigzagFrequencyHz = 3.0f;

    // ScatterSplit
    public float scatterDelaySec = 0.45f;
    public int scatterCount = 12;
    public float scatterFanAngle = 120f;

    //
    public float spinningStraightSpinSpeedDeg = 720f; // 초당 회전 각도(도)


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

        switch (type)
        {
            // 단발 컨트롤/무빙
            case SkillProjectileMovingType.Straight:
                proj.SetProjectile(casterType, so, spawnAction_Straight, tickAction_Straight);
                break;

            case SkillProjectileMovingType.Parabola:
                proj.SetProjectile(casterType, so, spawnAction_Parabola, tickAction_Parabola, parabolaHeightBase);
                break;

            case SkillProjectileMovingType.StraightThreeMultipleShot:
                proj.SetProjectile(casterType, so, spawnAction_Straight, tickAction_Straight);
                if (_runner != null) _runner.StartCoroutine(CoTripleBurst(casterType, startPosition, so));
                break;

            // Down/Rain
            case SkillProjectileMovingType.Rain:
                proj.SetProjectile(
                    casterType, so,
                    spawnAction_Rain, tickAction_NoOp,
                    rainDownCount, downSpawnHeight, downWidth, headYOffset, downFallSpeedMul
                );
                break;

            case SkillProjectileMovingType.Down:
                proj.SetProjectile(
                    casterType, so,
                    spawnAction_Down, tickAction_Down,
                    1, downSpawnHeight, 0f, headYOffset, downFallSpeedMul
                );
                break;

            // 퍼져나가는 스폰형
            case SkillProjectileMovingType.FanBurst:
                proj.SetProjectile(casterType, so, spawnAction_FanBurst, tickAction_NoOp);
                break;

            case SkillProjectileMovingType.RingBurstOut:
                proj.SetProjectile(casterType, so, spawnAction_RingBurstOut, tickAction_NoOp);
                break;

            case SkillProjectileMovingType.RingBurstInToTarget:
                proj.SetProjectile(casterType, so, spawnAction_RingBurstInToTarget, tickAction_NoOp);
                break;

            case SkillProjectileMovingType.LineVerticalForward:
                proj.SetProjectile(casterType, so, spawnAction_LineVerticalForward, tickAction_NoOp);
                break;

            case SkillProjectileMovingType.LineHorizontalForward:
                proj.SetProjectile(casterType, so, spawnAction_LineHorizontalForward, tickAction_NoOp);
                break;

            // 지속형 컨트롤러
            case SkillProjectileMovingType.RandomDownRainDuration:
                proj.SetProjectile(
                    casterType, so, spawnAction_RandomDownRain, tickAction_RandomDownRain
                );
                break;

            case SkillProjectileMovingType.SweepDownRainDuration:
                proj.SetProjectile(
                    casterType, so, spawnAction_SweepDownRain, tickAction_SweepDownRain
                );
                break;

            // 무빙형
            case SkillProjectileMovingType.SineStraight:
                proj.SetProjectile(casterType, so, spawnAction_SineStraight, tickAction_SineStraight);
                break;

            case SkillProjectileMovingType.ZigZagStraight:
                proj.SetProjectile(casterType, so, spawnAction_ZigZagStraight, tickAction_ZigZagStraight);
                break;

            // 분열형
            case SkillProjectileMovingType.ScatterSplit:
                proj.SetProjectile(casterType, so, spawnAction_ScatterSplit, tickAction_ScatterSplit);
                break;

            case SkillProjectileMovingType.SpinningStraight:
                {
                    proj.SetProjectile(casterType, so, spawnAction_StraightSpin, tickAction_StraightSpin, spinningStraightSpinSpeedDeg);
                    break;
                }

            default:
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

    // ------------------------------------------------------------------
    // 공통 유틸
    // ------------------------------------------------------------------
    static Vector3 DirFromTo(Vector3 from, Vector3 to)
    {
        var v = to - from;
        return (v.sqrMagnitude > 1e-6f) ? v.normalized : Vector3.right;
    }

    static float TriWave(float t) // -1..1
    {
        float v = Mathf.PingPong(t, 1f); // 0..1..0
        return (v * 2f - 1f);            // -1..1..-1
    }

    // ------------------------------------------------------------------
    // Straight (비유도) : args[0]로 Vector3 dir을 넘겨주면 그 방향으로 고정
    // ------------------------------------------------------------------
    void spawnAction_Straight(SkillProjectile p, object[] args)
    {
        Vector3 dir;
        if (args != null && args.Length > 0 && args[0] is Vector3 argDir)
        {
            dir = argDir.normalized;
        }
        else
        {
            dir = DirFromTo(p.transform.position, p.TargetPosStatic);
        }

        float maxDist = Mathf.Max(0.05f, Vector3.Distance(p.transform.position, p.TargetPosStatic));
        p.State["dir"] = dir;
        p.State["travel"] = 0f;
        p.State["maxDist"] = maxDist;
    }

    void tickAction_Straight(SkillProjectile p, object[] args)
    {
        Vector3 dir = (Vector3)p.State["dir"];
        float travel = (float)p.State["travel"];
        float maxDist = (float)p.State["maxDist"];

        float step = p.Data.projectileSpeed * Time.deltaTime;
        travel += step;

        Vector3 next = p.transform.position + dir * step;
        p.ApplyMove(next, dir);

        if (travel >= maxDist + 0.01f)
        {
            p.DestroySelf();
            return;
        }

        p.State["travel"] = travel;
    }

    // ------------------------------------------------------------------
    // Parabola (비유도)
    // ------------------------------------------------------------------
    void spawnAction_Parabola(SkillProjectile p, object[] args)
    {
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
        Vector3 a = Vector3.Lerp(from, ctrl, t);
        Vector3 b = Vector3.Lerp(ctrl, target, t);
        Vector3 pos = Vector3.Lerp(a, b, t);

        float t2 = Mathf.Clamp01(t + 0.02f);
        Vector3 a2 = Vector3.Lerp(from, ctrl, t2);
        Vector3 b2 = Vector3.Lerp(ctrl, target, t2);
        Vector3 fut = Vector3.Lerp(a2, b2, t2);
        Vector3 dir = (fut - pos); if (dir.sqrMagnitude < 1e-6f) dir = (target - pos);

        p.ApplyMove(pos, dir.normalized);

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

    // ------------------------------------------------------------------
    // Rain → Down N (단발)
    // args: [0]=count, [1]=spawnHeight, [2]=width, [3]=headYOffset, [4]=fallMul
    // ------------------------------------------------------------------
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
            float u = (i + 0.5f) / count; // 0..1
            float xOffset = Mathf.Lerp(-width * 0.5f, width * 0.5f, u);

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, p.transform.position, Quaternion.identity)
                            .GetComponent<SkillProjectile>();

            child.SetProjectile(
                p.AttackerType, p.Data,
                spawnAction_Down, tickAction_Down,
                centerBase.x, centerBase.y, spawnH, xOffset, fallMul
            );
        }

        p.DestroySelf();
    }

    // ------------------------------------------------------------------
    // Down (단순 낙하)
    // args: [0]=centerX, [1]=centerY, [2]=spawnHeight, [3]=xOffset, [4]=fallMul
    // ------------------------------------------------------------------
    void spawnAction_Down(SkillProjectile p, object[] args)
    {
        float cx = Convert.ToSingle(args[0]);
        float cy = Convert.ToSingle(args[1]);
        float spawnH = Convert.ToSingle(args[2]);
        float xOffset = Convert.ToSingle(args[3]);

        Vector3 spawnPos = new Vector3(cx + xOffset, cy + spawnH, p.transform.position.z);
        p.transform.position = spawnPos;

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

    // ------------------------------------------------------------------
    // FanBurst (부채꼴 확산) : 시작지점에서 N발 즉시 스폰 후 부모 종료
    // ------------------------------------------------------------------
    void spawnAction_FanBurst(SkillProjectile p, object[] args)
    {
        Vector3 baseDir = DirFromTo(p.transform.position, p.TargetPosStatic);
        int N = Mathf.Max(1, fanCount);
        float total = fanAngle;
        float step = (N <= 1) ? 0f : total / (N - 1);
        float start = -total * 0.5f;

        for (int i = 0; i < N; i++)
        {
            float ang = start + step * i;
            Quaternion rot = Quaternion.AngleAxis(ang, Vector3.forward);
            Vector3 dir = rot * baseDir;

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, p.transform.position, Quaternion.identity)
                            .GetComponent<SkillProjectile>();
            child.SetProjectile(p.AttackerType, p.Data, spawnAction_Straight, tickAction_Straight, dir);
        }

        p.DestroySelf();
    }

    // ------------------------------------------------------------------
    // RingBurstOut (시작 위치 원형 바깥으로)
    // ------------------------------------------------------------------
    void spawnAction_RingBurstOut(SkillProjectile p, object[] args)
    {
        int N = Mathf.Max(1, ringCount);
        Vector3 center = p.transform.position;

        for (int i = 0; i < N; i++)
        {
            float theta = (i / (float)N) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(theta) * ringRadius, Mathf.Sin(theta) * ringRadius, 0f);
            Vector3 dir = (pos - center).normalized;

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, pos, Quaternion.identity)
                            .GetComponent<SkillProjectile>();
            child.SetProjectile(p.AttackerType, p.Data, spawnAction_Straight, tickAction_Straight, dir);
        }

        p.DestroySelf();
    }

    // ------------------------------------------------------------------
    // RingBurstInToTarget (타깃 머리 주변 원형에서 안쪽으로)
    // ------------------------------------------------------------------
    void spawnAction_RingBurstInToTarget(SkillProjectile p, object[] args)
    {
        int N = Mathf.Max(1, ringCount);
        Vector3 center = p.TargetPosStatic + Vector3.up * headYOffset;

        for (int i = 0; i < N; i++)
        {
            float theta = (i / (float)N) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(theta) * ringRadius, Mathf.Sin(theta) * ringRadius, 0f);
            Vector3 dir = (center - pos).normalized;

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, pos, Quaternion.identity)
                            .GetComponent<SkillProjectile>();
            child.SetProjectile(p.AttackerType, p.Data, spawnAction_Straight, tickAction_Straight, dir);
        }

        p.DestroySelf();
    }

    // ------------------------------------------------------------------
    // LineVerticalForward / LineHorizontalForward
    // ------------------------------------------------------------------
    void spawnAction_LineVerticalForward(SkillProjectile p, object[] args)
    {
        int N = Mathf.Max(1, lineCount);
        int mid = (N - 1) / 2;
        Vector3 forward = DirFromTo(p.transform.position, p.TargetPosStatic);

        for (int i = 0; i < N; i++)
        {
            int offIdx = i - mid;
            Vector3 pos = p.transform.position + Vector3.up * (offIdx * lineSpacing);

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, pos, Quaternion.identity)
                            .GetComponent<SkillProjectile>();
            child.SetProjectile(p.AttackerType, p.Data, spawnAction_Straight, tickAction_Straight, forward);
        }

        p.DestroySelf();
    }

    void spawnAction_LineHorizontalForward(SkillProjectile p, object[] args)
    {
        int N = Mathf.Max(1, lineCount);
        int mid = (N - 1) / 2;
        Vector3 forward = DirFromTo(p.transform.position, p.TargetPosStatic);

        for (int i = 0; i < N; i++)
        {
            int offIdx = i - mid;
            Vector3 pos = p.transform.position + Vector3.right * (offIdx * lineSpacing);

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, pos, Quaternion.identity)
                            .GetComponent<SkillProjectile>();
            child.SetProjectile(p.AttackerType, p.Data, spawnAction_Straight, tickAction_Straight, forward);
        }

        p.DestroySelf();
    }

    // ------------------------------------------------------------------
    // RandomDownRainDuration (컨트롤러: duration 동안 interval마다 랜덤 낙하 생성)
    // State: tLeft, spawnInterval, acc, centerX, centerY, width
    // ------------------------------------------------------------------
    void spawnAction_RandomDownRain(SkillProjectile p, object[] args)
    {
        float duration = Mathf.Max(0.01f, randomRainDuration);
        float rate = Mathf.Max(0.01f, randomRainRatePerSec);
        float interval = 1f / rate;

        Vector3 headCenter = p.TargetPosStatic + Vector3.up * headYOffset;

        p.State["tLeft"] = duration;
        p.State["spawnInterval"] = interval;
        p.State["acc"] = 0f;
        p.State["cX"] = headCenter.x;
        p.State["cY"] = headCenter.y;
        p.State["width"] = randomRainWidth;
        p.State["spawnH"] = randomRainSpawnH;
        p.State["fallMul"] = randomRainFallMul;
    }

    void tickAction_RandomDownRain(SkillProjectile p, object[] args)
    {
        float tLeft = (float)p.State["tLeft"];
        float intv = (float)p.State["spawnInterval"];
        float acc = (float)p.State["acc"];
        float cX = (float)p.State["cX"];
        float cY = (float)p.State["cY"];
        float width = (float)p.State["width"];
        float spawnH = (float)p.State["spawnH"];
        float fallMul = (float)p.State["fallMul"];

        float dt = Time.deltaTime;
        tLeft -= dt;
        acc += dt;

        while (acc >= intv && tLeft > 0f)
        {
            acc -= intv;

            float xOff = UnityEngine.Random.Range(-width * 0.5f, width * 0.5f);

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, p.transform.position, Quaternion.identity)
                            .GetComponent<SkillProjectile>();

            child.SetProjectile(
                p.AttackerType, p.Data,
                spawnAction_Down, tickAction_Down,
                cX, cY, spawnH, xOff, fallMul
            );
        }

        p.State["tLeft"] = tLeft;
        p.State["acc"] = acc;

        if (tLeft <= 0f)
            p.DestroySelf();
    }

    // ------------------------------------------------------------------
    // SweepDownRainDuration (컨트롤러: 중앙이 좌↔우 스윕되며 낙하 생성)
    // State: tLeft, spawnInterval, acc, baseX, cY, width, sweepWidth, speed
    // ------------------------------------------------------------------
    void spawnAction_SweepDownRain(SkillProjectile p, object[] args)
    {
        float duration = Mathf.Max(0.01f, sweepRainDuration);
        float rate = Mathf.Max(0.01f, sweepRainRatePerSec);
        float interval = 1f / rate;

        Vector3 headCenter = p.TargetPosStatic + Vector3.up * headYOffset;

        p.State["tLeft"] = duration;
        p.State["spawnInterval"] = interval;
        p.State["acc"] = 0f;
        p.State["baseX"] = headCenter.x - sweepRainWidth * 0.5f;
        p.State["cY"] = headCenter.y;
        p.State["width"] = sweepRainWidth;   // 스윕 범위
        p.State["spawnH"] = sweepRainSpawnH;
        p.State["fallMul"] = sweepRainFallMul;
        p.State["speed"] = sweepRainSpeed;
    }

    void tickAction_SweepDownRain(SkillProjectile p, object[] args)
    {
        float tLeft = (float)p.State["tLeft"];
        float intv = (float)p.State["spawnInterval"];
        float acc = (float)p.State["acc"];
        float baseX = (float)p.State["baseX"];
        float cY = (float)p.State["cY"];
        float width = (float)p.State["width"];
        float spawnH = (float)p.State["spawnH"];
        float fallMul = (float)p.State["fallMul"];
        float speed = (float)p.State["speed"];

        float dt = Time.deltaTime;
        tLeft -= dt;
        acc += dt;

        // 현재 스윕 중심 (좌→우→좌 PingPong)
        float traveled = (p.Elapsed * speed) % (width * 2f);
        float offset = (traveled <= width) ? traveled : (2f * width - traveled);
        float centerX = baseX + offset;

        while (acc >= intv && tLeft > 0f)
        {
            acc -= intv;

            // 중심 부근 ±(width*0.25)에서 랜덤
            float jitter = UnityEngine.Random.Range(-width * 0.25f, width * 0.25f);
            float x = centerX + jitter;

            var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, p.transform.position, Quaternion.identity)
                            .GetComponent<SkillProjectile>();

            child.SetProjectile(
                p.AttackerType, p.Data,
                spawnAction_Down, tickAction_Down,
                x, cY, spawnH, 0f, fallMul
            );
        }

        p.State["tLeft"] = tLeft;
        p.State["acc"] = acc;

        if (tLeft <= 0f)
            p.DestroySelf();
    }

    // ------------------------------------------------------------------
    // SineStraight (무빙형): 직선 + 사인 횡진동
    // State: origin, forward, right, dist, maxDist
    // ------------------------------------------------------------------
    void spawnAction_SineStraight(SkillProjectile p, object[] args)
    {
        Vector3 forward = DirFromTo(p.transform.position, p.TargetPosStatic);
        Vector3 right = new Vector3(-forward.y, forward.x, 0f).normalized;

        p.State["origin"] = p.transform.position;
        p.State["forward"] = forward;
        p.State["right"] = right;
        p.State["dist"] = 0f;
        p.State["maxDist"] = Vector3.Distance(p.transform.position, p.TargetPosStatic);
    }

    void spawnAction_StraightSpin(SkillProjectile p, object[] args)
    {
        // 직선과 동일하게 방향/종료거리 셋업
        Vector3 dir = DirFromTo(p.transform.position, p.TargetPosStatic);
        float maxDist = Mathf.Max(0.05f, Vector3.Distance(p.transform.position, p.TargetPosStatic));

        p.State["dir"] = dir;
        p.State["travel"] = 0f;
        p.State["maxDist"] = maxDist;

        float spin = (args != null && args.Length > 0)
            ? Convert.ToSingle(args[0])
            : spinningStraightSpinSpeedDeg;
        p.State["spinSpeed"] = spin; // 도/초
    }

    void tickAction_StraightSpin(SkillProjectile p, object[] args)
    {
        Vector3 dir = (Vector3)p.State["dir"];
        float travel = (float)p.State["travel"];
        float maxDist = (float)p.State["maxDist"];
        float spin = (float)p.State["spinSpeed"];

        float step = p.Data.projectileSpeed * Time.deltaTime;
        travel += step;

        Vector3 next = p.transform.position + dir * step;

        // 기본 이동 + 방향 회전(ApplyMove)
        p.ApplyMove(next, dir);

        // 추가 스핀(이미 ApplyMove가 기본 각도를 세팅했으므로, 그 위에 누적 회전)
        p.transform.rotation = p.transform.rotation * Quaternion.Euler(0f, 0f, spin * Time.deltaTime);

        if (travel >= maxDist + 0.01f)
        {
            p.DestroySelf();
            return;
        }

        p.State["travel"] = travel;
    }

    void tickAction_SineStraight(SkillProjectile p, object[] args)
    {
        Vector3 origin = (Vector3)p.State["origin"];
        Vector3 forward = (Vector3)p.State["forward"];
        Vector3 right = (Vector3)p.State["right"];
        float dist = (float)p.State["dist"];
        float maxDist = (float)p.State["maxDist"];

        float dt = Time.deltaTime;
        dist += p.Data.projectileSpeed * dt;

        float phase = p.Elapsed * sineFrequencyHz * Mathf.PI * 2f;
        Vector3 pos = origin + forward * dist + right * (Mathf.Sin(phase) * sineAmplitude);

        // 진행 방향 근사
        float d2 = dist + p.Data.projectileSpeed * 0.02f;
        float ph2 = phase + sineFrequencyHz * Mathf.PI * 2f * 0.02f;
        Vector3 p2 = origin + forward * d2 + right * (Mathf.Sin(ph2) * sineAmplitude);
        Vector3 dir = (p2 - pos).normalized;

        p.ApplyMove(pos, dir);

        if (dist >= maxDist + 0.01f)
            p.DestroySelf();

        p.State["dist"] = dist;
    }

    // ------------------------------------------------------------------
    // ZigZagStraight (무빙형): 직선 + 삼각파 횡진동
    // ------------------------------------------------------------------
    void spawnAction_ZigZagStraight(SkillProjectile p, object[] args)
    {
        Vector3 forward = DirFromTo(p.transform.position, p.TargetPosStatic);
        Vector3 right = new Vector3(-forward.y, forward.x, 0f).normalized;

        p.State["origin"] = p.transform.position;
        p.State["forward"] = forward;
        p.State["right"] = right;
        p.State["dist"] = 0f;
        p.State["maxDist"] = Vector3.Distance(p.transform.position, p.TargetPosStatic);
    }

    void tickAction_ZigZagStraight(SkillProjectile p, object[] args)
    {
        Vector3 origin = (Vector3)p.State["origin"];
        Vector3 forward = (Vector3)p.State["forward"];
        Vector3 right = (Vector3)p.State["right"];
        float dist = (float)p.State["dist"];
        float maxDist = (float)p.State["maxDist"];

        float dt = Time.deltaTime;
        dist += p.Data.projectileSpeed * dt;

        float wave = TriWave(p.Elapsed * zigzagFrequencyHz);
        Vector3 pos = origin + forward * dist + right * (wave * zigzagAmplitude);

        float d2 = dist + p.Data.projectileSpeed * 0.02f;
        float w2 = TriWave((p.Elapsed + 0.02f) * zigzagFrequencyHz);
        Vector3 p2 = origin + forward * d2 + right * (w2 * zigzagAmplitude);
        Vector3 dir = (p2 - pos).normalized;

        p.ApplyMove(pos, dir);

        if (dist >= maxDist + 0.01f)
            p.DestroySelf();

        p.State["dist"] = dist;
    }

    // ------------------------------------------------------------------
    // ScatterSplit (분열형): delay 후 현재 위치에서 N발 부채꼴 분열
    // State: dir, timer
    // ------------------------------------------------------------------
    void spawnAction_ScatterSplit(SkillProjectile p, object[] args)
    {
        Vector3 dir = DirFromTo(p.transform.position, p.TargetPosStatic);
        p.State["dir"] = dir;
        p.State["timer"] = 0f;
    }

    void tickAction_ScatterSplit(SkillProjectile p, object[] args)
    {
        Vector3 dir = (Vector3)p.State["dir"];
        float timer = (float)p.State["timer"];

        float dt = Time.deltaTime;
        timer += dt;

        // 이동
        Vector3 next = p.transform.position + dir * p.Data.projectileSpeed * dt;
        p.ApplyMove(next, dir);

        if (timer >= scatterDelaySec)
        {
            // 분열
            int N = Mathf.Max(1, scatterCount);
            float total = scatterFanAngle;
            float step = (N <= 1) ? 0f : total / (N - 1);
            float start = -total * 0.5f;

            for (int i = 0; i < N; i++)
            {
                float ang = start + step * i;
                Quaternion rot = Quaternion.AngleAxis(ang, Vector3.forward);
                Vector3 dirOut = rot * dir;

                var child = UnityEngine.Object.Instantiate(p.Data.skillProjectile, p.transform.position, Quaternion.identity)
                                .GetComponent<SkillProjectile>();
                child.SetProjectile(p.AttackerType, p.Data, spawnAction_Straight, tickAction_Straight, dirOut);
            }

            p.DestroySelf();
            return;
        }

        p.State["timer"] = timer;
    }

    // ------------------------------------------------------------------
    // No-Op (컨트롤러 부모 등)
    // ------------------------------------------------------------------
    void tickAction_NoOp(SkillProjectile p, object[] args)
    {
        // 아무 것도 안 함 (부모는 필요 시 자체 DestroySelf 호출)
    }
}
