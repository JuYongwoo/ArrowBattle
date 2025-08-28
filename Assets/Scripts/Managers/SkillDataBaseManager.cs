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
    Rain,     // Down N�� �����ϰ� �θ�� ��� ����
    Down,     // �Ӹ� �� ���� ����, �Ʒ��� ���ϸ�
    StraightThreeMultipleShot // ����: 3���� ����
}

public class SkillDataBaseManager
{
    public Dictionary<Skill, SkillDataSO> attackSkillData;

    public int rainDownCount = 6;    // Down ����
    public float downSpawnHeight = 6f;   // �Ӹ� �� ����
    public float downWidth = 4f;   // ���� ���� ��
    public float headYOffset = 1.0f; // �Ӹ� ���� ������
    public float downFallSpeedMul = 1f;   // ���� �ӵ� ���

    // StraightThreeMultipleShot
    public int tripleShots = 3;
    public float tripleInterval = 0.12f;

    // Parabola
    public float parabolaHeightBase = 3.5f;

    // �ڷ�ƾ �߻��
    private MonoBehaviour _runner;

    // ��Ÿ��
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

    // === ��Ÿ�� ���� ===
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

    // === �߻� ===
    public void shoot(CharacterTypeEnumByTag casterType, Vector3 startPosition, Skill skill)
    {
        var so = attackSkillData[skill];
        var proj = UnityEngine.Object.Instantiate(so.skillProjectile, startPosition, Quaternion.identity)
                    .GetComponent<SkillProjectile>();

        var type = so.skillProjectileMovingType;

        // Ÿ�Ժ��� spawn/tick �� params ���� (�⺻ ���� ����)
        switch (type)
        {
            case SkillProjectileMovingType.Rain:
                // Rain�� Down N���� ����� �θ�� ��� ���� (Ÿ�� �������� ���)
                proj.SetProjectile(
                    casterType, so,
                    spawnAction_Rain, tickAction_NoOp,
                    // params: downCount, downHeight, downWidth, headYOffset, fallSpeedMul
                    rainDownCount, downSpawnHeight, downWidth, headYOffset, downFallSpeedMul
                );
                break;

            case SkillProjectileMovingType.Down:
                // Down ����
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
                // �⺻ Straight
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

    // --- ���� ��ƿ ---
    static Vector3 DirFromTo(Vector3 from, Vector3 to)
    {
        var v = to - from;
        return (v.sqrMagnitude > 1e-6f) ? v.normalized : Vector3.right;
    }

    // ================== Straight (������) ==================
    void spawnAction_Straight(SkillProjectile p, object[] args)
    {
        // ���� ������ "Ÿ�� ������" �������� ���� �� �ִ� �̵��Ÿ� ���
        Vector3 from = p.transform.position;
        Vector3 to = p.TargetPosStatic;

        Vector3 dir = DirFromTo(from, to);
        float maxDist = Mathf.Max(0.05f, Vector3.Distance(from, to)); // �ּҰ� ����

        p.State["dir"] = dir;
        p.State["travel"] = 0f;         // ���� �̵� �Ÿ�
        p.State["maxDist"] = maxDist;    // �ʰ� �� �ı�
    }

    void tickAction_Straight(SkillProjectile p, object[] args)
    {
        Vector3 dir = (Vector3)p.State["dir"];
        float travel = (float)p.State["travel"];
        float maxDist = (float)p.State["maxDist"];

        float step = p.Data.projectileSpeed * Time.deltaTime;
        travel += step;

        // �̵�
        Vector3 next = p.transform.position + dir * step;
        p.ApplyMove(next, dir);

        // ��ǥ�� ����/�ʰ� �� ��� �Ҹ�(ȭ�� ���� ����)
        if (travel >= maxDist + 0.01f)
        {
            p.DestroySelf();
            return;
        }

        p.State["travel"] = travel;
    }

    // ================== Parabola (������) ==================
    void spawnAction_Parabola(SkillProjectile p, object[] args)
    {
        // args[0] = parabolaHeightBase (manager Ʃ��)
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

        // ���� �����ϸ� ��� �Ҹ�
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

    // ================== Rain �� Down N ==================
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

            // Down�� �Ķ���� ����: centerX, centerY, spawnHeight, xOffset, fallMul
            child.SetProjectile(
                p.AttackerType, p.Data,
                spawnAction_Down, tickAction_Down,
                centerBase.x, centerBase.y, spawnH, xOffset, fallMul
            );
        }

        // �θ� Rain�� ��� ����
        p.DestroySelf();
    }

    // ================== Down (�ܼ� ����) ==================
    // args: [0]=centerX, [1]=centerY, [2]=spawnHeight, [3]=xOffset, [4]=fallMul
    void spawnAction_Down(SkillProjectile p, object[] args)
    {
        float cx = Convert.ToSingle(args[0]);
        float cy = Convert.ToSingle(args[1]);
        float spawnH = Convert.ToSingle(args[2]);
        float xOffset = Convert.ToSingle(args[3]);

        // ���� ��ġ: �Ӹ� �� spawnH
        Vector3 spawnPos = new Vector3(cx + xOffset, cy + spawnH, p.transform.position.z);
        p.transform.position = spawnPos;

        // ���� ���� Y ����(�Ӹ� ����), X�� ����
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

    // ================== No-Op (Rain �θ� ��) ==================
    void tickAction_NoOp(SkillProjectile p, object[] args)
    {
        // �ƹ� �͵� �� �� (�θ�� �̹� DestroySelf ȣ��)
    }
}
