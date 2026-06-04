using UnityEngine;

/// <summary>
/// Final Boss 手臂飞行物：激活后沿给定方向飞行，命中玩家/阻挡层后回收到对象池。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FinalBossArmProjectile : MonoBehaviour
{
    private Vector2 _moveDirection = Vector2.right;
    private float _speed;
    private float _damage;
    private float _maxLifetime;
    private float _lifeTimer;
    private bool _isActive;
    private LayerMask _hitMask;
    private FinalBossArmLauncher _owner;

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

        if ((_hitMask.value & (1 << other.gameObject.layer)) == 0)
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
            playerStats.TakeDamage(_damage);
        }

        ReturnToPool();
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
}
