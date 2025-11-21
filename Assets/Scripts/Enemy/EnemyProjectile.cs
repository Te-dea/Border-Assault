using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 10;
    public float speed = 5f;
    public float lifetime = 2f;
    private int direction = 1;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    public void SetDirection(int dir)
    {
        direction = dir;
        // 翻转投射物朝向
        if (dir < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 攻击玩家
        if (other.CompareTag("Player"))
        {
            // 这里可以添加玩家受伤逻辑（当前玩家暂无生命系统）
            Destroy(gameObject);
        }
        // 碰到墙壁等障碍物
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}