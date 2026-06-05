using UnityEngine;

/// <summary>
/// Final Boss 激光体：跟随发射点，按 tick 对玩家持续造成伤害，持续时间结束后回收。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FinalBossLaserBeam : MonoBehaviour
{
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private bool _syncRotation = true;
    [SerializeField] private bool _alignSpriteLeftEdgeToOrigin = true;
    [Header("Damage Area")]
    [SerializeField] private Vector2 _damageBoxOffset = new Vector2(1.1f, 0f);
    [SerializeField] private Vector2 _damageBoxSize = new Vector2(2.2f, 0.45f);
    [SerializeField] private float _damageStartDelay = 0.1f;
    [Header("Debug")]
    [SerializeField] private bool _showDamageGizmo = true;
    [SerializeField] private Color _damageGizmoColor = new Color(1f, 0.15f, 0.15f, 0.65f);

    private FinalBossLaserLauncher _owner;
    private Transform _origin;
    private float _damagePerTick;
    private float _tickInterval;
    private float _maxDuration;
    private float _elapsed;
    private float _nextTickTime;
    private float _damageEnableTime;
    private bool _active;
    private bool _lockDirectionOnCast;
    private Quaternion _lockedRotation;
    private Vector3 _visualOffsetLocal;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _damageCollider;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _damageCollider = GetComponent<BoxCollider2D>();
        SyncDamageColliderShape();
    }

    private void OnEnable()
    {
        _elapsed = 0f;
        _nextTickTime = 0f;
        _damageEnableTime = 0f;
        SyncDamageColliderShape();
        SetDamageColliderEnabled(false);
    }

    private void LateUpdate()
    {
        if (!_active)
        {
            return;
        }

        if (Time.time >= _damageEnableTime)
        {
            SetDamageColliderEnabled(true);
        }

        _elapsed += Time.deltaTime;
        if (_elapsed >= _maxDuration)
        {
            ReturnToPool();
            return;
        }

        if (_origin != null)
        {
            if (_syncRotation)
            {
                transform.rotation = _lockDirectionOnCast ? _lockedRotation : _origin.rotation;
            }

            transform.position = ComputeAlignedPosition();
        }
    }

    public void Activate(
        FinalBossLaserLauncher owner,
        Transform origin,
        Vector3 visualOffsetLocal,
        float damagePerTick,
        float tickInterval,
        float maxDuration,
        bool lockDirectionOnCast,
        Quaternion castRotation)
    {
        _owner = owner;
        _origin = origin;
        _damagePerTick = Mathf.Max(0f, damagePerTick);
        _tickInterval = Mathf.Max(0.01f, tickInterval);
        _maxDuration = Mathf.Max(0.05f, maxDuration);
        _visualOffsetLocal = visualOffsetLocal;
        _lockDirectionOnCast = lockDirectionOnCast;
        _lockedRotation = castRotation;
        _elapsed = 0f;
        _nextTickTime = 0f;
        _damageEnableTime = Time.time + Mathf.Max(0f, _damageStartDelay);
        _active = true;
        SetDamageColliderEnabled(false);

        if (_origin != null)
        {
            if (_syncRotation)
            {
                transform.rotation = _lockDirectionOnCast ? _lockedRotation : _origin.rotation;
            }

            transform.position = ComputeAlignedPosition();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_active || other == null)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < _nextTickTime)
        {
            return;
        }

        _nextTickTime = Time.time + _tickInterval;

        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = other.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.TakeTrueDamage(_damagePerTick);
        }
    }

    public void ReturnToPool()
    {
        _active = false;
        SetDamageColliderEnabled(false);
        if (_owner != null)
        {
            _owner.ReleaseLaser(this);
            return;
        }

        gameObject.SetActive(false);
    }

    private Vector3 ComputeAlignedPosition()
    {
        Quaternion offsetRotation = ResolveOffsetRotation();
        Vector3 basePosition = _origin.position + (offsetRotation * _visualOffsetLocal);
        if (!_alignSpriteLeftEdgeToOrigin || _spriteRenderer == null || _spriteRenderer.sprite == null)
        {
            return basePosition;
        }

        // Move pivot so sprite left-edge starts exactly at origin.
        float leftEdgeLocalX = _spriteRenderer.sprite.bounds.min.x;
        float worldShift = -leftEdgeLocalX * Mathf.Abs(transform.lossyScale.x);
        return basePosition + transform.right * worldShift;
    }

    private Quaternion ResolveOffsetRotation()
    {
        if (_origin == null)
        {
            return transform.rotation;
        }

        if (_lockDirectionOnCast)
        {
            return _lockedRotation;
        }

        return _origin.rotation;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showDamageGizmo)
        {
            return;
        }

        BoxCollider2D box = GetDamageCollider();
        if (box == null)
        {
            return;
        }

        Gizmos.color = _damageGizmoColor;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.offset, box.size);
        Gizmos.matrix = oldMatrix;
    }

    private void OnValidate()
    {
        _damageBoxSize.x = Mathf.Max(0.01f, _damageBoxSize.x);
        _damageBoxSize.y = Mathf.Max(0.01f, _damageBoxSize.y);
        SyncDamageColliderShape();
    }

    private BoxCollider2D GetDamageCollider()
    {
        if (_damageCollider == null)
        {
            _damageCollider = GetComponent<BoxCollider2D>();
        }

        return _damageCollider;
    }

    private void SyncDamageColliderShape()
    {
        BoxCollider2D box = GetDamageCollider();
        if (box == null)
        {
            return;
        }

        box.isTrigger = true;
        box.offset = GetDirectionalDamageOffset();
        box.size = _damageBoxSize;
    }

    private Vector2 GetDirectionalDamageOffset()
    {
        // z=180° 时局部 Y 轴也会翻转；为保证左右发射在世界空间上下对齐一致，
        // 左向时对 offset.y 做反向补偿。
        if (IsFacingLeft())
        {
            return new Vector2(_damageBoxOffset.x, -_damageBoxOffset.y);
        }

        return _damageBoxOffset;
    }

    private bool IsFacingLeft()
    {
        Vector3 dir;
        if (_lockDirectionOnCast)
        {
            dir = _lockedRotation * Vector3.right;
        }
        else if (_origin != null)
        {
            dir = _origin.rotation * Vector3.right;
        }
        else
        {
            dir = transform.right;
        }

        return dir.x < 0f;
    }

    private void SetDamageColliderEnabled(bool enabled)
    {
        BoxCollider2D box = GetDamageCollider();
        if (box == null)
        {
            return;
        }

        box.enabled = enabled;
    }
}
