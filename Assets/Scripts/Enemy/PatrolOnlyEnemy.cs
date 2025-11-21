using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 仅巡逻不追踪的敌人
public class PatrolOnlyEnemy : EnemyBase
{
    private bool movingToEnd = true; // 是否向终点移动

    protected override void ExecuteBehavior()
    {
        Patrol();
    }

    // 巡逻逻辑
    private void Patrol()
    {
        Vector2 targetPosition = movingToEnd ? patrolEndPoint : patrolStartPoint;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        // 移动到目标点
        rb.velocity = direction * patrolSpeed;
        animator?.SetBool("Run", true);

        // 翻转朝向
        float newScaleX = Mathf.Sign(direction.x) * initialScale.x;
        transform.localScale = new Vector3(newScaleX, initialScale.y, 1f);

        // 到达目标点则反向
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            movingToEnd = !movingToEnd;
        }
    }

    protected override void Attack()
    {
        // 仅巡逻敌人不攻击
    }
}

// 巡逻+追踪的敌人
public class PatrolAndChaseEnemy : EnemyBase
{
    private bool movingToEnd = true;
    private float returnToPatrolTimer = 0f;
    private float returnToPatrolDelay = 3f; // 丢失目标后多久返回巡逻

    protected override void ExecuteBehavior()
    {
        if (isPlayerInRange)
        {
            // 检测到玩家，开始追踪
            ChasePlayer();
            returnToPatrolTimer = 0;

            // 如果在攻击范围内则攻击
            if (isPlayerInAttackRange)
            {
                Attack();
            }
        }
        else
        {
            // 未检测到玩家，计时返回巡逻
            returnToPatrolTimer += Time.deltaTime;
            if (returnToPatrolTimer >= returnToPatrolDelay)
            {
                Patrol();
            }
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
        animator?.SetBool("Run", true);

        // 翻转朝向
        float newScaleX = Mathf.Sign(direction.x) * initialScale.x;
        transform.localScale = new Vector3(newScaleX, initialScale.y, 1f);
    }

    private void Patrol()
    {
        Vector2 targetPosition = movingToEnd ? patrolEndPoint : patrolStartPoint;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        rb.velocity = direction * patrolSpeed;
        animator?.SetBool("Run", true);

        // 翻转朝向
        float newScaleX = Mathf.Sign(direction.x) * initialScale.x;
        transform.localScale = new Vector3(newScaleX, initialScale.y, 1f);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            movingToEnd = !movingToEnd;
        }
    }

    protected override void Attack()
    {
        animator?.SetTrigger("Attack");
        // 攻击逻辑可以在这里添加，如伤害判定
    }
}

// 静止远程敌人
public class StationaryRangedEnemy : EnemyBase
{
    public GameObject projectilePrefab; // 投射物预制体
    public Transform firePoint;         // 发射点
    public float attackCooldown = 2f;   // 攻击冷却
    private float cooldownTimer = 0f;

    protected override void ExecuteBehavior()
    {
        cooldownTimer += Time.deltaTime;

        // 面向玩家
        if (player != null && isPlayerInRange)
        {
            float newScaleX = player.position.x > transform.position.x ?
                initialScale.x : -initialScale.x;
            transform.localScale = new Vector3(newScaleX, initialScale.y, 1f);
        }

        // 在攻击范围内且冷却结束则攻击
        if (isPlayerInAttackRange && cooldownTimer >= attackCooldown)
        {
            Attack();
            cooldownTimer = 0;
        }
        else
        {
            animator?.SetBool("Run", false);
        }
    }

    protected override void Attack()
    {
        if (projectilePrefab == null || firePoint == null) return;

        animator?.SetTrigger("Attack");
        // 实例化投射物
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        // 设置投射物方向
        float direction = transform.localScale.x > 0 ? 1 : -1;
        projectile.GetComponent<EnemyProjectile>().SetDirection(direction);
    }
}