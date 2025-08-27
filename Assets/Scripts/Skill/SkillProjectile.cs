using System;
using System.Collections;
using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [Header("��������Ʈ ���� ����(��)")]
    [SerializeField] private float spriteAngleOffset = 0f;

    [Header("���� ȸ�� �ӵ�(��/��) - Straight ���� �� ���")]
    [SerializeField] private float guidedTurnRate = 720f;

    [Header("������ �⺻ ����(�Ÿ� ��� ����)")]
    [SerializeField] private float parabolaHeightBase = 3.5f;

    [Header("����/������� Ⱦ����(����)")]
    [SerializeField] private float lateralAmplitude = 0.6f;

    [Header("����/������� �ֱ�(Hz) - �ʴ� ���� Ƚ��")]
    [SerializeField] private float waveFrequency = 2.5f;

    [Header("�θ޶� �պ� �ð�(��)")]
    [SerializeField] private float boomerangTotalTime = 1.6f;

    [Header("��Ƽ�� ����(��) - Straight/Parabola ��� ���� �߻�")]
    [SerializeField] private float multiShotInterval = 0.12f;

    [Header("��Ƽ�� ���� ����(��) - Straight/Parabola ��")]
    [SerializeField] private float multishotSpreadAngle = 6f;

    [Header("Rain: �Ӹ� �� ����(����)")]
    [SerializeField] private float rainSpawnHeight = 6f;

    [Header("Rain: �Ӹ� ��ġ ���� Y ������(����)")]
    [SerializeField] private float rainHeadYOffset = 1.0f;

    [Header("Rain: ���� �⺻ �ݰ�(����, ������ ���� �ڵ� Ȯ��)")]
    [SerializeField] private float rainSpreadBase = 1.25f;

    [Header("Rain: ���� ���� �ӵ�(����/��, guided=true�� ��)")]
    [SerializeField] private float rainLateralFollowSpeed = 8f;

    [Header("������ġ: �ִ� ���� �ð�(��)")]
    [SerializeField] private float maxLifeTime = 6f;

    // ===== SO ��� ���� =====
    private CharacterTypeEnumByTag attackerType;
    private Skill skill;

    private float speed;                 // SO.projectileSpeed
    private float damage;                // SO.skillDamage
    private bool guided;                 // SO.isGuided
    private SkillProjectileMovingType moveType; // SO.skillProjectileMovingType
    private int projectileCount;         // SO.projectileCount (����ȭ)

    // ===== �̵� ���� =====
    private Vector3 startPos;
    private Transform targetTr;          // ���� �� ���� ���
    private Vector3 targetPosStatic;     // �������� �� ���� ��ǥ ��ġ
    private string enemyTag;

    private float elapsed;               // ���� �ð�
    private float t;                     // 0~1 ���൵(�/�������� ���)

    // ���� �̵���(���� ȸ�� ����)
    private Vector3 straightDir;         // ���� ���� ����

    // ������
    private Vector3 controlPos;          // ������
    private float initDistance;          // ����-��ǥ �ʱ� �Ÿ�

    // �θ޶�
    private Vector3 boomerangMid;        // ���� ��ǥ(�߰���)

    // ��Ƽ�� ��� ����
    private bool spawnedByParent;

    // ===== Rain ���� =====
    private bool isRain;                 // ���� �� ź�� Rain �������
    private int rainIndex;               // Rain �������� �ε���(0..N-1)
    private int rainTotal;               // Rain ��ü ����
    private Vector3 rainCenter;          // "��� �Ӹ�" ���� �߽�(���� ��ǥ)
    private float rainRadius;            // ���� �ݰ�(������ ���)
    private Vector3 rainOffset;          // center������ ���� ������
    private float rainTargetY;           // �������� ������ ��ǥ Y(�Ӹ� ����)
    private bool rainConfigured;         // �ʱ� ��ġ/������ ���� �Ϸ� ����

    // ===== �ܺο��� ȣ�� =====
    public void SetProjectile(CharacterTypeEnumByTag attackerType, Skill skill)
    {
        this.attackerType = attackerType;
        this.skill = skill;

        // SO �ε�
        var data = ManagerObject.skillInfoM.attackSkillData[skill];
        speed = Mathf.Max(0.01f, data.projectileSpeed);
        damage = data.skillDamage;
        guided = data.isGuided;
        moveType = data.skillProjectileMovingType;
        projectileCount = Mathf.Max(1, Mathf.RoundToInt(data.projectileCount));

        // ����/Ÿ�� �ʱ�ȭ
        startPos = transform.position;

        enemyTag = Enum.GetName(
            typeof(CharacterTypeEnumByTag),
            ((int)attackerType + 1) % Enum.GetValues(typeof(CharacterTypeEnumByTag)).Length
        );
        targetTr = FindTargetByTag(enemyTag);
        targetPosStatic = (targetTr != null) ? targetTr.position : (transform.position + Vector3.right * 8f);

        // ����
        if (data.skillSound != null)
            ManagerObject.audioM.PlayAudioClip(data.skillSound);

        // �̵� Ÿ�Ժ� �غ�
        PrepareMovement();

        // ��Ƽ��/����Ʈ
        if (moveType == SkillProjectileMovingType.Rain)
        {
            // Rain�� "���ÿ� N�� �Ӹ� ������" ���� �� ������ ��� ���� + �������� ��� ����
            if (!spawnedByParent && projectileCount > 1)
                SpawnRainBurst(projectileCount); // �� ���� N��
        }
        else
        {
            // ���� ���: ���� �� �� + �������� ���� �߻�
            if (!spawnedByParent && projectileCount > 1)
                StartCoroutine(SpawnAdditionalProjectiles(projectileCount - 1));
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= maxLifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // ����: Ÿ�� ��Ȯ��(���/��Ȱ�� ���)
        if (guided && (targetTr == null || !targetTr.gameObject.activeInHierarchy))
            targetTr = FindTargetByTag(enemyTag);

        switch (moveType)
        {
            case SkillProjectileMovingType.Straight: TickStraight(); break;
            case SkillProjectileMovingType.Parabola: TickParabola(); break;
            case SkillProjectileMovingType.SineWave: TickSineWave(); break;
            case SkillProjectileMovingType.ZigZag: TickZigZag(); break;
            case SkillProjectileMovingType.Boomerang: TickBoomerang(); break;
            case SkillProjectileMovingType.Rain: TickRain(); break;
            default: TickStraight(); break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �Ʊ� �±׸� ����
        if (other.CompareTag(Enum.GetName(typeof(CharacterTypeEnumByTag), attackerType)))
            return;

        var stat = other.GetComponent<CharacterBase>();
        if (stat != null)
        {
            stat.getDamaged(damage);
            Destroy(gameObject);
        }
    }

    // =========================
    // �ʱ� �غ�
    // =========================
    private void PrepareMovement()
    {
        Vector3 initialTarget = GetCurrentTargetPos();

        // ���� �ʱ� ����
        Vector3 initDir = (initialTarget - startPos);
        straightDir = initDir.sqrMagnitude > 1e-6f ? initDir.normalized : Vector3.right;

        // ������
        controlPos = ComputeParabolaControl(startPos, initialTarget);
        initDistance = Vector3.Distance(startPos, initialTarget);
        t = 0f;

        // �θ޶�: ���� ������(�߰� ����)
        boomerangMid = Vector3.Lerp(startPos, initialTarget, 0.65f);

        // Rain �غ� (���� ź�� �ڽ��� �ڸ����� Rain���� ���ġ)
        if (moveType == SkillProjectileMovingType.Rain)
        {
            isRain = true;

            // "��� �Ӹ�" ���� �߽�
            var headBase = (targetTr != null ? targetTr.position : targetPosStatic) + Vector3.up * rainHeadYOffset;
            rainCenter = headBase;

            // ���� �ݰ�: ���� �� �� �а�
            // sqrt �����Ϸ� ������ �յ�
            int N = Mathf.Max(1, projectileCount);
            rainRadius = rainSpreadBase * Mathf.Sqrt(N);

            // �� ź�� �����̸� index 0���� ��� ����
            if (!spawnedByParent)
                ConfigureRain(0, N, rainCenter, rainRadius);
        }
    }

    // =========================
    // �̵� ����
    // =========================

    // 1) ����(���� ����)
    private void TickStraight()
    {
        Vector3 cur = transform.position;
        Vector3 dir = straightDir;

        if (guided && targetTr != null)
        {
            Vector3 toTarget = (targetTr.position - cur);
            if (toTarget.sqrMagnitude > 1e-6f)
            {
                float maxRad = Mathf.Deg2Rad * guidedTurnRate * Time.deltaTime;
                dir = Vector3.RotateTowards(straightDir, toTarget.normalized, maxRad, 1f).normalized;
            }
        }

        straightDir = dir;

        Vector3 next = cur + dir * speed * Time.deltaTime;
        ApplyMove(next, dir);

        // ��ǥ ���� �� �ı�(����: ���� Ÿ��, ������: ���� ��ǥ)
        Vector3 goal = GetCurrentTargetPos();
        if ((goal - transform.position).sqrMagnitude <= 0.09f) // 0.3����
            Destroy(gameObject);
    }

    // 2) ������(2�� ������, ���� on�̸� ���� ����)
    private void TickParabola()
    {
        Vector3 dynTarget = GetCurrentTargetPos();

        float baseDist = (initDistance > 0.01f) ? initDistance : Vector3.Distance(startPos, dynTarget);
        if (baseDist < 0.01f) { Destroy(gameObject); return; }

        t = Mathf.Clamp01(t + (speed / baseDist) * Time.deltaTime);

        // ���� �� ������ ����
        controlPos = ComputeParabolaControl(startPos, dynTarget);

        // Bezier ����
        Vector3 a = Vector3.Lerp(startPos, controlPos, t);
        Vector3 b = Vector3.Lerp(controlPos, dynTarget, t);
        Vector3 pos = Vector3.Lerp(a, b, t);

        // ���� ����(�̺� �ٻ�)
        float t2 = Mathf.Clamp01(t + 0.02f);
        Vector3 a2 = Vector3.Lerp(startPos, controlPos, t2);
        Vector3 b2 = Vector3.Lerp(controlPos, dynTarget, t2);
        Vector3 future = Vector3.Lerp(a2, b2, t2);
        Vector3 dir = (future - pos).sqrMagnitude > 1e-6f ? (future - pos).normalized : (dynTarget - pos).normalized;

        ApplyMove(pos, dir);

        if (t >= 1f)
            Destroy(gameObject);
    }

    // 3) ���� ���̺�(���� ���� + Ⱦ����)
    private void TickSineWave()
    {
        Vector3 dynTarget = GetCurrentTargetPos();
        Vector3 baseVec = dynTarget - startPos;
        float dist = baseVec.magnitude;
        if (dist < 0.01f) { Destroy(gameObject); return; }

        // ���൵
        t = Mathf.Clamp01(t + (speed / dist) * Time.deltaTime);

        Vector3 forward = baseVec / dist;
        Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized; // 2D(z+)���� Ⱦ����
        Vector3 basePos = Vector3.Lerp(startPos, dynTarget, t);

        float phase = elapsed * waveFrequency * Mathf.PI * 2f;
        Vector3 offset = right * (Mathf.Sin(phase) * lateralAmplitude);
        Vector3 pos = basePos + offset;

        // ���� ������ ����
        float dt = 0.02f;
        float tF = Mathf.Clamp01(t + (speed / dist) * dt);
        Vector3 basePosF = Vector3.Lerp(startPos, dynTarget, tF);
        Vector3 offsetF = right * (Mathf.Sin(phase + waveFrequency * Mathf.PI * 2f * dt) * lateralAmplitude);
        Vector3 dir = (basePosF + offsetF - pos).normalized;

        ApplyMove(pos, dir);

        if (t >= 1f)
            Destroy(gameObject);
    }

    // 4) �������(�ﰢ�� Ⱦ����)
    private void TickZigZag()
    {
        Vector3 dynTarget = GetCurrentTargetPos();
        Vector3 baseVec = dynTarget - startPos;
        float dist = baseVec.magnitude;
        if (dist < 0.01f) { Destroy(gameObject); return; }

        t = Mathf.Clamp01(t + (speed / dist) * Time.deltaTime);

        Vector3 forward = baseVec / dist;
        Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized;
        Vector3 basePos = Vector3.Lerp(startPos, dynTarget, t);

        // �ﰢ�� -1..1
        float tri = 2f * Mathf.Abs(2f * (elapsed * waveFrequency - Mathf.Floor(elapsed * waveFrequency + 0.5f))) - 1f;
        Vector3 offset = right * (tri * lateralAmplitude);
        Vector3 pos = basePos + offset;

        // ���� ������ ����
        float dt = 0.02f;
        float triF = 2f * Mathf.Abs(2f * ((elapsed + dt) * waveFrequency - Mathf.Floor((elapsed + dt) * waveFrequency + 0.5f))) - 1f;
        Vector3 dir = ((Vector3.Lerp(startPos, dynTarget, Mathf.Clamp01(t + (speed / dist) * dt)) + right * (triF * lateralAmplitude)) - pos).normalized;

        ApplyMove(pos, dir);

        if (t >= 1f)
            Destroy(gameObject);
    }

    // 5) �θ޶�(�����溹��)
    private void TickBoomerang()
    {
        float p = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, boomerangTotalTime));

        Vector3 outPoint = (guided && targetTr != null) ? targetTr.position : boomerangMid;

        Vector3 pos = (p <= 0.5f)
            ? Vector3.Lerp(startPos, outPoint, p / 0.5f)            // ����
            : Vector3.Lerp(outPoint, startPos, (p - 0.5f) / 0.5f);  // ����

        // ���� ����
        float pf = Mathf.Clamp01(p + 0.02f);
        Vector3 posF = (pf <= 0.5f)
            ? Vector3.Lerp(startPos, outPoint, pf / 0.5f)
            : Vector3.Lerp(outPoint, startPos, (pf - 0.5f) / 0.5f);

        Vector3 dir = (posF - pos).normalized;

        // �ӵ��� �ο�(������ ���� �̵�)
        Vector3 cur = Vector3.MoveTowards(transform.position, pos, speed * Time.deltaTime);
        ApplyMove(cur, dir);

        if (Mathf.Approximately(p, 1f))
            Destroy(gameObject);
    }

    // 6) Rain(�Ӹ� ������ ����, N�� ����/����, guided�� ���� ����)
    private void TickRain()
    {
        if (!rainConfigured)
        {
            // ���� ��ȣ: Ȥ�� �������� ConfigureRain�� �� ���� ���
            ConfigureRain(0, Mathf.Max(1, projectileCount),
                (targetTr != null ? targetTr.position : targetPosStatic) + Vector3.up * rainHeadYOffset,
                rainSpreadBase * Mathf.Sqrt(Mathf.Max(1, projectileCount)));
        }

        // guided�� �Ӹ� �߽��� ��� �ֽ�ȭ�Ͽ� ���� ����
        if (guided && targetTr != null)
        {
            Vector3 desiredCenter = targetTr.position + Vector3.up * rainHeadYOffset;
            rainCenter = Vector3.MoveTowards(rainCenter, desiredCenter, rainLateralFollowSpeed * Time.deltaTime);
        }

        // ��ǥ ���� ��ġ = �ֽ� �߽� + ���� ������
        Vector3 horizontalTarget = rainCenter + rainOffset;

        // ���� ��ġ���� ���� �ϰ�
        Vector3 cur = transform.position;
        float nextY = cur.y - speed * Time.deltaTime;

        // ���� ����(guided�� �� ���� ���󰡱�)
        float nextX = guided
            ? Mathf.MoveTowards(cur.x, horizontalTarget.x, rainLateralFollowSpeed * Time.deltaTime)
            : cur.x; // �������� ���� ����

        Vector3 next = new Vector3(nextX, nextY, cur.z);

        // ���� ����(�Ʒ� + �ణ�� ����)
        Vector3 dir = (next - cur);
        if (dir.sqrMagnitude < 1e-8f) dir = Vector3.down;
        ApplyMove(next, dir.normalized);

        // �Ӹ� ���� �Ʒ��� �������� ����(�浹�� �̹� �ı��� �� ����)
        if (transform.position.y <= rainTargetY - 0.2f)
            Destroy(gameObject);
    }

    // =========================
    // ��ƿ
    // =========================
    private void ApplyMove(Vector3 newPos, Vector3 dir)
    {
        transform.position = newPos;

        if (dir.sqrMagnitude > 1e-6f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + spriteAngleOffset);
        }
    }

    private Vector3 GetCurrentTargetPos()
    {
        if (guided && targetTr != null) return targetTr.position;
        return targetPosStatic;
    }

    private Transform FindTargetByTag(string tagName)
    {
        GameObject go = GameObject.FindGameObjectWithTag(tagName);
        return go != null ? go.transform : null;
    }

    private Vector3 ComputeParabolaControl(Vector3 from, Vector3 to)
    {
        Vector3 mid = (from + to) * 0.5f;
        float dist = Vector3.Distance(from, to);
        float height = parabolaHeightBase + 0.35f * dist; // �Ÿ� ��� ���
        return new Vector3(mid.x, mid.y + height, mid.z);
    }

    // ---- ��Ƽ��(�Ϲ� Ÿ�Կ�) ----
    private IEnumerator SpawnAdditionalProjectiles(int extraCount)
    {
        var data = ManagerObject.skillInfoM.attackSkillData[skill];

        // �߻� ���� ����(������)
        Vector3 baseDir = (GetCurrentTargetPos() - startPos).normalized;
        if (baseDir.sqrMagnitude < 1e-6f) baseDir = Vector3.right;

        for (int i = 0; i < extraCount; i++)
        {
            yield return new WaitForSeconds(multiShotInterval);

            // �ణ�� ���� ����
            float spread = (multishotSpreadAngle > 0f) ? UnityEngine.Random.Range(-multishotSpreadAngle, multishotSpreadAngle) : 0f;
            Quaternion rot = Quaternion.AngleAxis(spread, Vector3.forward);
            Vector3 spawnDir = rot * baseDir;

            // ���� ��ġ�� ���� �������ϸ� ��ħ ����(����)
            Vector3 spawnPos = startPos + spawnDir * 0.05f;

            var go = Instantiate(data.skillProjectile, spawnPos, Quaternion.identity);
            var proj = go.GetComponent<SkillProjectile>();
            proj.spawnedByParent = true; // �ڽ��� �߰� �߻� ����
            proj.SetProjectile(attackerType, skill);
        }
    }

    // ---- Rain ����: �� ���� N�� ���� ���� ----
    private void SpawnRainBurst(int total)
    {
        // �߽�/�ݰ� ����(Ȥ�� SetProjectile ȣ�� ���� Ÿ���� �ٲ� ���)
        var headBase = (targetTr != null ? targetTr.position : targetPosStatic) + Vector3.up * rainHeadYOffset;
        Vector3 center = headBase;
        float radius = rainSpreadBase * Mathf.Sqrt(Mathf.Max(1, total));

        // ����(����) ź�� index 0���� �̹� ConfigureRain ȣ���(PrepareMovement)
        // ������ N-1 ����
        var data = ManagerObject.skillInfoM.attackSkillData[skill];
        for (int i = 1; i < total; i++)
        {
            var go = Instantiate(data.skillProjectile, startPos, Quaternion.identity);
            var proj = go.GetComponent<SkillProjectile>();
            proj.spawnedByParent = true;
            proj.SetProjectile(attackerType, skill);
            proj.ConfigureRain(i, total, center, radius);
        }
    }

    // Rain ���� ź �ʱ� ����(�ڸ�/������/���� ��ġ)
    private void ConfigureRain(int index, int total, Vector3 center, float radius)
    {
        isRain = true;
        rainIndex = index;
        rainTotal = Mathf.Max(1, total);
        rainCenter = center;
        rainRadius = Mathf.Max(0f, radius);

        // Vogel(���ޱ�) ������ ���� ����
        // r = R * sqrt((i+0.5)/N), theta = i * goldenAngle
        const float goldenAngle = 137.50776405f * Mathf.Deg2Rad;
        float r = rainRadius * Mathf.Sqrt((index + 0.5f) / rainTotal);
        float theta = index * goldenAngle;

        // 2D(��ǥ��: x-����, y-����)
        Vector3 planar = new Vector3(Mathf.Cos(theta) * r, 0f, 0f);
        // �¿츸 �������� x�� ��� (���ϸ� z=0 ����)
        rainOffset = new Vector3(planar.x, 0f, 0f);

        // ���� ��ġ = (�Ӹ� �߽� + ������) + ���� rainSpawnHeight
        Vector3 spawnPos = (rainCenter + rainOffset) + Vector3.up * rainSpawnHeight;

        // �� ź�� �̹� ���忡 �����ϹǷ� ��ġ �缳��
        transform.position = spawnPos;
        startPos = spawnPos;

        // ������ ��ǥ Y(�Ӹ� ����)
        rainTargetY = rainCenter.y;

        // �Ʒ� ���� �ٶ󺸰� �ʱ� ȸ��
        ApplyMove(spawnPos, Vector3.down);

        rainConfigured = true;
    }
}
