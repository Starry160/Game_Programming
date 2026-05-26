using UnityEngine;
using System.Collections;

/// <summary>敌人 AI：追击、包抄、挥砍攻击与旧版血量（可被 EnemyHealth 替代）。</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;
    public float moveSpeed = 3f;
    [Header("Combat")]
    [Tooltip("进入该距离后停止追击，视为贴身攻击范围。")]
    public float attackRange = 1.1f;
    public float attackCooldown = 1.0f;
    private float lastAttackTime = -999f;
    public int attackDamage = 1;
    public float hitFrameTimeNormalized = 0.45f;
    public LayerMask playerLayer;
    public float hitRadius = 0.8f;
    public Transform hitPoint;
    private bool isAttacking = false;
    public Transform weaponPivot;
    public Transform weaponSprite;
    public float attackStartAngle = 35f;
    public float attackEndAngle = -85f;
    public float attackWindupDuration = 0.06f;
    public float attackSwingDuration = 0.10f;
    public float attackRecoverDuration = 0.10f;
    [Header("AI状态设置")]
    [Tooltip("是否允许开始追踪玩家")]
    public bool canChase = false;
    [Header("Chase Tuning")]
    [Tooltip("敌人希望与智能目标点保持的最近距离。")]
    public float stopDistance = 1.2f;
    [Tooltip("用于包抄玩家左右侧面的横向偏移。")]
    public float flankOffset = 1.0f;
    [Tooltip("随机游走半径，值越大绕行越明显。")]
    public float wanderRadius = 2.0f;
    [Tooltip("随机游走状态切换间隔（秒）。")]
    public float stateChangeInterval = 1.5f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    private Collider2D playerCollider;
    private bool isInAttackRange;
    private Quaternion weaponInitialRotation = Quaternion.identity;
    private Vector3 weaponPivotInitialLocalPosition = Vector3.zero;
    private bool hasLoggedMissingWeaponPivot = false;
    private Vector3 randomOffset;
    private float nextStateChangeTime = 0f;

    // 查找玩家、缓存组件与武器初始姿态。
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
        if (weaponPivot != null)
        {
            weaponInitialRotation = weaponPivot.localRotation;
            weaponPivotInitialLocalPosition = weaponPivot.localPosition;
        }

        // 连续碰撞检测可降低高速/高频接触时的穿透与抖动问题。
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    // 物理帧：追击/待机/发起攻击。
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
            UpdateFacing(new Vector2(playerTransform.position.x - transform.position.x, 0f));

            if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
            return;
        }

        Vector3 targetPosition = CalculateSmartTargetPosition();
        if (Time.time >= nextStateChangeTime)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * wanderRadius;
            float randomDistanceFactor = Random.Range(0.6f, 1.4f);
            randomOffset = new Vector3(randomCircle.x, randomCircle.y, 0f) * randomDistanceFactor;
            nextStateChangeTime = Time.time + stateChangeInterval + Random.Range(-0.3f, 0.3f);
        }

        if (distanceToPlayer > stopDistance + 0.5f)
        {
            targetPosition += randomOffset;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget > stopDistance)
        {
            Vector2 direction = ((Vector2)targetPosition - rb.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            UpdateFacing(direction);
            UpdateRunAnimation(direction.sqrMagnitude > 0.0001f);
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            UpdateFacing(new Vector2(playerTransform.position.x - transform.position.x, 0f));
            UpdateRunAnimation(false);
        }
    }

    // 贴身时清零速度，避免推挤玩家。
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

    // 按移动方向翻转 Sprite 与武器 pivot。
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

        UpdateWeaponFacing(direction.x);
    }

    // 同步武器 pivot 的 scale 与本地 X 偏移。
    private void UpdateWeaponFacing(float dirX)
    {
        if (weaponPivot == null)
        {
            if (!hasLoggedMissingWeaponPivot)
            {
                Debug.LogWarning($"[EnemyAI] {name} 未绑定 weaponPivot，无法同步武器朝向。", this);
                hasLoggedMissingWeaponPivot = true;
            }
            return;
        }

        Vector3 scale = weaponPivot.localScale;
        Vector3 pivotPos = weaponPivot.localPosition;
        float basePivotX = Mathf.Abs(weaponPivotInitialLocalPosition.x);
        if (dirX < -0.001f)
        {
            scale.x = -Mathf.Abs(scale.x);
            pivotPos.x = -basePivotX;
        }
        else if (dirX > 0.001f)
        {
            scale.x = Mathf.Abs(scale.x);
            pivotPos.x = basePivotX;
        }

        weaponPivot.localPosition = pivotPos;
        weaponPivot.localScale = scale;
    }

    // 设置 Animator 的 isRunning 参数。
    private void UpdateRunAnimation(bool isRunning)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isRunning", isRunning);
    }

    // 旧版受击扣血（无 EnemyHealth 时使用）。
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        HitFeedback feedback = GetComponent<HitFeedback>();
        if (feedback != null)
        {
            feedback.PlayFeedback();
        }
        Debug.Log(gameObject.name + " 受到伤害，剩余血量: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // 旧版死亡：直接销毁。
    private void Die()
    {
        Debug.Log(gameObject.name + " 已死亡！");
        Destroy(gameObject);
    }

    // 计算包抄目标点（玩家左右侧面偏移）。
    private Vector3 CalculateSmartTargetPosition()
    {
        Vector3 playerPos = playerTransform.position;
        float xOffset = transform.position.x < playerPos.x ? -flankOffset : flankOffset;
        return new Vector3(playerPos.x + xOffset, playerPos.y, playerPos.z);
    }

    // 挥砍协程：前摇 → 下劈（命中帧）→ 收招。
    private IEnumerator AttackRoutine()
    {
        if (weaponPivot == null)
        {
            if (!hasLoggedMissingWeaponPivot)
            {
                Debug.LogWarning($"[EnemyAI] {name} 未绑定 weaponPivot，无法执行旋转挥砍。", this);
                hasLoggedMissingWeaponPivot = true;
            }
            lastAttackTime = Time.time;
            yield break;
        }

        isAttacking = true;

        float dirSign = weaponPivot.localScale.x >= 0f ? 1f : -1f;
        Quaternion startRot = weaponInitialRotation;
        Quaternion windupRot = Quaternion.Euler(0f, 0f, attackStartAngle * dirSign);
        Quaternion swingRot = Quaternion.Euler(0f, 0f, attackEndAngle * dirSign);

        yield return LerpWeaponRotation(startRot, windupRot, Mathf.Max(0.0001f, attackWindupDuration), false);
        yield return LerpWeaponRotation(windupRot, swingRot, Mathf.Max(0.0001f, attackSwingDuration), true);
        yield return LerpWeaponRotation(swingRot, startRot, Mathf.Max(0.0001f, attackRecoverDuration), false);

        weaponPivot.localRotation = startRot;
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // 武器旋转插值，可选在归一化时间点触发伤害。
    private IEnumerator LerpWeaponRotation(Quaternion from, Quaternion to, float duration, bool applyHitFrame)
    {
        float elapsed = 0f;
        bool hitDone = false;
        float clampedHit = Mathf.Clamp01(hitFrameTimeNormalized);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            weaponPivot.localRotation = Quaternion.Slerp(from, to, t);

            if (applyHitFrame && !hitDone && t >= clampedHit)
            {
                DoHitCheck();
                hitDone = true;
            }

            yield return null;
        }

        weaponPivot.localRotation = to;
        if (applyHitFrame && !hitDone)
        {
            DoHitCheck();
        }
    }

    // 在 hitPoint 圆形范围内对玩家造成伤害。
    private void DoHitCheck()
    {
        if (hitPoint == null)
        {
            Debug.LogWarning($"[EnemyAI] {name} 未绑定 hitPoint，跳过伤害检测。", this);
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPoint.position, hitRadius, playerLayer);
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i];
            if (col == null)
            {
                continue;
            }

            PlayerStats playerStats = col.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = col.GetComponentInParent<PlayerStats>();
            }

            if (playerStats != null)
            {
                const float debugDamage = 1f;
                playerStats.TakeDamage(debugDamage);
                Debug.Log($"[EnemyAI] {name} Hit Player: {col.name}, damage={debugDamage}");
            }
            else
            {
                Debug.LogWarning($"[EnemyAI] 命中对象 {col.name} 未找到 PlayerStats，已跳过伤害调用。");
            }
        }
    }

    // 编辑器绘制攻击判定圆。
    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
    }
}
