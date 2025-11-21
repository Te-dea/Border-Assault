using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<EnemyBase> enemies = new List<EnemyBase>();
    private EnemyBase currentLeader = null;
    private int requiredLeaderCount = 5; // 触发领袖生成的敌人数量

    // 注册敌人
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            CheckLeaderStatus();
        }
    }

    // 注销敌人
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (enemies.Contains(enemy))
        {
            // 如果注销的是领袖，需要重新选举
            if (currentLeader == enemy)
            {
                currentLeader.isLeader = false;
                currentLeader = null;
            }

            enemies.Remove(enemy);
            CheckLeaderStatus();
        }
    }

    // 检查并更新领袖状态
    private void CheckLeaderStatus()
    {
        // 敌人数量超过阈值且没有领袖时，选举新领袖
        if (enemies.Count >= requiredLeaderCount && currentLeader == null)
        {
            ElectLeader();
        }
        // 敌人数量不足时，移除领袖
        else if (enemies.Count < requiredLeaderCount && currentLeader != null)
        {
            currentLeader.isLeader = false;
            currentLeader = null;
        }
    }

    // 选举领袖
    private void ElectLeader()
    {
        if (enemies.Count == 0) return;

        // 随机选择一个敌人作为领袖
        int randomIndex = Random.Range(0, enemies.Count);
        currentLeader = enemies[randomIndex];
        currentLeader.isLeader = true;

        // 可以在这里添加领袖的特殊效果，例如：
        // currentLeader.speed *= 1.2f;
        // currentLeader.damage *= 1.2f;
    }

    // 获取附近的盟友（可以用于协同攻击）
    public List<EnemyBase> GetAlliesInRange(Vector3 position, float range)
    {
        List<EnemyBase> alliesInRange = new List<EnemyBase>();

        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.isDead &&
                Vector2.Distance(position, enemy.transform.position) <= range)
            {
                alliesInRange.Add(enemy);
            }
        }

        return alliesInRange;
    }
}