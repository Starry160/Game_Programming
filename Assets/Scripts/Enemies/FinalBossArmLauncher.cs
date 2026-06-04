using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在 FinalBoss 上：由 Casting 动画事件触发，发射独立“手臂飞行物”。
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

    private readonly Queue<FinalBossArmProjectile> _pool = new Queue<FinalBossArmProjectile>();
    private Transform _playerTransform;

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        PrewarmPool();
    }

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
    /// 在 Casting 动画末帧添加 Animation Event，函数名填这个：OnCastRelease
    /// </summary>
    public void OnCastRelease()
    {
        FireArm();
    }

    public void FireArm()
    {
        if (armProjectilePrefab == null)
        {
            Debug.LogWarning("[FinalBossArmLauncher] armProjectilePrefab 未绑定。", this);
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
    }

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
