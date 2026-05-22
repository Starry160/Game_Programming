using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;
    public float moveSpeed = 3f;
    [Header("AI状态设置")]
    [Tooltip("是否允许开始追踪玩家")]
    public bool canChase = false;

    private Transform playerTransform;
    private Rigidbody2D rb;

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!canChase || playerTransform == null || rb == null)
        {
            return;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 受到伤害，剩余血量: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " 已死亡！");
        Destroy(gameObject);
    }
}
