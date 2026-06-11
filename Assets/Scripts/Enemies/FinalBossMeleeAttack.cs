using UnityEngine;

/// <summary>
/// Checks the boss melee sector and applies damage to the player.
/// </summary>
[DisallowMultipleComponent]
public class FinalBossMeleeAttack : MonoBehaviour
{
    [Header("Melee Timing")]
    [SerializeField] private float attackAnimDuration = 0.75f;
    [SerializeField] private float hitDelay = 0.5f;
    [SerializeField] private float hitActiveDuration = 0.12f;

    [Header("Melee Shape")]
    [SerializeField] private float meleeRange = 1.35f;
    [SerializeField, Range(5f, 180f)] private float meleeHalfAngle = 45f;
    [SerializeField] private float meleeDamage = 1f;

    [Header("Optional Anchor")]
    [SerializeField] private Transform meleeOrigin;

    private SpriteRenderer _spriteRenderer;

    public float AttackAnimDuration => Mathf.Max(0.05f, attackAnimDuration);
    public float HitDelay => Mathf.Clamp(hitDelay, 0f, AttackAnimDuration);
    public float HitActiveDuration => Mathf.Max(0.02f, hitActiveDuration);

    // Stores attack references and builds the boss melee hit mask.
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (meleeOrigin == null)
        {
            meleeOrigin = transform;
        }
    }

    // Checks whether the boss melee attack can damage this target.
    public bool CanHitTarget(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        return IsTargetInSector(target);
    }

    // Applies melee damage only when the target is inside the boss attack sector.
    public bool TryApplyDamageToTarget(Transform target)
    {
        if (target == null || !IsTargetInSector(target))
        {
            return false;
        }

        PlayerStats playerStats = target.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = target.GetComponentInParent<PlayerStats>();
        }

        if (playerStats == null)
        {
            return false;
        }

        playerStats.TakeDamage(Mathf.Max(0f, meleeDamage));
        return true;
    }

    private bool IsTargetInSector(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        if (IsPointInSector(target.position))
        {
            return true;
        }

        Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];
            if (col == null || !col.enabled || col.isTrigger || !col.gameObject.activeInHierarchy)
            {
                continue;
            }

            Bounds bounds = col.bounds;
            if (IsPointInSector(bounds.center) ||
                IsPointInSector(new Vector3(bounds.min.x, bounds.min.y, bounds.center.z)) ||
                IsPointInSector(new Vector3(bounds.min.x, bounds.max.y, bounds.center.z)) ||
                IsPointInSector(new Vector3(bounds.max.x, bounds.min.y, bounds.center.z)) ||
                IsPointInSector(new Vector3(bounds.max.x, bounds.max.y, bounds.center.z)))
            {
                return true;
            }
        }

        return false;
    }

    // Checks whether a target lies inside the melee sector.
    private bool IsPointInSector(Vector3 targetPosition)
    {
        Vector2 origin = meleeOrigin != null ? meleeOrigin.position : transform.position;
        Vector2 toTarget = (Vector2)(targetPosition - (Vector3)origin);
        float maxRange = Mathf.Max(0.1f, meleeRange);
        if (toTarget.sqrMagnitude > maxRange * maxRange)
        {
            return false;
        }

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return true;
        }

        Vector2 forward = GetForward();
        float angle = Vector2.Angle(forward, toTarget.normalized);
        return angle <= Mathf.Clamp(meleeHalfAngle, 1f, 180f);
    }

    // Returns the boss melee forward direction from facing scale.
    private Vector2 GetForward()
    {
        float facingSign = (_spriteRenderer != null && _spriteRenderer.flipX) ? -1f : 1f;
        return ((Vector2)transform.right * facingSign).normalized;
    }

    // Draws the boss melee hit circle in the Scene view.
    private void OnDrawGizmosSelected()
    {
        Vector3 center = meleeOrigin != null ? meleeOrigin.position : transform.position;
        float range = Mathf.Max(0.1f, meleeRange);
        float half = Mathf.Clamp(meleeHalfAngle, 1f, 180f);

        SpriteRenderer sr = _spriteRenderer != null ? _spriteRenderer : GetComponent<SpriteRenderer>();
        Vector2 forward = (sr != null && sr.flipX) ? -(Vector2)transform.right : (Vector2)transform.right;
        forward = forward.normalized;

        Quaternion leftRot = Quaternion.Euler(0f, 0f, half);
        Quaternion rightRot = Quaternion.Euler(0f, 0f, -half);
        Vector3 leftDir = leftRot * (Vector3)forward;
        Vector3 rightDir = rightRot * (Vector3)forward;

        Gizmos.color = new Color(1f, 0.55f, 0.2f, 0.9f);
        Gizmos.DrawLine(center, center + leftDir * range);
        Gizmos.DrawLine(center, center + rightDir * range);

        const int segments = 24;
        Vector3 prev = center + rightDir * range;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float a = Mathf.Lerp(-half, half, t);
            Vector3 dir = Quaternion.Euler(0f, 0f, a) * (Vector3)forward;
            Vector3 next = center + dir.normalized * range;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
