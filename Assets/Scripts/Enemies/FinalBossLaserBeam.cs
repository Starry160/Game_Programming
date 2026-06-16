using UnityEngine;

/// <summary>
/// Represents an active boss laser beam. It follows the emitter while active, keeps the visual
/// aligned with the laser direction, applies repeated player damage, and returns to the pool.
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
    [Header("Scene Gizmo")]
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

    // Captures laser renderers, colliders, and default sizes for beam telegraphs.
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _damageCollider = GetComponent<BoxCollider2D>();
        SyncDamageColliderShape();
    }

    // Resets the beam to its inactive visual state when reused.
    private void OnEnable()
    {
        _elapsed = 0f;
        _nextTickTime = 0f;
        _damageEnableTime = 0f;
        SyncDamageColliderShape();
        SetDamageColliderEnabled(false);
    }

    // Updates camera or visual follow logic after normal Update movement.
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

    // Activates the pooled projectile with direction, owner, and damage settings.
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

    // Applies laser tick damage while the player remains inside the damage collider.
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

    // Disables this pooled projectile and notifies its owner pool.
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

    // Calculates the laser position aligned to its emitter and facing.
    private Vector3 ComputeAlignedPosition()
    {
        Quaternion offsetRotation = ResolveOffsetRotation();
        Vector3 originPosition = ResolveOriginPosition();
        Vector3 basePosition = originPosition + (offsetRotation * _visualOffsetLocal);
        if (!_alignSpriteLeftEdgeToOrigin || _spriteRenderer == null || _spriteRenderer.sprite == null)
        {
            return basePosition;
        }

        // Move pivot so sprite left-edge starts exactly at origin.
        float leftEdgeLocalX = _spriteRenderer.sprite.bounds.min.x;
        float worldShift = -leftEdgeLocalX * Mathf.Abs(transform.lossyScale.x);
        return basePosition + transform.right * worldShift;
    }

    // Finds the world-space origin used by the active laser beam.
    private Vector3 ResolveOriginPosition()
    {
        if (_origin == null)
        {
            return transform.position;
        }

        // Keep right-facing as authored position; when facing left, mirror origin local X
        // so the muzzle point stays centered/symmetric around the boss head.
        if (_lockDirectionOnCast && IsFacingLeft() && _origin.parent != null)
        {
            Vector3 mirroredLocal = _origin.localPosition;
            mirroredLocal.x = -mirroredLocal.x;
            return _origin.parent.TransformPoint(mirroredLocal);
        }

        return _origin.position;
    }

    // Calculates the rotation applied to directional laser offsets.
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

    // Draws the laser beam length and aim direction in the Scene view.
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

    // Keeps editor-time references and collider setup consistent.
    private void OnValidate()
    {
        _damageBoxSize.x = Mathf.Max(0.01f, _damageBoxSize.x);
        _damageBoxSize.y = Mathf.Max(0.01f, _damageBoxSize.y);
        SyncDamageColliderShape();
    }

    // Finds the collider that applies laser damage.
    private BoxCollider2D GetDamageCollider()
    {
        if (_damageCollider == null)
        {
            _damageCollider = GetComponent<BoxCollider2D>();
        }

        return _damageCollider;
    }

    // Matches the damage collider size to the visible laser beam.
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

    // Returns the damage collider offset for the current facing direction.
    private Vector2 GetDirectionalDamageOffset()
    {
        if (IsFacingLeft())
        {
            return new Vector2(_damageBoxOffset.x, -_damageBoxOffset.y);
        }

        return _damageBoxOffset;
    }

    // Returns whether the laser emitter is facing left.
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

    // Enables or disables the laser damage collider.
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
