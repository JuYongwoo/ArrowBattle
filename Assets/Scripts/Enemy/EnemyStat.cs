using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStat : MonoBehaviour
{
    public CharacterStatManager stat;

    private void Awake()
    {
        stat = new CharacterStatManager("EnemyData");
    }

    private void Update()
    {

    }
}
