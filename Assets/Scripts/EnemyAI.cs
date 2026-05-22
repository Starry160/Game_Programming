using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;
    public float moveSpeed = 3f;
    [Header("Combat")]
    [Tooltip("进入该距离后停止追击，视为贴身攻击范围。")]
    public float attackRange = 1.1f;
    [Header("AI状态设置")]
    [Tooltip("是否允许开始追踪玩家")]
    public bool canChase = false;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    private Collider2D playerCollider;
    private bool isInAttackRange;

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerCollider = player.GetComponent<Collider2D>();
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();

        // 连续碰撞检测可降低高速/高频接触时的穿透与抖动问题。
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void FixedUpdate()
    {
        if (!canChase || playerTransform == null || rb == null)
        {
            isInAttackRange = false;
            UpdateRunAnimation(false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        bool touchingOrTooCloseByCollider = false;
        if (enemyCollider != null && playerCollider != null)
        {
            ColliderDistance2D colliderDistance = enemyCollider.Distance(playerCollider);
            touchingOrTooCloseByCollider = colliderDistance.isOverlapped || colliderDistance.distance <= 0.05f;
        }

        isInAttackRange = distanceToPlayer <= attackRange || touchingOrTooCloseByCollider;

        if (isInAttackRange)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            UpdateRunAnimation(false);
            return;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        UpdateFacing(direction);
        UpdateRunAnimation(direction.sqrMagnitude > 0.0001f);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (rb == null)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        bool veryCloseToPlayer = false;
        if (playerTransform != null)
        {
            veryCloseToPlayer = Vector2.Distance(rb.position, playerTransform.position) <= attackRange + 0.1f;
        }

        // 如果已经进入攻击状态或距离玩家过近，强制清掉推力，避免把玩家推着走。
        if (isInAttackRange || veryCloseToPlayer)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void UpdateFacing(Vector2 direction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        // 水平朝向翻转：向左移动时翻转，向右移动时恢复。
        if (direction.x < -0.001f)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x > 0.001f)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void UpdateRunAnimation(bool isRunning)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isRunning", isRunning);
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
