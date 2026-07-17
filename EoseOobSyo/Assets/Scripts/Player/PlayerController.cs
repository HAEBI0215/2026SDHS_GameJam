using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerType
    {
        Player1,
        Player2
    }

    [Header("플레이어 설정")]
    [SerializeField]
    private PlayerType playerType;

    [Header("이동")]
    [SerializeField]
    private float moveSpeed = 5f;

    [Header("캐릭터 방향 전환")]
    [SerializeField]
    private Transform visualRoot;

    [SerializeField]
    private float turnSpeed = 720f;

    [SerializeField]
    private bool startsFacingRight = true;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 movement;
    private Vector2 lastDirection;

    private float targetYRotation;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    public Vector2 Movement => movement;
    public Vector2 LastDirection => lastDirection;
    public bool IsMoving => movement.sqrMagnitude > 0.01f;
    public bool IsFacingLeft => lastDirection.x < 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if(visualRoot != null)
        {
            animator = visualRoot.GetComponent<Animator>();
        }

        if(startsFacingRight)
        {
            lastDirection = Vector2.right;
            targetYRotation = 0f;
        }
        else
        {
            lastDirection = Vector2.left;
            targetYRotation = 180f;
        }

        if(visualRoot != null)
        {
            visualRoot.localRotation =
                Quaternion.Euler(0f, targetYRotation, 0f);
        }
    }

    private void Update()
    {
        InputMove();
        RotateVisual();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void InputMove()
    {
        movement = Vector2.zero;

        if(playerType == PlayerType.Player1)
        {
            if(Input.GetKey(KeyCode.W))
                movement.y += 1f;

            if(Input.GetKey(KeyCode.S))
                movement.y -= 1f;

            if(Input.GetKey(KeyCode.A))
                movement.x -= 1f;

            if(Input.GetKey(KeyCode.D))
                movement.x += 1f;
        }
        else
        {
            if(Input.GetKey(KeyCode.UpArrow))
                movement.y += 1f;

            if(Input.GetKey(KeyCode.DownArrow))
                movement.y -= 1f;

            if(Input.GetKey(KeyCode.LeftArrow))
                movement.x -= 1f;

            if(Input.GetKey(KeyCode.RightArrow))
                movement.x += 1f;
        }

        movement = movement.normalized;

        UpdateFacingDirection();
    }

    private void UpdateFacingDirection()
    {
        if(movement.x < 0f)
        {
            lastDirection = Vector2.left;
            targetYRotation = 180f;
        }
        else if(movement.x > 0f)
        {
            lastDirection = Vector2.right;
            targetYRotation = 0f;
        }
    }

    private void RotateVisual()
    {
        if(visualRoot == null)
            return;

        float currentY = visualRoot.localEulerAngles.y;

        float newY = Mathf.MoveTowardsAngle(
            currentY,
            targetYRotation,
            turnSpeed * Time.deltaTime
        );

        visualRoot.localRotation =
            Quaternion.Euler(0f, newY, 0f);
    }

    private void UpdateAnimation()
    {
        if(animator == null)
            return;

        animator.SetBool(IsMovingHash, IsMoving);
    }

    private void Move()
    {
        rb.MovePosition(
            rb.position +
            movement * moveSpeed * Time.fixedDeltaTime
        );
    }
}