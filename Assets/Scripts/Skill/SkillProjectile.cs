using System;
using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [SerializeField]
    private float spriteAngleOffset; //임포트한 스프라이트가 기울어져 있을 수 있으므로 각도 보정값 존재

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 controlPos;   // 포물선 제어점
    private CharacterTypeEnumByTag attackerType;
    private Skills skill;

    private float t;              // 0~1 진행도

    public void SetProjectile(CharacterTypeEnumByTag attackerType, Skills skill)
    {
        this.attackerType = attackerType;
        targetPos =  GameObject.FindGameObjectWithTag(Enum.GetName(typeof(CharacterTypeEnumByTag), ((int)attackerType+1) % Enum.GetValues(typeof(CharacterTypeEnumByTag)).Length)).transform.position;
        this.skill = skill;
        startPos = transform.position;

        // 제어점 = 시작점과 목표점의 중간 + 위로 offset
        Vector3 mid = (startPos + targetPos) * 0.5f;
        controlPos = new Vector3(mid.x, mid.y + 5f, mid.z);

        // 사운드 재생
        ManagerObject.audioM.PlayAudioClip(
            ManagerObject.skillInfoM.attackSkillData[skill].skillSound
        );
    }

    private void Update()
    {
        if (targetPos == Vector3.zero) return;

        // t 증가 (속도 기반)
        float distance = Vector3.Distance(startPos, targetPos);
        if (distance < 0.01f) { Destroy(gameObject); return; }

        t += (ManagerObject.skillInfoM.attackSkillData[skill].projectileSpeed / distance) * Time.deltaTime;
        t = Mathf.Clamp01(t);

        // Quadratic Bezier 곡선 위치 계산
        Vector3 a = Vector3.Lerp(startPos, controlPos, t);
        Vector3 b = Vector3.Lerp(controlPos, targetPos, t);
        Vector3 pos = Vector3.Lerp(a, b, t);

        // 이동
        Vector3 moveDir = (pos - transform.position).normalized;
        transform.position = pos;

        // 진행 방향으로 회전 + 스프라이트 보정
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + spriteAngleOffset);
        }

        // 목표 도착하면 파괴
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Enum.GetName(typeof(CharacterTypeEnumByTag), attackerType))) return;

        CharacterBase stat = other.GetComponent<CharacterBase>();
        if (stat != null)
        {
            stat.getDamaged(ManagerObject.skillInfoM.attackSkillData[skill].skillDamage);
            Destroy(gameObject);
        }
    }
}
