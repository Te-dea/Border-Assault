using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public int health = 30;
    public int damage = 10;
    public float speed = 2f;

    public float detectionRange = 5f;
    public LayerMask playerLayer;

    private Transform player;
    private Rigidbody2D rb;
    private bool isDead;
    private Vector3 initialScale; // 存储初始缩放比例

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerMovement>().transform;

        // 缓存初始缩放比例（运行时不再被锁死）
        initialScale = transform.localScale;
    }

    private void Update()
    {
        if (isDead) return;
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * speed;

            // 修正角色翻转逻辑：保留初始Y轴缩放，仅翻转X轴
            float newScaleX = Mathf.Sign(direction.x) * initialScale.x; // 基于初始X缩放翻转
            transform.localScale = new Vector3(newScaleX, initialScale.y, 1f);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 0.5f);
    }

    // 修正：通过FindObjectOfType获取PlayerAttack实例
    private void OnHit()
    {
        PlayerAttack playerAttack = FindObjectOfType<PlayerAttack>();
        if (playerAttack != null)
        {
            Vector2 knockback = (transform.position - playerAttack.transform.position).normalized * 5f;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

public interface IDamageable
{
    void TakeDamage(int damage);
}