using UnityEngine;

/// <summary>
/// Final Boss 手臂飞行物：激活后沿给定方向飞行，命中玩家/阻挡层后回收到对象池。
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

    private void Awake()
    {
        _projectileCollider = GetComponent<Collider2D>();
        if (_projectileCollider != null)
        {
            _projectileCollider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        _lifeTimer = 0f;
    }

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

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null)
        {
            return;
        }

        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }
}
