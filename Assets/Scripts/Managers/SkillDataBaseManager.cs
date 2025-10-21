using System;
using System.Collections;
using UnityEngine;


public enum SkillProjectileMovingType
{
    // 기본
    Straight,
    Parabola,
    Rain,                      // Down 여러개 스폰
    Down,                      // 머리 위 고정 스폰, 아래로 낙하만
    StraightThreeMultipleShot, // 3연속
    StraightFiveMultipleShot,  // 5연속
    FanBurst,             // 시작 지점에서 부채꼴 N발 확산(직선)
    RingBurstOut,         // 시작 지점에서 원형 N발 바깥으로
    RingBurstInToTarget,  // 타깃 머리 주변 원형 N발 안쪽으로
    LineVerticalForward,  // 시작 지점에서 상하 라인 N개 → 직선 돌진
    LineHorizontalForward,// 시작 지점에서 좌우 라인 N개 → 직선 돌진

    RandomDownRainDuration, // 랜덤 낙하
    SweepDownRainDuration,  // 좌↔우 낙하 

    SineStraight,         
    ZigZagStraight,       
    ScatterSplit,         

    StraightSpin          // 직선 이동 + 스프라이트 지속 회전
}

public class SkillDataBaseManager
{


    ////////////////////// JYW TODO: SO로 옮기기(Editor사용하여 선택한 type 값에 따라 수치 칸 보이도록)
    private float headYOffset = 1.0f;

    // StraightThreeMultipleShot
    private int tripleShots = 3;
    private float tripleInterval = 0.12f;

    // StraightFiveMultipleShot
    private int fiveShots = 5;
    private float fiveInterval = 0.10f;

    // Parabola
    private float parabolaHeightBase = 3.5f;

    // Rain/Down
    private int rainDownCount = 6;
    private float downSpawnHeight = 5f;
    private float downWidth = 4f;
    private float downFallSpeedMul = 1f;

    // RandomDownRainDuration
    private float randomRainDuration = 5.0f;
    private float randomRainRatePerSec = 8.0f;
    private float randomRainWidth = 6.0f;
    private float randomRainSpawnH = 6.0f;
    private float randomRainFallMul = 1.0f;

    // SweepDownRainDuration
    private float sweepRainDuration = 5.0f;
    private float sweepRainRatePerSec = 8.0f;
    private float sweepRainWidth = 6.0f;
    private float sweepRainSpeed = 3.0f;
    private float sweepRainSpawnH = 6.0f;
    private float sweepRainFallMul = 1.0f;

    // FanBurst
    private int fanCount = 7;
    private float fanAngle = 60f;

    // RingBurst
    private int ringCount = 12;
    private float ringRadius = 7.0f;

    // LineForward
    private int lineCount = 5;
    private float lineSpacing = 0.6f;

    // Sine/ZigZag
    private float sineAmplitude = 0.6f;
    private float sineFrequencyHz = 2.5f;
    private float zigzagAmplitude = 0.6f;
    private float zigzagFrequencyHz = 3.0f;

    // ScatterSplit
    private float scatterDelaySec = 0.45f;
    private int scatterCount = 12;
    private float scatterFanAngle = 120f;

    // StraightSpin
    private float spinningStraightSpinSpeedDeg = -1440f; // 도/초

    //////////////////////

    // 코루틴 러너 (옵션)
    private MonoBehaviour _runner;


    public MonoBehaviour GetRunner()
    {
        if (_runner != null) return _runner;
        return SkillCoroutineHub.Instance;
    }

    

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

    private Transform FindTargetForCaster(CharacterTypeEnumByTag casterType)
    {
        var values = Enum.GetValues(typeof(CharacterTypeEnumByTag));
        string enemyTag = Enum.GetName(
            typeof(CharacterTypeEnumByTag),
            ((int)casterType + 1) % values.Length
        );
        var go = GameObject.FindGameObjectWithTag(enemyTag);
        return go != null ? go.transform : null;
    }

    // 매니저 전용 생성기 (유일한 Instantiate 경로)
    private SkillProjectile SpawnProjectile(
        CharacterTypeEnumByTag casterType,
        Vector3 pos,
        SkillData so,
        Action<SkillProjectile, object[]> init,
        Action<SkillProjectile, object[]> tick,
        params object[] args
    )
    {

        var p = ManagerObject.instance.poolManager.Spawn(so.SkillProjectile, pos, Quaternion.identity).GetComponent<SkillProjectile>();

        p.SetProjectile(casterType, so, init, tick, args);
        return p;
    }

    public void Shoot(CharacterTypeEnumByTag casterType, Vector3 startPosition, Skill skill)
    {
        var so = ManagerObject.instance.resourceManager.SkillDatas.Result.GetSkillDataById(skill);
        var targetTr = FindTargetForCaster(casterType);
        var targetPos = (targetTr != null) ? targetTr.position : (startPosition + Vector3.right * 8f);
        var forward = DirFromTo(startPosition, targetPos);

        ManagerObject.instance.eventManager.OnPlayAudioClip(ManagerObject.instance.resourceManager.SkillDatas.Result.GetSkillDataById(skill).SkillSound, 0.2f, false);

        switch (so.SkillProjectileMovingType)
        {
            case SkillProjectileMovingType.Straight:
                Spawn_Straight(casterType, so, startPosition, forward);
                break;

            case SkillProjectileMovingType.Parabola:
                Spawn_Parabola(casterType, so, startPosition, parabolaHeightBase);
                break;

            case SkillProjectileMovingType.StraightThreeMultipleShot:
                Spawn_StraightBurst(casterType, so, startPosition, forward, tripleShots, tripleInterval);
                break;

            case SkillProjectileMovingType.StraightFiveMultipleShot:
                Spawn_StraightBurst(casterType, so, startPosition, forward, fiveShots, fiveInterval);
                break;

            case SkillProjectileMovingType.Rain:
                Spawn_Rain(casterType, so, startPosition, targetPos);
                break;

            case SkillProjectileMovingType.Down:
                Spawn_DownSingle(casterType, so, targetPos);
                break;

            case SkillProjectileMovingType.FanBurst:
                Spawn_FanBurst(casterType, so, startPosition, forward);
                break;

            case SkillProjectileMovingType.RingBurstOut:
                Spawn_RingBurstOut(casterType, so, startPosition);
                break;

            case SkillProjectileMovingType.RingBurstInToTarget:
                Spawn_RingBurstInToTarget(casterType, so, targetPos);
                break;

            case SkillProjectileMovingType.LineVerticalForward:
                Spawn_LineVerticalForward(casterType, so, startPosition, forward);
                break;

            case SkillProjectileMovingType.LineHorizontalForward:
                Spawn_LineHorizontalForward(casterType, so, startPosition, forward);
                break;

            case SkillProjectileMovingType.RandomDownRainDuration:
                GetRunner().StartCoroutine(Co_RandomDownRainDuration(casterType, so, targetPos));
                break;

            case SkillProjectileMovingType.SweepDownRainDuration:
                GetRunner().StartCoroutine(Co_SweepDownRainDuration(casterType, so, targetPos));
                break;

            case SkillProjectileMovingType.SineStraight:
                Spawn_SineStraight(casterType, so, startPosition, forward);
                break;

            case SkillProjectileMovingType.ZigZagStraight:
                Spawn_ZigZagStraight(casterType, so, startPosition, forward);
                break;

            case SkillProjectileMovingType.ScatterSplit:
                Spawn_ScatterSplit(casterType, so, startPosition, forward);
                break;

            case SkillProjectileMovingType.StraightSpin:
                Spawn_StraightSpin(casterType, so, startPosition, forward, spinningStraightSpinSpeedDeg);
                break;

            default:
                Spawn_Straight(casterType, so, startPosition, forward);
                break;
        }
    }
    void Spawn_Straight(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 dir)
    {
        SpawnProjectile(caster, pos, so, init_Straight, tick_Straight, dir);
    }

    void Spawn_Parabola(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, float heightBase)
    {
        SpawnProjectile(caster, pos, so, init_Parabola, tick_Parabola, heightBase);
    }

    void Spawn_StraightSpin(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 dir, float spinDegPerSec)
    {
        SpawnProjectile(caster, pos, so, init_StraightSpin, tick_StraightSpin, dir, spinDegPerSec);
    }

    void Spawn_StraightBurst(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 dir, int shots, float interval)
    {
        shots = Mathf.Max(1, shots);
        // 첫 발
        Spawn_Straight(caster, so, pos, dir);
        // 나머지
        if (shots > 1)
            GetRunner().StartCoroutine(Co_StraightBurst(caster, so, pos, dir, shots - 1, interval));
    }

    IEnumerator Co_StraightBurst(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 dir, int remain, float interval)
    {
        for (int i = 0; i < remain; i++)
        {
            yield return new WaitForSeconds(interval);
            Spawn_Straight(caster, so, pos, dir);
        }
    }

    void Spawn_Rain(CharacterTypeEnumByTag caster, SkillData so, Vector3 startPos, Vector3 targetPos)
    {
        int count = Mathf.Max(1, rainDownCount);
        float width = downWidth;
        float spawnH = downSpawnHeight;

        Vector3 centerBase = targetPos + Vector3.up * headYOffset;
        for (int i = 0; i < count; i++)
        {
            float u = (i + 0.5f) / count;
            float xOff = Mathf.Lerp(-width * 0.5f, width * 0.5f, u);
            Vector3 spawnPos = new Vector3(centerBase.x + xOff, centerBase.y + spawnH, startPos.z);
            SpawnProjectile(caster, spawnPos, so, init_Down, tick_Down, centerBase.y, centerBase.x + xOff, downFallSpeedMul);
        }
    }

    void Spawn_DownSingle(CharacterTypeEnumByTag caster, SkillData so, Vector3 targetPos)
    {
        Vector3 centerBase = targetPos + Vector3.up * headYOffset;
        Vector3 spawnPos = new Vector3(centerBase.x, centerBase.y + downSpawnHeight, targetPos.z);
        SpawnProjectile(caster, spawnPos, so, init_Down, tick_Down, centerBase.y, centerBase.x, downFallSpeedMul);
    }

    void Spawn_FanBurst(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 forward)
    {
        int N = Mathf.Max(1, fanCount);
        float total = fanAngle;
        float step = (N <= 1) ? 0f : total / (N - 1);
        float start = -total * 0.5f;

        for (int i = 0; i < N; i++)
        {
            float ang = start + step * i;
            Vector3 dir = Quaternion.AngleAxis(ang, Vector3.forward) * forward;
            Spawn_Straight(caster, so, pos, dir);
        }
    }

    void Spawn_RingBurstOut(CharacterTypeEnumByTag caster, SkillData so, Vector3 center)
    {
        int N = Mathf.Max(1, ringCount);
        for (int i = 0; i < N; i++)
        {
            float theta = (i / (float)N) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(theta) * ringRadius, Mathf.Sin(theta) * ringRadius, 0f);
            Vector3 dir = (pos - center).normalized;
            Spawn_Straight(caster, so, pos, dir);
        }
    }

    void Spawn_RingBurstInToTarget(CharacterTypeEnumByTag caster, SkillData so, Vector3 targetPos)
    {
        int N = Mathf.Max(1, ringCount);
        Vector3 center = targetPos + Vector3.up * headYOffset;
        for (int i = 0; i < N; i++)
        {
            float theta = (i / (float)N) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(theta) * ringRadius, Mathf.Sin(theta) * ringRadius, 0f);
            Vector3 dir = (center - pos).normalized;
            Spawn_Straight(caster, so, pos, dir);
        }
    }

    void Spawn_LineVerticalForward(CharacterTypeEnumByTag caster, SkillData so, Vector3 start, Vector3 forward)
    {
        int N = Mathf.Max(1, lineCount);
        int mid = (N - 1) / 2;
        for (int i = 0; i < N; i++)
        {
            int off = i - mid;
            Vector3 pos = start + Vector3.up * (off * lineSpacing);
            Spawn_Straight(caster, so, pos, forward);
        }
    }

    void Spawn_LineHorizontalForward(CharacterTypeEnumByTag caster, SkillData so, Vector3 start, Vector3 forward)
    {
        int N = Mathf.Max(1, lineCount);
        int mid = (N - 1) / 2;
        for (int i = 0; i < N; i++)
        {
            int off = i - mid;
            Vector3 pos = start + Vector3.right * (off * lineSpacing);
            Spawn_Straight(caster, so, pos, forward);
        }
    }

    IEnumerator Co_RandomDownRainDuration(CharacterTypeEnumByTag caster, SkillData so, Vector3 targetPos)
    {
        float tLeft = Mathf.Max(0.01f, randomRainDuration);
        float interval = 1f / Mathf.Max(0.01f, randomRainRatePerSec);

        Vector3 center = targetPos + Vector3.up * headYOffset;

        while (tLeft > 0f)
        {
            float xOff = UnityEngine.Random.Range(-randomRainWidth * 0.5f, randomRainWidth * 0.5f);
            Vector3 spawnPos = new Vector3(center.x + xOff, center.y + randomRainSpawnH, targetPos.z);
            SpawnProjectile(caster, spawnPos, so, init_Down, tick_Down, center.y, center.x + xOff, randomRainFallMul);

            yield return new WaitForSeconds(interval);
            tLeft -= interval;
        }
    }

    IEnumerator Co_SweepDownRainDuration(CharacterTypeEnumByTag caster, SkillData so, Vector3 targetPos)
    {
        float tLeft = Mathf.Max(0.01f, sweepRainDuration);
        float interval = 1f / Mathf.Max(0.01f, sweepRainRatePerSec);

        Vector3 center = targetPos + Vector3.up * headYOffset;
        float baseX = center.x - sweepRainWidth * 0.5f;

        float elapsed = 0f;
        while (tLeft > 0f)
        {
            float traveled = (elapsed * sweepRainSpeed) % (sweepRainWidth * 2f);
            float offset = (traveled <= sweepRainWidth) ? traveled : (2f * sweepRainWidth - traveled);
            float cx = baseX + offset;

            float jitter = UnityEngine.Random.Range(-sweepRainWidth * 0.25f, sweepRainWidth * 0.25f);
            Vector3 spawnPos = new Vector3(cx + jitter, center.y + sweepRainSpawnH, targetPos.z);
            SpawnProjectile(caster, spawnPos, so, init_Down, tick_Down, center.y, cx + jitter, sweepRainFallMul);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
            tLeft -= interval;
        }
    }

    void Spawn_SineStraight(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 forward)
    {
        Vector3 right = new Vector3(-forward.y, forward.x, 0f).normalized;
        float maxDist = 9999f; // 실제 종료는 목표 거리 기반으로 tick에서 처리
        SpawnProjectile(caster, pos, so, init_SineStraight, tick_SineStraight,
            pos, forward, right, maxDist, sineAmplitude, sineFrequencyHz);
    }

    void Spawn_ZigZagStraight(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 forward)
    {
        Vector3 right = new Vector3(-forward.y, forward.x, 0f).normalized;
        float maxDist = 9999f;
        SpawnProjectile(caster, pos, so, init_ZigZagStraight, tick_ZigZagStraight,
            pos, forward, right, maxDist, zigzagAmplitude, zigzagFrequencyHz);
    }

    void Spawn_ScatterSplit(CharacterTypeEnumByTag caster, SkillData so, Vector3 pos, Vector3 dir)
    {
        var parent = SpawnProjectile(caster, pos, so, init_Straight, tick_Straight, dir);
        GetRunner().StartCoroutine(Co_ScatterSplit(caster, so, parent, dir, scatterDelaySec, scatterCount, scatterFanAngle));
    }

    IEnumerator Co_ScatterSplit(CharacterTypeEnumByTag caster, SkillData so, SkillProjectile parent, Vector3 forward, float delay, int count, float fan)
    {
        yield return new WaitForSeconds(delay);

        if (parent == null) yield break; // 이미 사라짐

        Vector3 pos = parent.transform.position;
        // 분열 생성(매니저만 Instantiate)
        count = Mathf.Max(1, count);
        float total = fan;
        float step = (count <= 1) ? 0f : total / (count - 1);
        float start = -total * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float ang = start + step * i;
            Vector3 dir = Quaternion.AngleAxis(ang, Vector3.forward) * forward;
            Spawn_Straight(caster, so, pos, dir);
        }

        if (parent != null)
            ManagerObject.instance.poolManager.DestroyPooled(parent.gameObject);

    }

    void init_Straight(SkillProjectile p, object[] args)
    {
        Vector3 dir = (args != null && args.Length > 0 && args[0] is Vector3 v) ? v.normalized
                         : DirFromTo(p.transform.position, p.GetCurrentTargetPos(false));

        float maxDist = Mathf.Max(0.05f, Vector3.Distance(p.transform.position, p.GetCurrentTargetPos(false)));
        p.State["dir"] = dir;
        p.State["travel"] = 0f;
        p.State["maxDist"] = maxDist;
    }

    void tick_Straight(SkillProjectile p, object[] args)
    {
        Vector3 dir = (Vector3)p.State["dir"];
        float travel = (float)p.State["travel"];
        float maxDist = (float)p.State["maxDist"];

        float step = p.Data.ProjectileSpeed * Time.deltaTime;
        travel += step;

        Vector3 next = p.transform.position + dir * step;
        p.ApplyMove(next, dir);

        if (travel >= maxDist + 0.01f) { ManagerObject.instance.poolManager.DestroyPooled(p.gameObject); return; }
        p.State["travel"] = travel;
    }

    void init_Parabola(SkillProjectile p, object[] args)
    {
        float heightBase = (args != null && args.Length > 0) ? Convert.ToSingle(args[0]) : 3.5f;

        Vector3 from = p.transform.position;
        Vector3 target = p.GetCurrentTargetPos(false);

        p.State["from"] = from;
        p.State["targetSnap"] = target;
        p.State["t"] = 0f;
        p.State["initDist"] = Vector3.Distance(from, target);
        p.State["heightBase"] = heightBase;
    }

    void tick_Parabola(SkillProjectile p, object[] args)
    {
        float t = (float)p.State["t"];
        float initDist = (float)p.State["initDist"];
        float heightBase = (float)p.State["heightBase"];
        Vector3 from = (Vector3)p.State["from"];
        Vector3 target = (Vector3)p.State["targetSnap"];

        float baseDist = (initDist > 0.01f) ? initDist : Vector3.Distance(from, target);
        t = Mathf.Clamp01(t + (p.Data.ProjectileSpeed / baseDist) * Time.deltaTime);
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

        if (Mathf.Approximately(t, 1f)) ManagerObject.instance.poolManager.DestroyPooled(p.gameObject);
    }

    Vector3 ComputeCtrl(float heightBase, Vector3 from, Vector3 to)
    {
        Vector3 mid = (from + to) * 0.5f;
        float dist = Vector3.Distance(from, to);
        float h = heightBase + 0.35f * dist;
        return new Vector3(mid.x, mid.y + h, mid.z);
    }

    void init_Down(SkillProjectile p, object[] args)
    {
        float stopY = Convert.ToSingle(args[0]);
        float xFixed = Convert.ToSingle(args[1]);

        p.State["stopY"] = stopY;
        p.State["x"] = xFixed;
        // 위치는 매니저가 스폰 시 이미 y=spawnH로 맞춰둠
    }

    void tick_Down(SkillProjectile p, object[] args)
    {
        float fallMul = (args != null && args.Length >= 3) ? Convert.ToSingle(args[2]) : 1f;
        float stopY = (float)p.State["stopY"];
        float x = (float)p.State["x"];

        Vector3 cur = p.transform.position;
        float nextY = cur.y - (p.Data.ProjectileSpeed * fallMul) * Time.deltaTime;

        Vector3 next = new Vector3(x, nextY, cur.z);
        p.ApplyMove(next, Vector3.down);

        if (p.transform.position.y <= stopY - 0.2f) ManagerObject.instance.poolManager.DestroyPooled(p.gameObject);
    }

    void init_SineStraight(SkillProjectile p, object[] args)
    {
        p.State["origin"] = (Vector3)args[0];
        p.State["forward"] = (Vector3)args[1];
        p.State["right"] = (Vector3)args[2];
        p.State["dist"] = 0f;
        p.State["maxDist"] = (float)args[3];
        p.State["amp"] = (float)args[4];
        p.State["freqHz"] = (float)args[5];
    }

    void tick_SineStraight(SkillProjectile p, object[] args)
    {
        Vector3 origin = (Vector3)p.State["origin"];
        Vector3 forward = (Vector3)p.State["forward"];
        Vector3 right = (Vector3)p.State["right"];
        float dist = (float)p.State["dist"];
        float maxDist = (float)p.State["maxDist"];
        float amp = (float)p.State["amp"];
        float freqHz = (float)p.State["freqHz"];

        float dt = Time.deltaTime;
        dist += p.Data.ProjectileSpeed * dt;

        float phase = p.Elapsed * freqHz * Mathf.PI * 2f;
        Vector3 pos = origin + forward * dist + right * (Mathf.Sin(phase) * amp);

        float d2 = dist + p.Data.ProjectileSpeed * 0.02f;
        float ph2 = phase + freqHz * Mathf.PI * 2f * 0.02f;
        Vector3 p2 = origin + forward * d2 + right * (Mathf.Sin(ph2) * amp);
        Vector3 dir = (p2 - pos).normalized;

        p.ApplyMove(pos, dir);

        if (dist >= maxDist + 0.01f) ManagerObject.instance.poolManager.DestroyPooled(p.gameObject);
        p.State["dist"] = dist;
    }

    void init_ZigZagStraight(SkillProjectile p, object[] args)
    {
        p.State["origin"] = (Vector3)args[0];
        p.State["forward"] = (Vector3)args[1];
        p.State["right"] = (Vector3)args[2];
        p.State["dist"] = 0f;
        p.State["maxDist"] = (float)args[3];
        p.State["amp"] = (float)args[4];
        p.State["freqHz"] = (float)args[5];
    }

    void tick_ZigZagStraight(SkillProjectile p, object[] args)
    {
        Vector3 origin = (Vector3)p.State["origin"];
        Vector3 forward = (Vector3)p.State["forward"];
        Vector3 right = (Vector3)p.State["right"];
        float dist = (float)p.State["dist"];
        float maxDist = (float)p.State["maxDist"];
        float amp = (float)p.State["amp"];
        float freqHz = (float)p.State["freqHz"];

        float dt = Time.deltaTime;
        dist += p.Data.ProjectileSpeed * dt;

        float wave = TriWave(p.Elapsed * freqHz);
        Vector3 pos = origin + forward * dist + right * (wave * amp);

        float d2 = dist + p.Data.ProjectileSpeed * 0.02f;
        float w2 = TriWave((p.Elapsed + 0.02f) * freqHz);
        Vector3 p2 = origin + forward * d2 + right * (w2 * amp);
        Vector3 dir = (p2 - pos).normalized;

        p.ApplyMove(pos, dir);

        if (dist >= maxDist + 0.01f) ManagerObject.instance.poolManager.DestroyPooled(p.gameObject);
        p.State["dist"] = dist;
    }

    void init_StraightSpin(SkillProjectile p, object[] args)
    {
        Vector3 dir = (args != null && args.Length > 0 && args[0] is Vector3 v) ? v.normalized
                         : DirFromTo(p.transform.position, p.GetCurrentTargetPos(false));
        float spin = (args != null && args.Length > 1) ? Convert.ToSingle(args[1]) : spinningStraightSpinSpeedDeg;

        float maxDist = Mathf.Max(0.05f, Vector3.Distance(p.transform.position, p.GetCurrentTargetPos(false)));

        p.State["dir"] = dir;
        p.State["travel"] = 0f;
        p.State["maxDist"] = maxDist;
        p.State["spinSpeed"] = spin;   // 도/초
        p.State["spinAccum"] = 0f;     // 누적 각
        // baseRot은 첫 틱에서 ApplyMove가 만든 회전을 캡처
    }

    void tick_StraightSpin(SkillProjectile p, object[] args)
    {
        Vector3 dir = (Vector3)p.State["dir"];
        float travel = (float)p.State["travel"];
        float maxDist = (float)p.State["maxDist"];
        float spinSpd = (float)p.State["spinSpeed"];
        float dt = Time.deltaTime;

        float step = p.Data.ProjectileSpeed * dt;
        travel += step;
        Vector3 next = p.transform.position + dir * step;

        // 위치/기본 방향 회전
        p.ApplyMove(next, dir);

        // 첫 프레임에 기준 회전 저장
        if (!p.State.ContainsKey("baseRot"))
            p.State["baseRot"] = p.transform.rotation;

        // 누적 회전 적용
        float accum = (float)p.State["spinAccum"];
        accum += spinSpd * dt;
        p.State["spinAccum"] = accum;

        Quaternion baseRot = (Quaternion)p.State["baseRot"];
        p.transform.rotation = baseRot * Quaternion.Euler(0f, 0f, accum);

        if (travel >= maxDist + 0.01f) { ManagerObject.instance.poolManager.DestroyPooled(p.gameObject); return; }
        p.State["travel"] = travel;
    }
}

//Monobehaviour가 필요해서 작성
internal class SkillCoroutineHub : MonoBehaviour
{
    private static SkillCoroutineHub _inst;
    public static SkillCoroutineHub Instance
    {
        get
        {
            if (_inst == null)
            {
                var go = new GameObject("~SkillCoroutineHub");
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<SkillCoroutineHub>();
            }
            return _inst;
        }
    }
}
