using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        projectile.GetComponent<EnemyProjectile>().SetDirection((int)direction);
    }
}