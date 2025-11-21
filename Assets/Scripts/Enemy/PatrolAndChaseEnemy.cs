using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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