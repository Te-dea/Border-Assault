using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement movement;

    // 下落判定阈值（可根据实际动画调整，负值表示向下运动）
    [Tooltip("垂直速度小于该值时触发下落动画")]
    public float fallThreshold = -0.5f;

    public void SetBool(string paramName, bool value)
    {
        anim.SetBool(paramName, value);
    }

    public void SetTrigger(string paramName)
    {
        anim.SetTrigger(paramName);
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // 基础动画参数更新
        anim.SetFloat("Speed", Mathf.Abs(movement.Rb.velocity.x));
        anim.SetBool("IsGrounded", movement.IsGrounded);
        anim.SetBool("IsDashing", movement.IsDashing);

        // 下落动画逻辑（核心新增）
        UpdateFallAnimation();

        // 跳跃动画触发
        if (movement.RemainingJumps < 2 && !movement.IsGrounded && movement.VerticalVelocity > 0)
        {
            anim.SetTrigger("Jump");
        }
    }

    /// <summary>
    /// 更新下落动画状态（当角色在空中且向下运动时触发）
    /// </summary>
    private void UpdateFallAnimation()
    {
        bool isFalling = !movement.IsGrounded && movement.VerticalVelocity < fallThreshold;
        anim.SetBool("IsFalling", isFalling);

        // 从下落状态落地时，触发落地动画（如果素材包有落地帧）
        if (movement.IsGrounded && movement.VerticalVelocity <= fallThreshold)
        {
            anim.SetTrigger("Land");
        }
    }

    // 外部调用的动画触发方法（保持不变）
    public void SetJumpTrigger() => anim.SetTrigger("Jump");
    public void SetAttackTrigger() => anim.SetTrigger("Attack");
    public void SetInteger(string paramName, int value) => anim.SetInteger(paramName, value);
}