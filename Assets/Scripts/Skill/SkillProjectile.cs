using System;
using System.Collections;
using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [Header("스프라이트 각도 보정(도)")]
    [SerializeField] private float spriteAngleOffset = 0f;

    [Header("유도 회전 속도(도/초) - Straight 유도 시 사용")]
    [SerializeField] private float guidedTurnRate = 720f;

    [Header("포물선 기본 높이(거리 비례 가중)")]
    [SerializeField] private float parabolaHeightBase = 3.5f;

    [Header("사인/지그재그 횡진폭(유닛)")]
    [SerializeField] private float lateralAmplitude = 0.6f;

    [Header("사인/지그재그 주기(Hz) - 초당 파형 횟수")]
    [SerializeField] private float waveFrequency = 2.5f;

    [Header("부메랑 왕복 시간(초)")]
    [SerializeField] private float boomerangTotalTime = 1.6f;

    [Header("멀티샷 간격(초) - Straight/Parabola 등에서 연속 발사")]
    [SerializeField] private float multiShotInterval = 0.12f;

    [Header("멀티샷 각도 퍼짐(도) - Straight/Parabola 등")]
    [SerializeField] private float multishotSpreadAngle = 6f;

    [Header("Rain: 머리 위 높이(유닛)")]
    [SerializeField] private float rainSpawnHeight = 6f;

    [Header("Rain: 머리 위치 기준 Y 오프셋(유닛)")]
    [SerializeField] private float rainHeadYOffset = 1.0f;

    [Header("Rain: 퍼짐 기본 반경(유닛, 개수에 따라 자동 확장)")]
    [SerializeField] private float rainSpreadBase = 1.25f;

    [Header("Rain: 가로 추적 속도(유닛/초, guided=true일 때)")]
    [SerializeField] private float rainLateralFollowSpeed = 8f;

    [Header("안전장치: 최대 생존 시간(초)")]
    [SerializeField] private float maxLifeTime = 6f;

    // ===== SO 기반 상태 =====
    private CharacterTypeEnumByTag attackerType;
    private Skill skill;

    private float speed;                 // SO.projectileSpeed
    private float damage;                // SO.skillDamage
    private bool guided;                 // SO.isGuided
    private SkillProjectileMovingType moveType; // SO.skillProjectileMovingType
    private int projectileCount;         // SO.projectileCount (정수화)

    // ===== 이동 공통 =====
    private Vector3 startPos;
    private Transform targetTr;          // 유도 시 추적 대상
    private Vector3 targetPosStatic;     // 비유도일 때 고정 목표 위치
    private string enemyTag;

    private float elapsed;               // 생존 시간
    private float t;                     // 0~1 진행도(곡선/파형에서 사용)

    // 직선 이동용(유도 회전 포함)
    private Vector3 straightDir;         // 현재 진행 방향

    // 포물선
    private Vector3 controlPos;          // 제어점
    private float initDistance;          // 시작-목표 초기 거리

    // 부메랑
    private Vector3 boomerangMid;        // 전진 목표(중간점)

    // 멀티샷 재귀 방지
    private bool spawnedByParent;

    // ===== Rain 관련 =====
    private bool isRain;                 // 현재 이 탄이 Rain 모드인지
    private int rainIndex;               // Rain 내에서의 인덱스(0..N-1)
    private int rainTotal;               // Rain 전체 개수
    private Vector3 rainCenter;          // "상대 머리" 기준 중심(수평 좌표)
    private float rainRadius;            // 퍼짐 반경(개수에 비례)
    private Vector3 rainOffset;          // center에서의 수평 오프셋
    private float rainTargetY;           // 떨어져서 도달할 목표 Y(머리 높이)
    private bool rainConfigured;         // 초기 위치/오프셋 설정 완료 여부

    // ===== 외부에서 호출 =====
    public void SetProjectile(CharacterTypeEnumByTag attackerType, Skill skill)
    {
        this.attackerType = attackerType;
        this.skill = skill;

        // SO 로드
        var data = ManagerObject.skillInfoM.attackSkillData[skill];
        speed = Mathf.Max(0.01f, data.projectileSpeed);
        damage = data.skillDamage;
        guided = data.isGuided;
        moveType = data.skillProjectileMovingType;
        projectileCount = Mathf.Max(1, Mathf.RoundToInt(data.projectileCount));

        // 시작/타깃 초기화
        startPos = transform.position;

        enemyTag = Enum.GetName(
            typeof(CharacterTypeEnumByTag),
            ((int)attackerType + 1) % Enum.GetValues(typeof(CharacterTypeEnumByTag)).Length
        );
        targetTr = FindTargetByTag(enemyTag);
        targetPosStatic = (targetTr != null) ? targetTr.position : (transform.position + Vector3.right * 8f);

        // 사운드
        if (data.skillSound != null)
            ManagerObject.audioM.PlayAudioClip(data.skillSound);

        // 이동 타입별 준비
        PrepareMovement();

        // 멀티샷/버스트
        if (moveType == SkillProjectileMovingType.Rain)
        {
            // Rain은 "동시에 N개 머리 위에서" 생성 ⇒ 원본이 즉시 구성 + 나머지를 즉시 생성
            if (!spawnedByParent && projectileCount > 1)
                SpawnRainBurst(projectileCount); // 한 번에 N개
        }
        else
        {
            // 기존 방식: 원본 한 발 + 나머지는 연속 발사
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

        // 유도: 타깃 재확인(사망/비활성 대비)
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
        // 아군 태그면 무시
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
    // 초기 준비
    // =========================
    private void PrepareMovement()
    {
        Vector3 initialTarget = GetCurrentTargetPos();

        // 직선 초기 방향
        Vector3 initDir = (initialTarget - startPos);
        straightDir = initDir.sqrMagnitude > 1e-6f ? initDir.normalized : Vector3.right;

        // 포물선
        controlPos = ComputeParabolaControl(startPos, initialTarget);
        initDistance = Vector3.Distance(startPos, initialTarget);
        t = 0f;

        // 부메랑: 전진 목적지(중간 지점)
        boomerangMid = Vector3.Lerp(startPos, initialTarget, 0.65f);

        // Rain 준비 (원본 탄도 자신의 자리에서 Rain으로 재배치)
        if (moveType == SkillProjectileMovingType.Rain)
        {
            isRain = true;

            // "상대 머리" 기준 중심
            var headBase = (targetTr != null ? targetTr.position : targetPosStatic) + Vector3.up * rainHeadYOffset;
            rainCenter = headBase;

            // 퍼짐 반경: 개수 ↑ ⇒ 넓게
            // sqrt 스케일로 밀집도 균등
            int N = Mathf.Max(1, projectileCount);
            rainRadius = rainSpreadBase * Mathf.Sqrt(N);

            // 이 탄이 원본이면 index 0으로 즉시 구성
            if (!spawnedByParent)
                ConfigureRain(0, N, rainCenter, rainRadius);
        }
    }

    // =========================
    // 이동 로직
    // =========================

    // 1) 직선(유도 가능)
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

        // 목표 근접 시 파괴(유도: 현재 타깃, 비유도: 고정 목표)
        Vector3 goal = GetCurrentTargetPos();
        if ((goal - transform.position).sqrMagnitude <= 0.09f) // 0.3유닛
            Destroy(gameObject);
    }

    // 2) 포물선(2차 베지어, 유도 on이면 동적 갱신)
    private void TickParabola()
    {
        Vector3 dynTarget = GetCurrentTargetPos();

        float baseDist = (initDistance > 0.01f) ? initDistance : Vector3.Distance(startPos, dynTarget);
        if (baseDist < 0.01f) { Destroy(gameObject); return; }

        t = Mathf.Clamp01(t + (speed / baseDist) * Time.deltaTime);

        // 유도 시 제어점 갱신
        controlPos = ComputeParabolaControl(startPos, dynTarget);

        // Bezier 보간
        Vector3 a = Vector3.Lerp(startPos, controlPos, t);
        Vector3 b = Vector3.Lerp(controlPos, dynTarget, t);
        Vector3 pos = Vector3.Lerp(a, b, t);

        // 진행 방향(미분 근사)
        float t2 = Mathf.Clamp01(t + 0.02f);
        Vector3 a2 = Vector3.Lerp(startPos, controlPos, t2);
        Vector3 b2 = Vector3.Lerp(controlPos, dynTarget, t2);
        Vector3 future = Vector3.Lerp(a2, b2, t2);
        Vector3 dir = (future - pos).sqrMagnitude > 1e-6f ? (future - pos).normalized : (dynTarget - pos).normalized;

        ApplyMove(pos, dir);

        if (t >= 1f)
            Destroy(gameObject);
    }

    // 3) 사인 웨이브(기준 직선 + 횡진동)
    private void TickSineWave()
    {
        Vector3 dynTarget = GetCurrentTargetPos();
        Vector3 baseVec = dynTarget - startPos;
        float dist = baseVec.magnitude;
        if (dist < 0.01f) { Destroy(gameObject); return; }

        // 진행도
        t = Mathf.Clamp01(t + (speed / dist) * Time.deltaTime);

        Vector3 forward = baseVec / dist;
        Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized; // 2D(z+)에서 횡방향
        Vector3 basePos = Vector3.Lerp(startPos, dynTarget, t);

        float phase = elapsed * waveFrequency * Mathf.PI * 2f;
        Vector3 offset = right * (Mathf.Sin(phase) * lateralAmplitude);
        Vector3 pos = basePos + offset;

        // 다음 프레임 예상
        float dt = 0.02f;
        float tF = Mathf.Clamp01(t + (speed / dist) * dt);
        Vector3 basePosF = Vector3.Lerp(startPos, dynTarget, tF);
        Vector3 offsetF = right * (Mathf.Sin(phase + waveFrequency * Mathf.PI * 2f * dt) * lateralAmplitude);
        Vector3 dir = (basePosF + offsetF - pos).normalized;

        ApplyMove(pos, dir);

        if (t >= 1f)
            Destroy(gameObject);
    }

    // 4) 지그재그(삼각파 횡진동)
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

        // 삼각파 -1..1
        float tri = 2f * Mathf.Abs(2f * (elapsed * waveFrequency - Mathf.Floor(elapsed * waveFrequency + 0.5f))) - 1f;
        Vector3 offset = right * (tri * lateralAmplitude);
        Vector3 pos = basePos + offset;

        // 다음 프레임 예측
        float dt = 0.02f;
        float triF = 2f * Mathf.Abs(2f * ((elapsed + dt) * waveFrequency - Mathf.Floor((elapsed + dt) * waveFrequency + 0.5f))) - 1f;
        Vector3 dir = ((Vector3.Lerp(startPos, dynTarget, Mathf.Clamp01(t + (speed / dist) * dt)) + right * (triF * lateralAmplitude)) - pos).normalized;

        ApplyMove(pos, dir);

        if (t >= 1f)
            Destroy(gameObject);
    }

    // 5) 부메랑(전진→복귀)
    private void TickBoomerang()
    {
        float p = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, boomerangTotalTime));

        Vector3 outPoint = (guided && targetTr != null) ? targetTr.position : boomerangMid;

        Vector3 pos = (p <= 0.5f)
            ? Vector3.Lerp(startPos, outPoint, p / 0.5f)            // 전진
            : Vector3.Lerp(outPoint, startPos, (p - 0.5f) / 0.5f);  // 복귀

        // 진행 방향
        float pf = Mathf.Clamp01(p + 0.02f);
        Vector3 posF = (pf <= 0.5f)
            ? Vector3.Lerp(startPos, outPoint, pf / 0.5f)
            : Vector3.Lerp(outPoint, startPos, (pf - 0.5f) / 0.5f);

        Vector3 dir = (posF - pos).normalized;

        // 속도감 부여(프레임 독립 이동)
        Vector3 cur = Vector3.MoveTowards(transform.position, pos, speed * Time.deltaTime);
        ApplyMove(cur, dir);

        if (Mathf.Approximately(p, 1f))
            Destroy(gameObject);
    }

    // 6) Rain(머리 위에서 낙하, N개 동시/퍼짐, guided면 수평 추적)
    private void TickRain()
    {
        if (!rainConfigured)
        {
            // 예외 보호: 혹시 원본에서 ConfigureRain이 안 됐을 경우
            ConfigureRain(0, Mathf.Max(1, projectileCount),
                (targetTr != null ? targetTr.position : targetPosStatic) + Vector3.up * rainHeadYOffset,
                rainSpreadBase * Mathf.Sqrt(Mathf.Max(1, projectileCount)));
        }

        // guided면 머리 중심을 계속 최신화하여 수평 추적
        if (guided && targetTr != null)
        {
            Vector3 desiredCenter = targetTr.position + Vector3.up * rainHeadYOffset;
            rainCenter = Vector3.MoveTowards(rainCenter, desiredCenter, rainLateralFollowSpeed * Time.deltaTime);
        }

        // 목표 수평 위치 = 최신 중심 + 나의 오프셋
        Vector3 horizontalTarget = rainCenter + rainOffset;

        // 현재 위치에서 수직 하강
        Vector3 cur = transform.position;
        float nextY = cur.y - speed * Time.deltaTime;

        // 수평 보정(guided일 때 가로 따라가기)
        float nextX = guided
            ? Mathf.MoveTowards(cur.x, horizontalTarget.x, rainLateralFollowSpeed * Time.deltaTime)
            : cur.x; // 비유도면 수평 고정

        Vector3 next = new Vector3(nextX, nextY, cur.z);

        // 진행 방향(아래 + 약간의 수평)
        Vector3 dir = (next - cur);
        if (dir.sqrMagnitude < 1e-8f) dir = Vector3.down;
        ApplyMove(next, dir.normalized);

        // 머리 높이 아래로 내려가면 정리(충돌로 이미 파괴될 수 있음)
        if (transform.position.y <= rainTargetY - 0.2f)
            Destroy(gameObject);
    }

    // =========================
    // 유틸
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
        float height = parabolaHeightBase + 0.35f * dist; // 거리 비례 상승
        return new Vector3(mid.x, mid.y + height, mid.z);
    }

    // ---- 멀티샷(일반 타입용) ----
    private IEnumerator SpawnAdditionalProjectiles(int extraCount)
    {
        var data = ManagerObject.skillInfoM.attackSkillData[skill];

        // 발사 기준 방향(퍼짐용)
        Vector3 baseDir = (GetCurrentTargetPos() - startPos).normalized;
        if (baseDir.sqrMagnitude < 1e-6f) baseDir = Vector3.right;

        for (int i = 0; i < extraCount; i++)
        {
            yield return new WaitForSeconds(multiShotInterval);

            // 약간의 퍼짐 각도
            float spread = (multishotSpreadAngle > 0f) ? UnityEngine.Random.Range(-multishotSpreadAngle, multishotSpreadAngle) : 0f;
            Quaternion rot = Quaternion.AngleAxis(spread, Vector3.forward);
            Vector3 spawnDir = rot * baseDir;

            // 시작 위치를 소폭 오프셋하면 겹침 감소(선택)
            Vector3 spawnPos = startPos + spawnDir * 0.05f;

            var go = Instantiate(data.skillProjectile, spawnPos, Quaternion.identity);
            var proj = go.GetComponent<SkillProjectile>();
            proj.spawnedByParent = true; // 자식은 추가 발사 금지
            proj.SetProjectile(attackerType, skill);
        }
    }

    // ---- Rain 전용: 한 번에 N개 동시 생성 ----
    private void SpawnRainBurst(int total)
    {
        // 중심/반경 재평가(혹시 SetProjectile 호출 직후 타깃이 바뀐 경우)
        var headBase = (targetTr != null ? targetTr.position : targetPosStatic) + Vector3.up * rainHeadYOffset;
        Vector3 center = headBase;
        float radius = rainSpreadBase * Mathf.Sqrt(Mathf.Max(1, total));

        // 현재(원본) 탄은 index 0으로 이미 ConfigureRain 호출됨(PrepareMovement)
        // 나머지 N-1 생성
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

    // Rain 개별 탄 초기 구성(자리/오프셋/스폰 위치)
    private void ConfigureRain(int index, int total, Vector3 center, float radius)
    {
        isRain = true;
        rainIndex = index;
        rainTotal = Mathf.Max(1, total);
        rainCenter = center;
        rainRadius = Mathf.Max(0f, radius);

        // Vogel(골든앵글) 분포로 균일 퍼짐
        // r = R * sqrt((i+0.5)/N), theta = i * goldenAngle
        const float goldenAngle = 137.50776405f * Mathf.Deg2Rad;
        float r = rainRadius * Mathf.Sqrt((index + 0.5f) / rainTotal);
        float theta = index * goldenAngle;

        // 2D(좌표계: x-수평, y-수직)
        Vector3 planar = new Vector3(Mathf.Cos(theta) * r, 0f, 0f);
        // 좌우만 퍼지도록 x축 사용 (원하면 z=0 고정)
        rainOffset = new Vector3(planar.x, 0f, 0f);

        // 스폰 위치 = (머리 중심 + 오프셋) + 위로 rainSpawnHeight
        Vector3 spawnPos = (rainCenter + rainOffset) + Vector3.up * rainSpawnHeight;

        // 이 탄이 이미 월드에 존재하므로 위치 재설정
        transform.position = spawnPos;
        startPos = spawnPos;

        // 떨어질 목표 Y(머리 높이)
        rainTargetY = rainCenter.y;

        // 아래 방향 바라보게 초기 회전
        ApplyMove(spawnPos, Vector3.down);

        rainConfigured = true;
    }
}
