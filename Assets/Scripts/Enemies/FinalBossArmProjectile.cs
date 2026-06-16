using UnityEngine;

/// <summary>
/// Controls one boss arm projectile after it is launched. It moves forward, damages the player
/// on hit, spawns optional impact effects, and returns itself to the launcher pool.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FinalBossArmProjectile : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;

    private Vector2 _moveDirection = Vector2.right;
    private float _speed;
    private float _damage;
    private float _maxLifetime;
    private float _lifeTimer;
    private bool _isActive;
    private LayerMask _hitMask;
    private FinalBossArmLauncher _owner;
    private Transform _ownerTransform;

    private Collider2D _projectileCollider;

    // Caches physics and warning visuals before the arm projectile starts moving.
    private void Awake()
    {
        _projectileCollider = GetComponent<Collider2D>();
        if (_projectileCollider != null)
        {
            _projectileCollider.isTrigger = true;
        }
    }

    // Resets projectile lifetime and collision state whenever the arm is spawned.
    private void OnEnable()
    {
        _lifeTimer = 0f;
    }

    // Runs physics-timed movement and collision-sensitive behavior.
    private void FixedUpdate()
    {
        if (!_isActive)
        {
            return;
        }

        _lifeTimer += Time.fixedDeltaTime;
        if (_lifeTimer >= _maxLifetime)
        {
            ReturnToPool();
            return;
        }

        transform.position += (Vector3)(_moveDirection * (_speed * Time.fixedDeltaTime));
    }

    // Activates the pooled projectile with direction, owner, and damage settings.
    public void Activate(
        FinalBossArmLauncher owner,
        Vector2 direction,
        float speed,
        float damage,
        float maxLifetime,
        LayerMask hitMask)
    {
        _owner = owner;
        _ownerTransform = owner != null ? owner.transform : null;
        _moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        _speed = Mathf.Max(0f, speed);
        _damage = Mathf.Max(0f, damage);
        _maxLifetime = Mathf.Max(0.05f, maxLifetime);
        _hitMask = hitMask;
        _lifeTimer = 0f;
        _isActive = true;

        if (_projectileCollider != null)
        {
            _projectileCollider.enabled = true;
        }
    }

    // Damages the player when the boss arm finishes warning and collides.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isActive || other == null)
        {
            return;
        }

        if (_ownerTransform != null &&
            (other.transform == _ownerTransform || other.transform.IsChildOf(_ownerTransform)))
        {
            return;
        }

        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = other.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            if ((_hitMask.value & (1 << other.gameObject.layer)) != 0)
            {
                playerStats.TakeDamage(_damage);
                SpawnImpactEffect();
                ReturnToPool();
            }
            return;
        }

        if (IsObstacleCollider(other))
        {
            SpawnImpactEffect();
            ReturnToPool();
        }
    }

    // Disables this pooled projectile and notifies its owner pool.
    public void ReturnToPool()
    {
        _isActive = false;
        if (_projectileCollider != null)
        {
            _projectileCollider.enabled = false;
        }

        if (_owner != null)
        {
            _owner.ReleaseProjectile(this);
            return;
        }

        gameObject.SetActive(false);
    }

    // Returns whether this collider should stop the projectile.
    private bool IsObstacleCollider(Collider2D col)
    {
        if (col == null || !col.enabled || col.isTrigger)
        {
            return false;
        }

        if (col.CompareTag("Player"))
        {
            return false;
        }

        if (col.CompareTag("Enemy"))
        {
            return false;
        }

        if (col.GetComponent<FinalBossController>() != null || col.GetComponentInParent<FinalBossController>() != null)
        {
            return false;
        }

        if (col.GetComponent<EnemyAI>() != null || col.GetComponentInParent<EnemyAI>() != null)
        {
            return false;
        }

        if (col.GetComponent<MonsterAI>() != null || col.GetComponentInParent<MonsterAI>() != null)
        {
            return false;
        }

        if (col.GetComponent<EnemyHealth>() != null || col.GetComponentInParent<EnemyHealth>() != null)
        {
            return false;
        }

        return true;
    }

    // Spawns the projectile impact effect at the hit position.
    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null)
        {
            return;
        }

        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }
}
