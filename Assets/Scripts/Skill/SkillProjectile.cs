using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillProjectile : MonoBehaviour, PooledObject
{
    [Header("스프라이트 각도 보정(도)")]
    [SerializeField] private float spriteAngleOffset = -45f;

    public CharacterTypeEnumByTag AttackerType { get; private set; }
    public SkillData Data { get; private set; }

    public Transform TargetTr { get; private set; }
    public Vector3 TargetPosStatic { get; private set; }
    public string EnemyTag { get; private set; }

    public Vector3 StartPos { get; private set; }
    public float Elapsed { get; private set; }

    // 실행 중 상태 저장용(각 액션들이 자유롭게 사용)
    public readonly Dictionary<string, object> State = new();

    private Action<SkillProjectile, object[]> _onInit;
    private Action<SkillProjectile, object[]> _onTick;
    private object[] _args; // params 보관

    public void PoolStart()
    {

    }

    public void PoolDestroy()
    {

    }

    public void SetProjectile(
        CharacterTypeEnumByTag attackerType,
        SkillData SkillData,
        Action<SkillProjectile, object[]> initAction,
        Action<SkillProjectile, object[]> tickAction,
        params object[] args
    )
    {
        AttackerType = attackerType;
        Data = SkillData;

        StartPos = transform.position;

        EnemyTag = Enum.GetName(
            typeof(CharacterTypeEnumByTag),
            ((int)attackerType + 1) % Enum.GetValues(typeof(CharacterTypeEnumByTag)).Length
        );

        var targetTr = FindTargetByTag(EnemyTag);
        TargetTr = targetTr; // 참고용으로 보관
        TargetPosStatic = (targetTr != null) ? targetTr.position : (transform.position + Vector3.right * 8f);

        _onInit = initAction;
        _onTick = tickAction;
        _args = args;

        _onInit?.Invoke(this, _args);
    }

    private void Update()
    {
        Elapsed += Time.deltaTime;
        _onTick?.Invoke(this, _args);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Enum.GetName(typeof(CharacterTypeEnumByTag), AttackerType)))
            return;

        if (other.CompareTag("DeadZone"))
        {
            ManagerObject.instance.poolManager.DestroyPooled(gameObject);
            return;
        }

        var stat = other.GetComponent<CharacterBase>();
        if (stat != null)
        {
            stat.GetDamaged(Data.SkillDamage);
            ManagerObject.instance.poolManager.DestroyPooled(gameObject);
        }
    }

    public Vector3 GetCurrentTargetPos(bool isCurrent)
    {
        if (isCurrent && TargetTr != null)
            return TargetTr.position;
        else
            return TargetPosStatic;
    }

    public void ApplyMove(Vector3 newPos, Vector3 facingDir)
    {
        transform.position = newPos;
        if (facingDir.sqrMagnitude > 1e-6f)
        {
            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + spriteAngleOffset);
        }
    }



    private Transform FindTargetByTag(string tagName)
    {
        GameObject go = GameObject.FindGameObjectWithTag(tagName);
        return go != null ? go.transform : null;
    }


}
