using Unity.Burst;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    // SO ������ ��ü ����

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer ĳ��
        moveSpeed = GetComponent<Player>().stat.Current.CurrentMoveSpeed; // Player ��ũ��Ʈ���� �̵� �ӵ� ��������
        ManagerObject.inputM.leftRightMove += Move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
    }

    private void Move(float moveX)
    {
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // �¿� �ø�
        if (moveX > 0.01f)        // ������ �̵�
            sr.flipX = false;
        else if (moveX < -0.01f)  // ���� �̵�
            sr.flipX = true;
    }
}
