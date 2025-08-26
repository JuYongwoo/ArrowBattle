using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    // SO ������ ��ü ����
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer ĳ��
    }

    private void Update()
    {
        // �¿� �̵�
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // �¿� �ø�
        if (moveX > 0.01f)        // ������ �̵�
            sr.flipX = false;
        else if (moveX < -0.01f)  // ���� �̵�
            sr.flipX = true;
    }
}
