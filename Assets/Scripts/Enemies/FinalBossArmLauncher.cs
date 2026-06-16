using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the final boss arm-projectile attack. Animation events call this script to fire
/// pooled arm projectiles from the correct launch point toward the player.
/// </summary>
public class FinalBossArmLauncher : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private FinalBossArmProjectile armProjectilePrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileDamage = 1f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private LayerMask hitMask;

    [Header("Pool")]
    [SerializeField] private int prewarmCount = 4;

    [Header("Aim")]
    [SerializeField] private bool autoAimAtPlayer = true;
    [SerializeField] private bool facePlayerWhenFiring = true;
    [SerializeField] private Vector2 fallbackDirection = Vector2.right;

    [Header("Audio")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioClip armFireSfx;
    [SerializeField, Range(0f, 1f)] private float armFireSfxVolume = 0.45f;
    [SerializeField] private Vector2 armFirePitchRange = new Vector2(0.95f, 1.05f);

    private readonly Queue<FinalBossArmProjectile> _pool = new Queue<FinalBossArmProjectile>();
    private Transform _playerTransform;

    // Stores the player target, audio source, and spawn center used by the boss arm projectiles.
    private void Awake()
    {
        if (attackAudioSource == null)
        {
            attackAudioSource = GetComponent<AudioSource>();
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        PrewarmPool();
    }

    // Creates pooled projectile objects before combat starts.
    private void PrewarmPool()
    {
        if (armProjectilePrefab == null)
        {
            return;
        }

        int count = Mathf.Max(0, prewarmCount);
        for (int i = 0; i < count; i++)
        {
            FinalBossArmProjectile projectile = Instantiate(armProjectilePrefab, transform.position, Quaternion.identity);
            projectile.gameObject.SetActive(false);
            _pool.Enqueue(projectile);
        }
    }

    /// <summary>
    /// Fires the boss arm projectile at the animation release frame.
    /// </summary>
    public void OnCastRelease()
    {
        FireArm();
    }

    // Launches a pooled arm projectile in the resolved facing direction.
    public void FireArm()
    {
        if (armProjectilePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = launchPoint != null ? launchPoint.position : transform.position;
        Vector2 shootDirection = GetShootDirection(spawnPosition);
        ApplyFacingByShootDirection(shootDirection);

        FinalBossArmProjectile projectile = GetProjectileFromPool();
        projectile.transform.position = spawnPosition;
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg);
        projectile.gameObject.SetActive(true);
        projectile.Activate(this, shootDirection, projectileSpeed, projectileDamage, projectileLifetime, hitMask);
        PlayArmFireSfx();
    }

    // Plays the thrown-arm launch sound when the projectile is actually released.
    private void PlayArmFireSfx()
    {
        if (armFireSfx == null)
        {
            return;
        }

        if (attackAudioSource == null)
        {
            AudioSource.PlayClipAtPoint(armFireSfx, transform.position, armFireSfxVolume);
            return;
        }

        attackAudioSource.pitch = Random.Range(armFirePitchRange.x, armFirePitchRange.y);
        attackAudioSource.PlayOneShot(armFireSfx, armFireSfxVolume);
    }

    // Resolves the direction used by the boss arm projectile.
    private Vector2 GetShootDirection(Vector3 spawnPosition)
    {
        if (autoAimAtPlayer)
        {
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
            }

            if (_playerTransform != null)
            {
                Vector2 toPlayer = (Vector2)(_playerTransform.position - spawnPosition);
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    return toPlayer.normalized;
                }
            }
        }

        if (fallbackDirection.sqrMagnitude <= 0.0001f)
        {
            return Vector2.right;
        }

        return fallbackDirection.normalized;
    }

    // Flips boss visuals to match the arm projectile direction.
    private void ApplyFacingByShootDirection(Vector2 shootDirection)
    {
        if (!facePlayerWhenFiring)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        if (shootDirection.x > 0.001f)
        {
            spriteRenderer.flipX = false;
        }
        else if (shootDirection.x < -0.001f)
        {
            spriteRenderer.flipX = true;
        }
    }

    // Gets an available arm projectile from the object pool.
    private FinalBossArmProjectile GetProjectileFromPool()
    {
        while (_pool.Count > 0)
        {
            FinalBossArmProjectile candidate = _pool.Dequeue();
            if (candidate != null)
            {
                return candidate;
            }
        }

        return Instantiate(armProjectilePrefab, transform.position, Quaternion.identity);
    }

    // Returns an arm projectile to the object pool.
    public void ReleaseProjectile(FinalBossArmProjectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        projectile.gameObject.SetActive(false);
        _pool.Enqueue(projectile);
    }
}
