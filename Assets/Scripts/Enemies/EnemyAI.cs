using UnityEngine;
using System.Collections;

/// <summary>Controls enemy chasing, flanking, melee attacks, and fallback health behavior.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;
    public float moveSpeed = 3f;
    [Header("Combat")]
    [Tooltip("Radius of the melee damage sector.")]
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
    [Header("Settings")]
    [Tooltip("Allows the enemy to chase the player after its room battle starts.")]
    public bool canChase = false;
    [Header("Chase Tuning")]
    [Tooltip("Distance where the enemy stops moving and prepares to attack.")]
    public float stopDistance = 1.2f;
    [Tooltip("Horizontal offset used to approach from the side instead of the exact player center.")]
    public float flankOffset = 1.0f;
    [Tooltip("Radius used when picking small random chase offsets.")]
    public float wanderRadius = 2.0f;
    [Tooltip("Time between random chase-offset refreshes.")]
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

    // Finds the player and records weapon pivot defaults before melee movement begins.
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

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    // Chooses between chasing, idling, and attacking during the physics step.
    private void FixedUpdate()
    {
        if (!canChase || playerTransform == null || rb == null)
        {
            // Enemies stay idle until the room controller explicitly enables chasing.
            isInAttackRange = false;
            UpdateRunAnimation(false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        bool touchingOrTooCloseByCollider = false;
        if (enemyCollider != null && playerCollider != null)
        {
            // Collider distance catches contact cases where center-to-center range is misleading.
            ColliderDistance2D colliderDistance = enemyCollider.Distance(playerCollider);
            touchingOrTooCloseByCollider = colliderDistance.isOverlapped || colliderDistance.distance <= 0.05f;
        }

        isInAttackRange = distanceToPlayer <= attackRange || touchingOrTooCloseByCollider;

        if (isInAttackRange)
        {
            // Stop moving before attacking so melee swings do not slide through the player.
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
            // Random offsets keep groups of melee enemies from stacking on the same target point.
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

    // Handles ongoing physical contact with another collider.
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

        if (isInAttackRange || veryCloseToPlayer)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // Flips the enemy visuals toward movement or target direction.
    private void UpdateFacing(Vector2 direction)
    {
        if (spriteRenderer == null)
        {
            return;
        }

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

    // Keeps the enemy weapon pivot aligned with the current facing direction.
    private void UpdateWeaponFacing(float dirX)
    {
        if (weaponPivot == null)
        {
            if (!hasLoggedMissingWeaponPivot)
            {
                Debug.LogWarning($"[EnemyAI] {name} is missing a required attack reference.", this);
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

    // Updates the enemy running animation parameter.
    private void UpdateRunAnimation(bool isRunning)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isRunning", isRunning);
    }

    // Applies incoming damage and related hit feedback.
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
        Debug.Log(gameObject.name + " took damage. Remaining health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Starts the death flow for this object.
    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        Destroy(gameObject);
    }

    // Offsets the target point to make enemies flank around the player.
    private Vector3 CalculateSmartTargetPosition()
    {
        Vector3 playerPos = playerTransform.position;
        float xOffset = transform.position.x < playerPos.x ? -flankOffset : flankOffset;
        return new Vector3(playerPos.x + xOffset, playerPos.y, playerPos.z);
    }

    // Runs the enemy melee attack wind-up, hit frame, and recovery animation.
    private IEnumerator AttackRoutine()
    {
        if (weaponPivot == null)
        {
            if (!hasLoggedMissingWeaponPivot)
            {
                Debug.LogWarning($"[EnemyAI] {name} is missing a required attack reference.", this);
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

        // The swing is split into windup, hit frame, and recovery for readable tuning.
        yield return LerpWeaponRotation(startRot, windupRot, Mathf.Max(0.0001f, attackWindupDuration), false);
        yield return LerpWeaponRotation(windupRot, swingRot, Mathf.Max(0.0001f, attackSwingDuration), true);
        yield return LerpWeaponRotation(swingRot, startRot, Mathf.Max(0.0001f, attackRecoverDuration), false);

        weaponPivot.localRotation = startRot;
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // Interpolates the enemy weapon swing and optionally triggers the hit frame.
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
                // Damage is applied once at the configured point inside the swing animation.
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

    // Checks the attack circle and applies damage to any player hit inside it.
    private void DoHitCheck()
    {
        if (hitPoint == null)
        {
            Debug.LogWarning($"[EnemyAI] {name} is missing a required attack reference.", this);
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
                Debug.LogWarning($"[EnemyAI] Hit object {col.name} has no PlayerStats, so damage was skipped.");
            }
        }
    }

    // Draws the enemy melee hit radius in the Scene view.
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
