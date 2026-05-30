using UnityEngine;

/// <summary>
/// Monster 独立 AI：
/// - 玩家未进入本房间：只随机跑动，不攻击。
/// - 玩家进入本房间：可发射火球；若 MouthPoint -> Player 被 Wall 阻挡，则跑位找直线角度。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class MonsterAI : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private RoomController roomController;

    [Header("Random Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float wanderRadius = 2.2f;
    [SerializeField] private float wanderInterval = 1.0f;
    [SerializeField] private float idleDuration = 0.4f;
    [SerializeField] private float runThreshold = 0.05f;

    [Header("LOS Reposition")]
    [SerializeField] private float strafeWeight = 0.85f;
    [SerializeField] private float approachWeight = 0.35f;
    [SerializeField] private float strafeSwitchInterval = 0.8f;
    [SerializeField] private float strafeProbeDistance = 2.4f;
    [SerializeField] private float stuckCheckInterval = 0.6f;
    [SerializeField] private float stuckMinMoveDistance = 0.08f;

    [Header("Fireball Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileDamage = 1f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioClip fireballSfx;
    [SerializeField, Range(0f, 1f)] private float fireballSfxVolume = 0.45f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Transform _playerTransform;
    private PlayerStats _playerStats;
    private bool _hasLoggedMissingRoomController = false;
    private Collider2D _cachedRoomTriggerZone;

    private Vector2 _wanderTargetPos;
    private float _nextWanderPickTime = 0f;
    private float _idleTimer = 0f;
    private float _lastAttackTime = -999f;
    private int _strafeSign = 1;
    private float _nextStrafeSwitchTime = 0f;
    private Vector2 _lastRepositionSamplePos;
    private float _nextStuckCheckTime = 0f;

    private void Awake()
    {
        if (roomController == null)
        {
            roomController = GetComponentInParent<RoomController>();
        }

        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        attackAudioSource = GetComponent<AudioSource>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerStats = player.GetComponent<PlayerStats>();
            if (_playerStats == null)
            {
                _playerStats = player.GetComponentInParent<PlayerStats>();
            }
        }

        _wanderTargetPos = _rb.position;
        _nextWanderPickTime = Time.time;
        _nextStrafeSwitchTime = Time.time + strafeSwitchInterval;
        _lastRepositionSamplePos = _rb.position;
        _nextStuckCheckTime = Time.time + stuckCheckInterval;
    }

    private void FixedUpdate()
    {
        if (!IsPlayerAlive())
        {
            if (_rb != null)
            {
                _rb.velocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            if (_animator != null)
            {
                _animator.SetBool("isRunning", false);
            }

            return;
        }

        Vector2 nextVelocity;
        bool playerInRoom = IsPlayerInSameRoom();
        bool hasClearShot = playerInRoom && HasClearShotToPlayer();

        if (playerInRoom && !hasClearShot)
        {
            nextVelocity = GetRepositionVelocity();
        }
        else
        {
            nextVelocity = GetRandomWanderVelocity();
        }

        _rb.velocity = nextVelocity;
        UpdateFacing(nextVelocity.x);
        UpdateAnimator();
        TryFireProjectile(playerInRoom, hasClearShot);
    }

    private void UpdateFacing(float velocityX)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (velocityX > runThreshold)
        {
            _spriteRenderer.flipX = false;
        }
        else if (velocityX < -runThreshold)
        {
            _spriteRenderer.flipX = true;
        }
    }

    private void UpdateFacingByAim(Vector2 aimDirection)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (aimDirection.x > 0.001f)
        {
            _spriteRenderer.flipX = false;
        }
        else if (aimDirection.x < -0.001f)
        {
            _spriteRenderer.flipX = true;
        }
    }

    private void UpdateAnimator()
    {
        if (_animator == null || _rb == null)
        {
            return;
        }

        bool isRunning = Mathf.Abs(_rb.velocity.x) > runThreshold;
        _animator.SetBool("isRunning", isRunning);
    }

    private bool IsPlayerAlive()
    {
        if (_playerTransform == null)
        {
            return false;
        }

        if (_playerStats == null)
        {
            _playerStats = _playerTransform.GetComponent<PlayerStats>();
            if (_playerStats == null)
            {
                _playerStats = _playerTransform.GetComponentInParent<PlayerStats>();
            }
        }

        if (_playerStats == null)
        {
            return true;
        }

        return _playerStats.currentHealth > 0f;
    }

    private bool IsPlayerInSameRoom()
    {
        if (_playerTransform == null)
        {
            return false;
        }

        if (roomController == null)
        {
            if (!_hasLoggedMissingRoomController)
            {
                Debug.LogWarning($"[MonsterAI] {name} 未绑定 RoomController，默认按同房间处理。", this);
                _hasLoggedMissingRoomController = true;
            }

            return true;
        }

        Collider2D roomTrigger = GetRoomTriggerZone();
        if (roomTrigger == null)
        {
            return true;
        }

        return roomTrigger.bounds.Contains(_playerTransform.position);
    }

    private Collider2D GetRoomTriggerZone()
    {
        if (_cachedRoomTriggerZone != null)
        {
            return _cachedRoomTriggerZone;
        }

        if (roomController == null)
        {
            return null;
        }

        Transform triggerTransform = roomController.transform.Find("RoomTriggerZone");
        if (triggerTransform != null)
        {
            _cachedRoomTriggerZone = triggerTransform.GetComponent<Collider2D>();
        }

        if (_cachedRoomTriggerZone == null)
        {
            _cachedRoomTriggerZone = roomController.GetComponent<Collider2D>();
        }

        return _cachedRoomTriggerZone;
    }

    private bool HasClearShotToPlayer()
    {
        if (_playerTransform == null)
        {
            return false;
        }

        Vector2 origin = GetMouthWorldPosition();
        Vector2 target = _playerTransform.position;
        RaycastHit2D[] hits = Physics2D.LinecastAll(origin, target);
        if (hits == null || hits.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i].collider;
            if (c == null || c.isTrigger)
            {
                continue;
            }

            Transform t = c.transform;
            if (t == transform || t.IsChildOf(transform))
            {
                continue;
            }

            if (c.CompareTag("Wall"))
            {
                return false;
            }
        }

        return true;
    }

    private Vector2 GetRandomWanderVelocity()
    {
        if (_idleTimer > 0f)
        {
            _idleTimer -= Time.fixedDeltaTime;
            return Vector2.zero;
        }

        if (Time.time >= _nextWanderPickTime || Vector2.Distance(_rb.position, _wanderTargetPos) < 0.15f)
        {
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            _wanderTargetPos = (Vector2)transform.position + randomOffset;
            _nextWanderPickTime = Time.time + wanderInterval;
            _idleTimer = idleDuration;
            return Vector2.zero;
        }

        Vector2 dir = (_wanderTargetPos - _rb.position).normalized;
        return dir * moveSpeed;
    }

    private Vector2 GetRepositionVelocity()
    {
        if (_playerTransform == null)
        {
            return Vector2.zero;
        }

        if (Time.time >= _nextStrafeSwitchTime)
        {
            _strafeSign = ChooseBetterStrafeSign();
            _nextStrafeSwitchTime = Time.time + strafeSwitchInterval;
        }

        if (Time.time >= _nextStuckCheckTime)
        {
            float moved = Vector2.Distance(_rb.position, _lastRepositionSamplePos);
            if (moved < stuckMinMoveDistance)
            {
                // 当前位置基本没推进，切另一侧避免左右抖动卡住。
                _strafeSign *= -1;
                _nextStrafeSwitchTime = Time.time + (strafeSwitchInterval * 0.5f);
            }

            _lastRepositionSamplePos = _rb.position;
            _nextStuckCheckTime = Time.time + stuckCheckInterval;
        }

        Vector2 toPlayer = ((Vector2)_playerTransform.position - _rb.position).normalized;
        Vector2 side = new Vector2(-toPlayer.y, toPlayer.x) * _strafeSign;
        Vector2 desired = (side * strafeWeight) + (toPlayer * approachWeight);
        if (desired.sqrMagnitude < 0.0001f)
        {
            return Vector2.zero;
        }

        return desired.normalized * moveSpeed;
    }

    private int ChooseBetterStrafeSign()
    {
        if (_playerTransform == null)
        {
            return _strafeSign;
        }

        Vector2 toPlayer = ((Vector2)_playerTransform.position - _rb.position).normalized;
        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            return _strafeSign;
        }

        Vector2 leftDir = new Vector2(-toPlayer.y, toPlayer.x);
        Vector2 rightDir = -leftDir;

        float leftClear = ProbeSideClearDistance(leftDir);
        float rightClear = ProbeSideClearDistance(rightDir);

        // 差距不明显时保持当前方向，避免频繁切换。
        if (Mathf.Abs(leftClear - rightClear) < 0.12f)
        {
            return _strafeSign;
        }

        return leftClear > rightClear ? 1 : -1;
    }

    private float ProbeSideClearDistance(Vector2 sideDir)
    {
        Vector2 origin = GetMouthWorldPosition();
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, sideDir, strafeProbeDistance);
        float nearestWallDistance = strafeProbeDistance;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i].collider;
            if (c == null || c.isTrigger)
            {
                continue;
            }

            Transform t = c.transform;
            if (t == transform || t.IsChildOf(transform))
            {
                continue;
            }

            if (c.CompareTag("Wall"))
            {
                nearestWallDistance = Mathf.Min(nearestWallDistance, hits[i].distance);
            }
        }

        return nearestWallDistance;
    }

    private void TryFireProjectile(bool playerInRoom, bool hasClearShot)
    {
        if (!playerInRoom || !hasClearShot || _playerTransform == null || projectilePrefab == null)
        {
            return;
        }

        if (Time.time - _lastAttackTime < attackCooldown)
        {
            return;
        }

        Vector2 spawnPos = GetMouthWorldPosition();
        Vector2 fireDir = ((Vector2)_playerTransform.position - spawnPos).normalized;
        if (fireDir.sqrMagnitude < 0.0001f)
        {
            fireDir = _spriteRenderer != null && _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        // 发射瞬间让朝向与当前开火方向一致。
        UpdateFacingByAim(fireDir);

        float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.speed = projectileSpeed;
            projectile.damage = projectileDamage;
            projectile.targetSide = Projectile.TargetSide.Player;
            projectile.ownerTag = "Enemy";
            projectile.ownerTransform = transform;
        }

        _lastAttackTime = Time.time;
        PlayFireballSfx();
    }

    private Vector2 GetMouthWorldPosition()
    {
        if (mouthPoint != null)
        {
            return mouthPoint.position;
        }

        float xOffset = _spriteRenderer != null && _spriteRenderer.flipX ? -0.2f : 0.2f;
        return (Vector2)transform.position + new Vector2(xOffset, -0.06f);
    }

    private void PlayFireballSfx()
    {
        if (attackAudioSource == null || fireballSfx == null)
        {
            return;
        }

        // 与法师发射音同风格：轻微随机音高，减少机械重复感。
        attackAudioSource.pitch = Random.Range(0.9f, 1.1f);
        attackAudioSource.PlayOneShot(fireballSfx, fireballSfxVolume);
    }
}
