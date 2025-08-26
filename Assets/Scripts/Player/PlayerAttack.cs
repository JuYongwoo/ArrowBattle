using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{

    private void Awake()
    {
        ManagerObject.inputM.useSkill = Attack; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
    }

    private void Attack(Skills skill)
    {
        //��ų�� ����ü ������ ��ȯ
        GameObject projectile = Instantiate(ManagerObject.skillInfoM.attackSkillData[skill].skillProjectile, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), Quaternion.identity);
        projectile.GetComponent<SkillProjectile>().SetProjectile(GameObject.FindGameObjectWithTag("Enemy").transform.position, gameObject.tag, skill);
    }
}
