using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在 FinalBoss 上：由动画事件触发激光开始/结束，使用对象池复用激光体。
/// </summary>
public class FinalBossLaserLauncher : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private FinalBossLaserBeam laserBeamPrefab;
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private Vector3 _laserVisualOffset = Vector3.zero;
    [SerializeField] private float laserDuration = 1.0f;
    [SerializeField] private float damagePerTick = 1f;
    [SerializeField] private float tickInterval = 0.2f;
    [SerializeField] private bool lockDirectionOnCast = true;
    [SerializeField] private bool followBossFacingDirection = true;
    [SerializeField] private bool matchOriginScale = true;
    [SerializeField] private int sortingOrderOffset = 8;

    [Header("Pool")]
    [SerializeField] private int prewarmCount = 2;

    private readonly Queue<FinalBossLaserBeam> _pool = new Queue<FinalBossLaserBeam>();
    private FinalBossLaserBeam _activeLaser;

    private void Awake()
    {
        PrewarmPool();
    }

    private void PrewarmPool()
    {
        if (laserBeamPrefab == null)
        {
            return;
        }

        int count = Mathf.Max(0, prewarmCount);
        for (int i = 0; i < count; i++)
        {
            FinalBossLaserBeam beam = Instantiate(laserBeamPrefab, transform.position, Quaternion.identity);
            beam.gameObject.SetActive(false);
            _pool.Enqueue(beam);
        }
    }

    /// <summary>在 FinalBoss_LaserBeam 动画中添加事件调用。</summary>
    public void OnLaserFire()
    {
        FireLaser();
    }

    /// <summary>在 FinalBoss_LaserBeam 动画末段添加事件调用。</summary>
    public void OnLaserEnd()
    {
        StopLaser();
    }

    public void FireLaser()
    {
        if (laserBeamPrefab == null)
        {
            Debug.LogWarning("[FinalBossLaserLauncher] laserBeamPrefab 未绑定。", this);
            return;
        }

        if (_activeLaser != null)
        {
            _activeLaser.ReturnToPool();
            _activeLaser = null;
        }

        FinalBossLaserBeam beam = GetLaserFromPool();
        Transform firePoint = laserOrigin != null ? laserOrigin : transform;
        Quaternion castRotation = ResolveCastRotation(firePoint);
        Vector3 spawnPosition = firePoint.position + firePoint.TransformDirection(_laserVisualOffset);
        beam.transform.position = spawnPosition;
        beam.transform.rotation = castRotation;
        if (matchOriginScale)
        {
            beam.transform.localScale = firePoint.lossyScale;
        }

        // Keep beam rendering above boss to avoid being visually buried.
        SpriteRenderer bossRenderer = GetComponent<SpriteRenderer>();
        SpriteRenderer beamRenderer = beam.GetComponent<SpriteRenderer>();
        if (bossRenderer != null && beamRenderer != null)
        {
            beamRenderer.sortingLayerID = bossRenderer.sortingLayerID;
            beamRenderer.sortingOrder = bossRenderer.sortingOrder + sortingOrderOffset;
        }

        beam.gameObject.SetActive(true);
        beam.Activate(this, firePoint, _laserVisualOffset, damagePerTick, tickInterval, laserDuration, lockDirectionOnCast, castRotation);
        _activeLaser = beam;
    }

    private Quaternion ResolveCastRotation(Transform firePoint)
    {
        Quaternion baseRotation = firePoint != null ? firePoint.rotation : transform.rotation;
        if (!followBossFacingDirection)
        {
            return baseRotation;
        }

        SpriteRenderer bossRenderer = GetComponent<SpriteRenderer>();
        if (bossRenderer == null)
        {
            return baseRotation;
        }

        return bossRenderer.flipX ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
    }

    public void StopLaser()
    {
        if (_activeLaser == null)
        {
            return;
        }

        _activeLaser.ReturnToPool();
        _activeLaser = null;
    }

    private FinalBossLaserBeam GetLaserFromPool()
    {
        while (_pool.Count > 0)
        {
            FinalBossLaserBeam candidate = _pool.Dequeue();
            if (candidate != null)
            {
                return candidate;
            }
        }

        return Instantiate(laserBeamPrefab, transform.position, Quaternion.identity);
    }

    public void ReleaseLaser(FinalBossLaserBeam laser)
    {
        if (laser == null)
        {
            return;
        }

        if (_activeLaser == laser)
        {
            _activeLaser = null;
        }

        laser.gameObject.SetActive(false);
        _pool.Enqueue(laser);
    }
}
