using UnityEngine;

/// <summary>Controls ranged monster movement, line-of-sight repositioning, and fireball attacks.</summary>
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
    [SerializeField] private float idleDuration;
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
    [SerializeField] private float projectileSpeed;
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

    // Finds the player, room, physics, and animation references used by ranged AI.
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

    // Updates monster roaming, line-of-sight repositioning, and ranged firing.
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

        // Ranged monsters reposition for line of sight, otherwise they keep roaming.
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

    // Flips the enemy visuals toward movement or target direction.
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

    // Keeps legacy aim-facing logic disabled in favor of PlayerFacing.
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

    // Sets the running animation from current horizontal movement speed.
    private void UpdateAnimator()
    {
        if (_animator == null || _rb == null)
        {
            return;
        }

        bool isRunning = Mathf.Abs(_rb.velocity.x) > runThreshold;
        _animator.SetBool("isRunning", isRunning);
    }

    // Returns whether the player can still be targeted.
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

    // Checks whether the player is inside this monster room.
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
                Debug.LogWarning($"[MonsterAI] {name} has no RoomController assigned, so same-room behavior is assumed.", this);
                _hasLoggedMissingRoomController = true;
            }

            return true;
        }

        Collider2D roomTrigger = GetRoomTriggerZone();
        if (roomTrigger == null)
        {
            // Missing room bounds means the monster cannot confirm containment, so it stays active.
            return true;
        }

        return roomTrigger.bounds.Contains(_playerTransform.position);
    }

    // Finds the trigger zone used to define this monster room.
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

    // Checks whether a wall blocks the monster mouth from seeing the player.
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

        // Any non-trigger wall between mouth and player blocks a fireball shot.
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

    // Chooses a wandering direction when the player is not engaged.
    private Vector2 GetRandomWanderVelocity()
    {
        if (_idleTimer > 0f)
        {
            // Brief idle pauses make wandering less mechanical between target picks.
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

    // Chooses a side movement direction that improves line of sight to the player.
    private Vector2 GetRepositionVelocity()
    {
        if (_playerTransform == null)
        {
            return Vector2.zero;
        }

        // Periodically choose the side with better wall clearance for a shooting lane.
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
                // If the monster barely moved, flip side to escape local obstacles.
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

    // Chooses the strafe side with clearer line of sight.
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

        if (Mathf.Abs(leftClear - rightClear) < 0.12f)
        {
            return _strafeSign;
        }

        return leftClear > rightClear ? 1 : -1;
    }

    // Measures how far a side movement direction stays clear.
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

    // Spawns a fireball from the mouth point and aims it at the player.
    private void TryFireProjectile(bool playerInRoom, bool hasClearShot)
    {
        // Fire only when the player is in the same room and no wall blocks the shot.
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
            // If the player overlaps the mouth point, fall back to the monster's facing side.
            fireDir = _spriteRenderer != null && _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        UpdateFacingByAim(fireDir);

        float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.speed = projectileSpeed;
            projectile.SetDamage(projectileDamage);
            projectile.targetSide = Projectile.TargetSide.Player;
            projectile.ownerTag = "Enemy";
            projectile.ownerTransform = transform;
        }

        _lastAttackTime = Time.time;
        PlayFireballSfx();
    }

    // Returns the projectile spawn position at the monster mouth.
    private Vector2 GetMouthWorldPosition()
    {
        if (mouthPoint != null)
        {
            return mouthPoint.position;
        }

        float xOffset = _spriteRenderer != null && _spriteRenderer.flipX ? -0.2f : 0.2f;
        return (Vector2)transform.position + new Vector2(xOffset, -0.06f);
    }

    // Plays the monster fireball sound with slight pitch variation.
    private void PlayFireballSfx()
    {
        if (attackAudioSource == null || fireballSfx == null)
        {
            return;
        }

        attackAudioSource.pitch = Random.Range(0.9f, 1.1f);
        attackAudioSource.PlayOneShot(fireballSfx, fireballSfxVolume);
    }
}
