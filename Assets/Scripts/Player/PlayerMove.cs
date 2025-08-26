using Unity.Burst;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    // SO 값으로 교체 예정

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱
        moveSpeed = GetComponent<Player>().stat.Current.CurrentMoveSpeed; // Player 스크립트에서 이동 속도 가져오기
        ManagerObject.inputM.leftRightMove += Move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
    }

    private void Move(float moveX)
    {
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // 좌우 플립
        if (moveX > 0.01f)        // 오른쪽 이동
            sr.flipX = false;
        else if (moveX < -0.01f)  // 왼쪽 이동
            sr.flipX = true;
    }
}
