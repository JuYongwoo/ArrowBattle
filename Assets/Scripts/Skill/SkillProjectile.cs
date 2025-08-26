using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [SerializeField] private float spriteAngleOffset = 45f;
    [SerializeField] private float speed = 5f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 controlPos;   // ������ ������
    private string attackerTag;
    private Skills skill;

    private float t;              // 0~1 ���൵

    public void SetProjectile(Vector3 targetPos, string attackerTag, Skills skill)
    {
        this.startPos = transform.position;
        this.targetPos = targetPos;
        this.attackerTag = attackerTag;
        this.skill = skill;

        // ������ = �������� ��ǥ���� �߰� + ���� offset
        Vector3 mid = (startPos + targetPos) * 0.5f;
        controlPos = new Vector3(mid.x, mid.y + 5f, mid.z);

        // ���� ���
        ManagerObject.audioM.PlayAudioClip(
            ManagerObject.skillInfoM.attackSkillData[skill].skillSound
        );
    }

    private void Update()
    {
        if (targetPos == Vector3.zero) return;

        // t ���� (�ӵ� ���)
        float distance = Vector3.Distance(startPos, targetPos);
        if (distance < 0.01f) { Destroy(gameObject); return; }

        t += (speed / distance) * Time.deltaTime;
        t = Mathf.Clamp01(t);

        // Quadratic Bezier � ��ġ ���
        Vector3 a = Vector3.Lerp(startPos, controlPos, t);
        Vector3 b = Vector3.Lerp(controlPos, targetPos, t);
        Vector3 pos = Vector3.Lerp(a, b, t);

        // �̵�
        Vector3 moveDir = (pos - transform.position).normalized;
        transform.position = pos;

        // ���� �������� ȸ�� + ��������Ʈ ����
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + spriteAngleOffset);
        }

        // ��ǥ �����ϸ� �ı�
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(attackerTag)) return;

        CharacterStatBase stat = other.GetComponent<CharacterStatBase>();
        if (stat != null)
        {
            stat.getDamaged(0);
            Destroy(gameObject);
        }
    }
}
