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