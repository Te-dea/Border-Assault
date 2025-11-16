using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家攻击系统（纯碰撞体实现，无AttackPoint）
/// 核心：通过动画事件激活/禁用不同的Polygon Collider 2D
/// </summary>
[RequireComponent(typeof(PlayerMovement), typeof(PlayerAnimator), typeof(PlayerInputHandler))]
public class PlayerAttack : MonoBehaviour
{
    #region 攻击配置
    [Header("三段攻击碰撞体")]
    [Tooltip("第一段攻击的碰撞体（需禁用初始状态）")]
    public PolygonCollider2D attackCollider1;
    [Tooltip("第二段攻击的碰撞体（需禁用初始状态）")]
    public PolygonCollider2D attackCollider2;
    [Tooltip("第三段攻击的碰撞体（需禁用初始状态）")]
    public PolygonCollider2D attackCollider3;

    [Header("攻击参数")]
    [Tooltip("攻击冷却时间（秒）")]
    public float attackCooldown = 0.3f;
    [Tooltip("攻击时的位移力度")]
    public float attackMoveForce = 0.8f;
    [Tooltip("碰撞体激活时长（秒，控制判定持续时间）")]
    public float colliderActiveTime = 0.1f;
    [Tooltip("敌人所在图层")]
    public LayerMask enemyLayer;
    [Tooltip("三段攻击的伤害值")]
    public int[] comboDamages = new int[] { 10, 12, 15 }; // 长度必须为3
    #endregion

    #region 组件与状态
    private PlayerMovement movement;
    private PlayerAnimator anim;
    private PlayerInputHandler input;

    private bool isAttacking;
    private float cooldownTimer;
    private int comboCount; // 1-3
    private float comboIntervalTimer;
    private bool needApplyAttackForce;
    private float attackForceX;

    [Tooltip("连击最大间隔时间（秒）")]
    public float maxComboInterval = 0.8f;

    // 缓存当前激活的碰撞体（用于延迟禁用）
    private PolygonCollider2D currentActiveCollider;
    #endregion

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<PlayerAnimator>();
        input = GetComponent<PlayerInputHandler>();

        // 初始化碰撞体状态（确保默认禁用）
        DisableAllColliders();
    }

    private void Update()
    {
        UpdateCooldown();
        UpdateComboInterval();
        CheckAttackInput();
    }

    #region 攻击输入与状态管理
    private void UpdateCooldown()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
        else if (isAttacking)
        {
            isAttacking = false;
            anim.SetBool("IsAttacking", false);
        }
    }

    private void UpdateComboInterval()
    {
        if (comboCount > 0)
        {
            comboIntervalTimer += Time.deltaTime;
            if (comboIntervalTimer >= maxComboInterval)
                ResetCombo();
        }
    }

    private void CheckAttackInput()
    {
        if (input.IsAttackPressed && !isAttacking && cooldownTimer <= 0 && !movement.IsDashing)
        {
            PerformAttack();
            input.ResetOneShotInputs();
        }
    }

    private void PerformAttack()
    {
        comboCount = Mathf.Min(comboCount + 1, 3);
        comboIntervalTimer = 0;
        Debug.Log("当前连击数：" + comboCount); // 调试用

        isAttacking = true;
        cooldownTimer = attackCooldown;
        anim.SetBool("IsAttacking", true);
        anim.SetInteger("ComboCount", comboCount);
        anim.SetTrigger("Attack");

        // 攻击位移（根据角色朝向）
        float faceDir = transform.localScale.x;
        movement.Rb.AddForce(new Vector2(faceDir * attackMoveForce, 0), ForceMode2D.Impulse);
        attackForceX = faceDir * attackMoveForce;
        needApplyAttackForce = true;

    }

    private void FixedUpdate()
    {
        if (needApplyAttackForce)
        {
            movement.Rb.AddForce(new Vector2(attackForceX, 0), ForceMode2D.Impulse);
            needApplyAttackForce = false;
        }
    }

    private void ResetCombo()
    {
        comboCount = 0;
        anim.SetInteger("ComboCount", 0);
        anim.SetTrigger("ResetCombo");
        Debug.Log("连击重置"); // 调试用
    }
    #endregion

    #region 碰撞体控制与攻击判定（核心）
    /// <summary>
    /// 激活当前连击对应的碰撞体（由动画事件调用）
    /// 注意：需在每段攻击的"命中帧"添加此事件，参数为当前连击数（1/2/3）
    /// </summary>
    public void ActivateAttackCollider(int comboIndex)
    {
        // 先禁用所有碰撞体，避免重复检测
        DisableAllColliders();

        // 根据连击数激活对应碰撞体
        currentActiveCollider = comboIndex switch
        {
            1 => attackCollider1,
            2 => attackCollider2,
            3 => attackCollider3,
            _ => null
        };

        if (currentActiveCollider != null)
        {
            currentActiveCollider.enabled = true;
            // 延迟禁用碰撞体（控制判定持续时间）
            Invoke(nameof(DisableCurrentCollider), colliderActiveTime);
            // 立即检测一次碰撞（避免延迟）
            DetectEnemies(comboIndex);
        }
    }

    /// <summary>
    /// 检测碰撞体内的敌人并造成伤害
    /// </summary>
    private void DetectEnemies(int comboIndex)
    {
        if (currentActiveCollider == null) return;

        // 正确初始化ContactFilter2D
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);

        // 检测碰撞体范围内的敌人
        Collider2D[] results = new Collider2D[5];
        int hitCount = Physics2D.OverlapCollider(currentActiveCollider, filter, results);

        // 对每个敌人造成伤害
        for (int i = 0; i < hitCount; i++)
        {
            if (results[i].TryGetComponent<IDamageable>(out IDamageable enemy))
            {
                enemy.TakeDamage(comboDamages[comboIndex - 1]); // 取对应段伤害
            }
        }
    }

    /// <summary>
    /// 禁用当前激活的碰撞体
    /// </summary>
    private void DisableCurrentCollider()
    {
        if (currentActiveCollider != null)
            currentActiveCollider.enabled = false;
    }

    /// <summary>
    /// 禁用所有攻击碰撞体
    /// </summary>
    private void DisableAllColliders()
    {
        if (attackCollider1 != null) attackCollider1.enabled = false;
        if (attackCollider2 != null) attackCollider2.enabled = false;
        if (attackCollider3 != null) attackCollider3.enabled = false;
    }
    #endregion

    #region 动画事件与外部接口
    /// <summary>
    /// 攻击动画结束时调用（由动画事件触发）
    /// </summary>
    public void OnAttackEnd()
    {
        if (comboCount >= 3)
            ResetCombo();
    }

    public bool IsAttacking => isAttacking;

    // 调试：在Scene窗口显示碰撞体范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackCollider1 != null && attackCollider1.enabled)
            DrawPolygonGizmos(attackCollider1);
        if (attackCollider2 != null && attackCollider2.enabled)
            DrawPolygonGizmos(attackCollider2);
        if (attackCollider3 != null && attackCollider3.enabled)
            DrawPolygonGizmos(attackCollider3);
    }

    // 绘制多边形碰撞体的Gizmos（调试用）
    private void DrawPolygonGizmos(PolygonCollider2D collider)
    {
        Vector2[] points = collider.points;
        for (int i = 0; i < points.Length; i++)
        {
            int next = (i + 1) % points.Length;
            Gizmos.DrawLine(
                collider.transform.TransformPoint(points[i]),
                collider.transform.TransformPoint(points[next])
            );
        }
    }
    #endregion
}