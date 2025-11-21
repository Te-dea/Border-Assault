using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人行为类型枚举
public enum EnemyBehaviorType
{
    PatrolOnly,       // 仅巡逻不追踪
    PatrolAndChase,   // 巡逻+检测到玩家时追踪
    StationaryRanged  // 静止远程攻击
}

// 敌人基类，实现IDamageable接口并封装通用逻辑
public abstract class EnemyBase : MonoBehaviour,IDamageable
{
    [Header("基础属性")]
    public int health = 30;
    public int damage = 10;
    public float speed = 2f;
    public EnemyBehaviorType behaviorType;

    [Header("检测范围")]
    public float detectionRange = 5f;       // 玩家检测范围
    public float attackRange = 1.5f;        // 攻击范围
    public LayerMask playerLayer;

    [Header("动画与特效")]
    public Animator animator;
    public GameObject hitEffect;            // 流血特效预制体
    public float flashDuration = 0.1f;      // 闪白持续时间

    [Header("巡逻参数")]
    public Vector2 patrolStartPoint;        // 巡逻起点
    public Vector2 patrolEndPoint;          // 巡逻终点
    public float patrolSpeed = 1f;          // 巡逻速度

    protected Transform player;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    public bool isDead;  // 修改：从protected改为public，让EnemyManager可访问
    protected bool isHit;
    protected Vector3 initialScale;
    protected bool isPlayerInRange;         // 玩家是否在检测范围内
    protected bool isPlayerInAttackRange;   // 玩家是否在攻击范围内

    // 群体行为相关
    [HideInInspector] public bool isLeader = false;
    [HideInInspector] public EnemyManager enemyManager;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyManager = FindObjectOfType<EnemyManager>();

        // 注册到敌人管理器
        if (enemyManager != null)
            enemyManager.RegisterEnemy(this);

        // 尝试获取玩家
        var playerObj = FindObjectOfType<PlayerMovement>();
        if (playerObj != null)
            player = playerObj.transform;

        initialScale = transform.localScale;
        patrolStartPoint = transform.position; // 默认起点为初始位置
    }

    protected virtual void Update()
    {
        if (isDead || isHit) return;

        // 检测玩家是否在范围内
        CheckPlayerInRange();

        // 根据行为类型执行不同逻辑
        ExecuteBehavior();
    }

    // 检测玩家范围
    protected virtual void CheckPlayerInRange()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= detectionRange;
        isPlayerInAttackRange = distanceToPlayer <= attackRange;
    }

    // 执行行为（由子类实现具体逻辑）
    protected abstract void ExecuteBehavior();

    // 受伤处理（实现IDamageable接口）
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        StartCoroutine(HitEffect());

        if (health <= 0)
        {
            Die();
            return;
        }

        // 受击击退
        OnHit();
    }

    // 受击效果
    protected virtual IEnumerator HitEffect()
    {
        isHit = true;
        animator?.SetTrigger("Hit");

        // 实例化流血特效
        if (hitEffect != null)
        {
            var effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // 闪白效果
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;

        isHit = false;
    }

    // 死亡处理
    protected virtual void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        animator?.SetTrigger("Death");
        GetComponent<Collider2D>().enabled = false;

        // 从管理器移除
        if (enemyManager != null)
            enemyManager.UnregisterEnemy(this);

        Destroy(gameObject, 1f); // 延长销毁时间以播放死亡动画
    }

    // 受击击退
    protected virtual void OnHit()
    {
        if (player == null) return;

        Vector2 knockback = (transform.position - player.position).normalized * 5f;
        rb.AddForce(knockback, ForceMode2D.Impulse);
    }

    // 攻击逻辑（由子类实现）
    protected abstract void Attack();

    // 绘制检测范围 gizmos
    protected virtual void OnDrawGizmosSelected()
    {
        // 检测范围（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 攻击范围（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 巡逻路径（蓝色）
        if (behaviorType != EnemyBehaviorType.StationaryRanged)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(patrolStartPoint, patrolEndPoint);
        }
    }
}