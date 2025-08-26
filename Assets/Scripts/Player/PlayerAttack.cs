using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{

    private void Awake()
    {
        ManagerObject.inputM.useSkill = Attack; // InputManager의 attack 이벤트에 Attack 메서드 구독
    }

    private void Attack(Skills skill)
    {
        //스킬의 투사체 프리팹 소환
        Instantiate(ManagerObject.skillInfoM.attackSkillData[skill].skillProjectile, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f), Quaternion.identity);
    }
}
