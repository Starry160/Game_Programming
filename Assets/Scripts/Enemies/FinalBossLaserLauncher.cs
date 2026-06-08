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
    [Header("Edge Check")]
    [SerializeField] private bool avoidLaserWhenBlockedByMapEdge = true;
    [SerializeField] private Collider2D roomTriggerZone;
    [SerializeField] private bool autoUseVisibleLaserLength = true;
    [SerializeField] private float edgeCheckDistance = 16.5f;
    [SerializeField, Range(0.5f, 1.2f)] private float edgeLengthScale = 1.1f;
    [SerializeField] private float edgeCheckPadding = 0.05f;

    [Header("Pool")]
    [SerializeField] private int prewarmCount = 2;

    private readonly Queue<FinalBossLaserBeam> _pool = new Queue<FinalBossLaserBeam>();
    private FinalBossLaserBeam _activeLaser;

    private void Awake()
    {
        AutoBindRoomTriggerZone();
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

        // Final gate at fire time: facing/position may have changed after action selection.
        if (!CanFireLaserFromCurrentPose())
        {
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
        Vector3 spawnPosition = ResolveCheckStartPosition(firePoint, castRotation);
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

    public bool CanFireLaserFromCurrentPose()
    {
        if (!avoidLaserWhenBlockedByMapEdge)
        {
            return true;
        }

        AutoBindRoomTriggerZone();
        if (roomTriggerZone == null)
        {
            return true;
        }

        Transform firePoint = laserOrigin != null ? laserOrigin : transform;
        Quaternion castRotation = ResolveCastRotation(firePoint);
        Vector3 spawnPosition = ResolveCheckStartPosition(firePoint, castRotation);
        float originX = spawnPosition.x;
        float laserLength = GetRequiredLaserLength(firePoint);
        float padding = Mathf.Max(0f, edgeCheckPadding);
        Bounds bounds = roomTriggerZone.bounds;
        Vector3 dir = castRotation * Vector3.right;
        bool facingLeft = dir.x < 0f;

        if (facingLeft)
        {
            float minAllowed = bounds.min.x + padding;
            return originX - laserLength >= minAllowed;
        }

        float maxAllowed = bounds.max.x - padding;
        return originX + laserLength <= maxAllowed;
    }

    private void OnValidate()
    {
        edgeCheckDistance = Mathf.Max(0.05f, edgeCheckDistance);
        edgeLengthScale = Mathf.Clamp(edgeLengthScale, 0.5f, 1.2f);
        edgeCheckPadding = Mathf.Max(0f, edgeCheckPadding);
    }

    private float GetRequiredLaserLength(Transform firePoint)
    {
        float lengthScale = Mathf.Clamp(edgeLengthScale, 0.5f, 1.2f);
        if (!autoUseVisibleLaserLength || laserBeamPrefab == null)
        {
            return Mathf.Max(0.05f, edgeCheckDistance * lengthScale);
        }

        float scaleX;
        if (matchOriginScale && firePoint != null)
        {
            scaleX = Mathf.Abs(firePoint.lossyScale.x);
        }
        else
        {
            scaleX = Mathf.Abs(laserBeamPrefab.transform.localScale.x);
        }

        float normalizedScaleX = Mathf.Max(0.001f, scaleX);

        // Prefer the tuned damage box length for edge checks; it matches gameplay expectation better.
        BoxCollider2D prefabCollider = laserBeamPrefab.GetComponent<BoxCollider2D>();
        if (prefabCollider != null)
        {
            float colliderLength = Mathf.Max(0.01f, prefabCollider.size.x);
            return Mathf.Max(0.05f, colliderLength * normalizedScaleX * lengthScale);
        }

        // Fallback to sprite width when collider is missing.
        SpriteRenderer prefabRenderer = laserBeamPrefab.GetComponent<SpriteRenderer>();
        if (prefabRenderer != null && prefabRenderer.sprite != null)
        {
            float spriteWidth = Mathf.Max(0.01f, prefabRenderer.sprite.bounds.size.x);
            return Mathf.Max(0.05f, spriteWidth * normalizedScaleX * lengthScale);
        }

        return Mathf.Max(0.05f, edgeCheckDistance * lengthScale);
    }

    private void AutoBindRoomTriggerZone()
    {
        if (roomTriggerZone != null)
        {
            return;
        }

        RoomController room = GetComponentInParent<RoomController>();
        if (room == null)
        {
            return;
        }

        Transform triggerZone = room.transform.Find("RoomTriggerZone");
        if (triggerZone != null)
        {
            roomTriggerZone = triggerZone.GetComponent<Collider2D>();
        }

        if (roomTriggerZone == null)
        {
            roomTriggerZone = room.GetComponent<Collider2D>();
        }
    }

    private Vector3 ResolveCheckStartPosition(Transform firePoint, Quaternion castRotation)
    {
        if (firePoint == null)
        {
            return transform.position;
        }

        Vector3 dir = castRotation * Vector3.right;
        bool facingLeft = dir.x < 0f;
        Vector3 originPosition = firePoint.position;

        // Mirror muzzle origin on left cast, matching FinalBossLaserBeam.ResolveOriginPosition().
        if (lockDirectionOnCast && facingLeft && firePoint.parent != null)
        {
            Vector3 mirroredLocal = firePoint.localPosition;
            mirroredLocal.x = -mirroredLocal.x;
            originPosition = firePoint.parent.TransformPoint(mirroredLocal);
        }

        Quaternion offsetRotation = lockDirectionOnCast ? castRotation : firePoint.rotation;
        return originPosition + (offsetRotation * _laserVisualOffset);
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
