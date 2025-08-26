using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public CharacterStatManager stat;

    private void Awake()
    {
        stat = new CharacterStatManager();
    }

    private void Update()
    {

    }
}
