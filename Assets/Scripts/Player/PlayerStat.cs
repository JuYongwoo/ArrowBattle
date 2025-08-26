using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStat : MonoBehaviour
{
    public CharacterStatManager stat;

    private void Awake()
    {
        stat = new CharacterStatManager("PlayerData");
    }

}
