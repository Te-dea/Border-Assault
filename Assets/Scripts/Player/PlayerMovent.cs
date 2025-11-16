using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float airMoveMultiplier = 0.8f;

    [Header("跳跃参数")]
    public float jumpForce = 7f;
    public bool enableDoubleJump = true;
    public float doubleJumpMultiplier = 0.8f;
    public float jumpHoldBoost = 1.2f;
    public float maxJumpHoldTime = 0.2f;

    [Header("冲刺参数")]
    public float dashSpeedMultiplier = 2.5f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1f;

    [Header("地面检测")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;
    public Rigidbody2D Rb => rb;
    public int RemainingJumps => remainingJumps;
    public float VerticalVelocity => rb.velocity.y;

    private Rigidbody2D rb;
    private PlayerInputHandler input;
    private bool isGrounded;
    private int remainingJumps;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float jumpHoldTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputHandler>();
        remainingJumps = enableDoubleJump ? 2 : 1;
    }

    private void Update()
    {
        CheckGrounded();
        UpdateDashState();
        UpdateJumpHoldTimer();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        HandleDash();
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            remainingJumps = enableDoubleJump ? 2 : 1;
            jumpHoldTimer = 0;
        }
    }

    private void HandleMovement()
    {
        // 获取攻击组件的状态（是否正在攻击）
        bool isAttacking = GetComponent<PlayerAttack>().IsAttacking;

        // 攻击时：冻结水平移动（保留垂直跳跃）
        if (isAttacking)
        {
            // 仅保留垂直速度，水平速度设为0
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // 冲刺时：按冲刺逻辑移动（不受移动键影响）
        if (isDashing)
        {
            rb.velocity = new Vector2(transform.localScale.x * moveSpeed * dashSpeedMultiplier, 0);
            return;
        }

        // 正常移动逻辑（非攻击/非冲刺状态）
        float moveX = input.MoveInput.x;
        float currentSpeed = isGrounded ? moveSpeed : moveSpeed * airMoveMultiplier;
        rb.velocity = new Vector2(moveX * currentSpeed, rb.velocity.y);

        // 角色翻转（仅在有水平输入时）
        if (moveX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveX), 1f, 1f);
        }
    }

    private void HandleJump()
    {
        if (input.IsJumpPressed && remainingJumps > 0)
        {
            float finalJumpForce = remainingJumps == 2 ? jumpForce : jumpForce * doubleJumpMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * finalJumpForce, ForceMode2D.Impulse);
            remainingJumps--;
            input.ResetOneShotInputs();
        }

        if (input.IsJumpHeld && !isGrounded && rb.velocity.y > 0)
        {
            if (jumpHoldTimer < maxJumpHoldTime)
            {
                rb.AddForce(Vector2.up * jumpForce * jumpHoldBoost * Time.fixedDeltaTime, ForceMode2D.Force);
            }
        }
    }

    private void HandleDash()
    {
        if (input.IsDashPressed && !isDashing && dashCooldownTimer <= 0 && Mathf.Abs(input.MoveInput.x) > 0.1f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            input.ResetOneShotInputs();
        }
    }

    private void UpdateDashState()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            rb.velocity = new Vector2(transform.localScale.x * moveSpeed * dashSpeedMultiplier, 0);

            if (dashTimer <= 0)
            {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
            }
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateJumpHoldTimer()
    {
        jumpHoldTimer = input.IsJumpHeld && !isGrounded && rb.velocity.y > 0
            ? jumpHoldTimer + Time.deltaTime
            : 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}