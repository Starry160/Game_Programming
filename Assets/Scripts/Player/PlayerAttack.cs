using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Main player combat script. It reads the selected weapon index from the class system and runs
/// the matching attack style: knight sword sweep, mage fireball, or archer arrow.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    private const int WEAPON_SWORD = 0;
    private const int WEAPON_STAFF = 1;
    private const int WEAPON_BOW = 2;

    [Header("References")]
    [Tooltip("Weapon pivot rotated and moved by attack animations.")]
    public Transform weaponPivot;

    [Tooltip("Center point used for melee damage checks.")]
    public Transform attackPoint;

    [Header("Audio")]
    [Tooltip("AudioSource used for attack sound effects.")]
    public AudioSource audioSource;

    [Tooltip("Sound effect played by the sword attack.")]
    public AudioClip swordSfx;

    [Tooltip("Sound effect played by the staff attack.")]
    public AudioClip staffSfx;

    [Tooltip("Sound effect played by the bow attack.")]
    public AudioClip bowSfx;

    [Header("Settings")]
    [Tooltip("Total duration of the sword swing.")]
    public float swordSwingDuration = 0.2f;

    [Tooltip("Target sword swing angle in degrees.")]
    public float swordSwingAngle = -120f;

    [Header("Sword Visuals")]
    [Tooltip("Shows a short blue crescent slash during the knight's downward sword swing.")]
    public bool showSwordSlash = true;

    [Tooltip("Point used to place the visual slash. The weapon pivot keeps it near the sword tip.")]
    public Transform swordSlashSpawnPoint;

    [Tooltip("Local offset from the slash spawn point.")]
    public Vector2 swordSlashOffset = new Vector2(0.58f, 0.9f);

    [Tooltip("World scale of the visual-only sword slash.")]
    public Vector2 swordSlashScale = new Vector2(0.75f, 0.75f);

    [Tooltip("Delay after the sword attack starts before the slash appears.")]
    public float swordSlashDelay = 0.03f;

    [Tooltip("How long the visual slash stays on screen.")]
    public float swordSlashDuration = 0.18f;

    [Tooltip("Sorting order used by the slash so it renders above the player and weapon.")]
    public int swordSlashSortingOrder = 13;

    [Header("Settings")]
    [Tooltip("Total duration of the staff swing.")]
    public float staffSwingDuration = 0.1f;

    [Tooltip("Target staff swing angle in degrees.")]
    public float staffSwingAngle = 25f;

    [Tooltip("Fireball prefab fired by the Mage staff.")]
    public GameObject fireballPrefab;

    [Tooltip("Transform used as the staff projectile spawn point.")]
    public Transform staffFirePoint;

    [Header("Settings")]
    [Tooltip("Total duration of the bow recoil animation.")]
    public float bowRecoilDuration = 0.1f;

    [Tooltip("Local recoil distance applied when drawing the bow.")]
    public float bowRecoilDistance = 0.15f;

    [Tooltip("Arrow prefab fired by the Archer bow.")]
    public GameObject arrowPrefab;

    [Tooltip("Transform used as the arrow spawn point.")]
    public Transform bowFirePoint;

    [Header("Cooldown")]
    [Tooltip("Cooldown time between attacks in seconds.")]
    public float attackCooldown;

    [Header("Damage")]
    [Tooltip("Radius of the melee damage sector.")]
    public float attackRange;

    [Tooltip("Total angle of the melee damage sector in degrees.")]
    public float attackAngle;

    [Tooltip("Forward range added around the blue sword slash.")]
    public float swordSlashDamageRadius = 0.7f;

    [Tooltip("Angle of the blue sword slash damage sector in degrees.")]
    public float swordSlashDamageAngle = 80f;

    [Tooltip("Shows the real sword damage sweep and slash hit area in the Scene view.")]
    public bool showSwordDamageGizmos = true;

    [Tooltip("Layers included in melee damage checks.")]
    public LayerMask enemyLayers;

    [Tooltip("Damage dealt by each sword melee hit.")]
    public int attackDamage;
    [Tooltip("Melee damage dealt to the final boss per hit.")]
    public float bossMeleeDamagePerHit = 1f;
    [Tooltip("Damage dealt by each fireball.")]
    public float fireballDamage = 20f;
    [Tooltip("Damage dealt by each arrow.")]
    public float arrowDamage = 10f;

    private bool isAttacking = false;

    // Ensures attack sounds have an AudioSource configured for all weapon types.
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
    }

    // Starts the selected weapon attack when the player clicks and is off cooldown.
    private void Update()
    {
        if (isAttacking)
        {
            return;
        }

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (GlobalData.chosenWeaponIndex < 0)
        {
            return;
        }

        // Weapon index comes from the class selection flow, so each class maps to one attack coroutine.
        switch (GlobalData.chosenWeaponIndex)
        {
            case WEAPON_SWORD:
                StartCoroutine(SwordAttack());
                break;
            case WEAPON_STAFF:
                StartCoroutine(StaffAttack());
                break;
            case WEAPON_BOW:
                StartCoroutine(BowAttack());
                break;
            default:
                Debug.LogWarning($"[PlayerAttack] Unknown weapon index: {GlobalData.chosenWeaponIndex}.", this);
                break;
        }
    }

    // Runs the sword swing and melee damage check.
    private IEnumerator SwordAttack()
    {
        isAttacking = true;

        if (weaponPivot == null)
        {
            yield return new WaitForSeconds(swordSwingDuration + attackCooldown);
            isAttacking = false;
            yield break;
        }

        Quaternion defaultRotation = Quaternion.Euler(0f, 0f, 0f);
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, swordSwingAngle);
        float halfDuration = Mathf.Max(0.0001f, swordSwingDuration * 0.5f);

        // The hit resolves as the blade starts moving downward, matching the blue slash visual.
        PlayAttackSfx(swordSfx);
        StartCoroutine(ResolveSwordHitAfterDelay());

        yield return LerpRotation(defaultRotation, targetRotation, halfDuration);
        yield return LerpRotation(targetRotation, defaultRotation, halfDuration);

        weaponPivot.localRotation = defaultRotation;

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // Runs the staff swing and fires a magic projectile.
    private IEnumerator StaffAttack()
    {
        isAttacking = true;

        if (weaponPivot == null)
        {
            yield return new WaitForSeconds(staffSwingDuration + attackCooldown);
            isAttacking = false;
            yield break;
        }

        Quaternion startRotation = weaponPivot.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, staffSwingAngle);
        float halfDuration = Mathf.Max(0.0001f, staffSwingDuration * 0.5f);

        // Delay the projectile slightly so the fireball appears during the staff motion.
        yield return new WaitForSeconds(0.05f);
        SpawnFireball();
        PlayAttackSfx(staffSfx);

        yield return LerpRotation(startRotation, targetRotation, halfDuration);
        yield return LerpRotation(targetRotation, startRotation, halfDuration);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // Runs bow recoil and fires an arrow projectile.
    private IEnumerator BowAttack()
    {
        isAttacking = true;

        if (weaponPivot == null)
        {
            yield return new WaitForSeconds(bowRecoilDuration + attackCooldown);
            isAttacking = false;
            yield break;
        }

        Vector3 startPosition = weaponPivot.localPosition;
        Vector3 recoilPosition = startPosition + new Vector3(-bowRecoilDistance, 0f, 0f);
        float halfDuration = Mathf.Max(0.0001f, bowRecoilDuration * 0.5f);

        // Bow attack fires first, then uses a short local-position recoil for feedback.
        SpawnProjectileTowardMouse(arrowPrefab, bowFirePoint, "Arrow", arrowDamage);
        PlayAttackSfx(bowSfx);

        yield return LerpPosition(startPosition, recoilPosition, halfDuration);
        yield return LerpPosition(recoilPosition, startPosition, halfDuration);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // Spawns the mage fireball from the staff fire point.
    private void SpawnFireball()
    {
        SpawnProjectileTowardMouse(fireballPrefab, staffFirePoint, "Fireball", fireballDamage);
    }

    // Plays an attack sound with slight pitch variation.
    private void PlayAttackSfx(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
    }

    // Delays sword hit resolution so damage matches the downward swing and blue slash.
    private IEnumerator ResolveSwordHitAfterDelay()
    {
        if (swordSlashDelay > 0f)
        {
            yield return new WaitForSeconds(swordSlashDelay);
        }

        if (showSwordSlash)
        {
            SpawnSwordSlash();
        }

        PerformDamage();
    }

    // Creates a short-lived blue crescent without adding any damage collider.
    private void SpawnSwordSlash()
    {
        Transform spawnPoint = swordSlashSpawnPoint != null ? swordSlashSpawnPoint : weaponPivot;
        if (spawnPoint == null)
        {
            spawnPoint = attackPoint;
        }

        if (spawnPoint == null)
        {
            return;
        }

        bool facingRight = transform.localScale.x >= 0f;

        GameObject slashObject = new GameObject("SwordSlashBlue");
        slashObject.transform.position = GetSwordSlashWorldPosition();

        slashObject.AddComponent<SpriteRenderer>();
        SwordSlashEffect slashEffect = slashObject.AddComponent<SwordSlashEffect>();
        slashEffect.Initialize(
            facingRight,
            swordSlashDuration,
            swordSlashScale,
            ResolveSwordSlashSortingLayerId(),
            swordSlashSortingOrder);
    }

    // Resolves the sorting layer used to render the sword slash effect.
    private int ResolveSwordSlashSortingLayerId()
    {
        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            return playerRenderer.sortingLayerID;
        }

        SpriteRenderer childRenderer = GetComponentInChildren<SpriteRenderer>();
        return childRenderer != null ? childRenderer.sortingLayerID : 0;
    }

    // Spawns and aims a projectile toward the mouse world position.
    private void SpawnProjectileTowardMouse(GameObject prefab, Transform spawnPoint, string debugName, float projectileDamage)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[PlayerAttack] {debugName} prefab is not configured, so it cannot be fired.", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning($"[PlayerAttack] {debugName} prefab is not configured, so it cannot be fired.", this);
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null || Mouse.current == null)
        {
            return;
        }

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        float distanceToCamera = Mathf.Abs(mainCamera.transform.position.z - spawnPoint.position.z);
        Vector3 screenPosWithZ = new Vector3(mouseScreenPos.x, mouseScreenPos.y, distanceToCamera);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);

        // Aim angle is calculated from spawn point to mouse world position before instantiating.
        Vector2 aimDir = (Vector2)(mouseWorldPos - spawnPoint.position);
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        GameObject projectileObj = Instantiate(prefab, spawnPoint.position, Quaternion.Euler(0f, 0f, angle));
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetDamage(projectileDamage);
        }
    }

    // Interpolates weapon rotation over a short attack window.
    private IEnumerator LerpRotation(Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            weaponPivot.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        weaponPivot.localRotation = to;
    }

    // Interpolates weapon local position for bow recoil.
    private IEnumerator LerpPosition(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            weaponPivot.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        weaponPivot.localPosition = to;
    }

    // Applies melee damage to enemies inside the sword sweep or blue slash area.
    private void PerformDamage()
    {
        Vector2 damageOrigin = GetSwordDamageOrigin();
        Vector2 facingDir = transform.localScale.x > 0f ? Vector2.right : Vector2.left;
        float sectorRange = Mathf.Max(0.05f, attackRange);
        float halfAngle = Mathf.Clamp(attackAngle * 0.5f, 0f, 180f);
        Vector2 slashCenter = GetSwordSlashWorldPosition();
        Vector2 slashDir = GetSwordSlashDamageDirection(damageOrigin, facingDir, slashCenter);
        float slashRange = GetSwordSlashDamageRange(damageOrigin, slashCenter);
        float slashHalfAngle = Mathf.Clamp(swordSlashDamageAngle * 0.5f, 0f, 180f);
        float queryRange = Mathf.Max(sectorRange, slashRange);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            damageOrigin,
            queryRange,
            enemyLayers);
        HashSet<Component> damagedTargets = new HashSet<Component>();

        // Targets inside either the sword sweep cone or the blue slash fan receive the melee hit.
        for (int i = 0; i < hitEnemies.Length; i++)
        {
            Collider2D enemy = hitEnemies[i];
            if (enemy == null)
            {
                continue;
            }

            if (IsInsideSwordDamage(enemy, damageOrigin, facingDir, sectorRange, halfAngle, slashDir, slashRange, slashHalfAngle))
            {
                Component damageTarget = ResolveMeleeDamageTarget(enemy);
                if (damageTarget == null)
                {
                    Debug.LogWarning($"[PlayerAttack] Sword hit {enemy.name}, but EnemyHealth was not found.", enemy);
                    continue;
                }

                if (damagedTargets.Add(damageTarget))
                {
                    ApplyMeleeDamage(damageTarget);
                }
            }
        }
    }

    // Checks whether an enemy collider is inside either sword damage sector.
    private bool IsInsideSwordDamage(
        Collider2D enemy,
        Vector2 damageOrigin,
        Vector2 facingDir,
        float sectorRange,
        float halfAngle,
        Vector2 slashDir,
        float slashRange,
        float slashHalfAngle)
    {
        return IsColliderInsideForwardSector(enemy, damageOrigin, facingDir, sectorRange, halfAngle)
            || IsColliderInsideForwardSector(enemy, damageOrigin, slashDir, slashRange, slashHalfAngle);
    }

    // Checks whether a collider overlaps the forward-facing damage sector.
    private bool IsColliderInsideForwardSector(
        Collider2D collider,
        Vector2 origin,
        Vector2 facingDir,
        float range,
        float halfAngle)
    {
        Bounds bounds = collider.bounds;
        Vector2 center = bounds.center;
        Vector2 extents = bounds.extents;

        if (IsPointInsideForwardSector(collider.ClosestPoint(origin), origin, facingDir, range, halfAngle))
        {
            return true;
        }

        if (IsPointInsideForwardSector(center, origin, facingDir, range, halfAngle))
        {
            return true;
        }

        Vector2 min = bounds.min;
        Vector2 max = bounds.max;
        if (IsPointInsideForwardSector(new Vector2(min.x, min.y), origin, facingDir, range, halfAngle)
            || IsPointInsideForwardSector(new Vector2(min.x, max.y), origin, facingDir, range, halfAngle)
            || IsPointInsideForwardSector(new Vector2(max.x, min.y), origin, facingDir, range, halfAngle)
            || IsPointInsideForwardSector(new Vector2(max.x, max.y), origin, facingDir, range, halfAngle)
            || IsPointInsideForwardSector(center + new Vector2(extents.x, 0f), origin, facingDir, range, halfAngle)
            || IsPointInsideForwardSector(center - new Vector2(extents.x, 0f), origin, facingDir, range, halfAngle)
            || IsPointInsideForwardSector(center + new Vector2(0f, extents.y), origin, facingDir, range, halfAngle)
            || IsPointInsideForwardSector(center - new Vector2(0f, extents.y), origin, facingDir, range, halfAngle))
        {
            return true;
        }

        return false;
    }

    // Checks whether a point is inside a forward-facing sector.
    private static bool IsPointInsideForwardSector(
        Vector2 point,
        Vector2 origin,
        Vector2 facingDir,
        float range,
        float halfAngle)
    {
        Vector2 dirToTarget = point - origin;

        if (dirToTarget.sqrMagnitude > range * range)
        {
            return false;
        }

        if (dirToTarget.sqrMagnitude <= 0.0001f)
        {
            return true;
        }

        float angle = Vector2.Angle(facingDir, dirToTarget.normalized);
        return angle <= halfAngle;
    }

    // Resolves which damage component should receive a melee hit.
    private Component ResolveMeleeDamageTarget(Collider2D enemy)
    {
        FinalBossController bossController = enemy.GetComponent<FinalBossController>();
        if (bossController == null)
        {
            bossController = enemy.GetComponentInParent<FinalBossController>();
        }

        if (bossController != null)
        {
            return bossController;
        }

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            return enemyHealth;
        }

        return null;
    }

    // Applies melee damage to the resolved enemy or boss target.
    private void ApplyMeleeDamage(Component damageTarget)
    {
        FinalBossController bossController = damageTarget as FinalBossController;
        if (bossController != null)
        {
            bossController.TakeDamage(Mathf.Max(0f, bossMeleeDamagePerHit));
            return;
        }

        EnemyHealth enemyHealth = damageTarget as EnemyHealth;
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(1);
            return;
        }

    }

    // Gets the world position used as the origin for sword damage checks.
    private Vector2 GetSwordDamageOrigin()
    {
        if (weaponPivot != null)
        {
            return weaponPivot.position;
        }

        if (attackPoint != null)
        {
            return attackPoint.position;
        }

        return transform.position;
    }

    // Calculates the world position where the sword slash effect appears.
    private Vector2 GetSwordSlashWorldPosition()
    {
        if (weaponPivot != null)
        {
            Quaternion hitRotation = GetSwordHitLocalRotation();
            Vector3 localSlashPosition = weaponPivot.localPosition + hitRotation * (Vector3)swordSlashOffset;
            Transform parent = weaponPivot.parent;
            return parent != null ? parent.TransformPoint(localSlashPosition) : localSlashPosition;
        }

        Transform spawnPoint = swordSlashSpawnPoint != null ? swordSlashSpawnPoint : attackPoint;
        if (spawnPoint != null)
        {
            return spawnPoint.TransformPoint(swordSlashOffset);
        }

        return transform.position;
    }

    // Calculates the local rotation reached when the sword hit is triggered.
    private Quaternion GetSwordHitLocalRotation()
    {
        float halfDuration = Mathf.Max(0.0001f, swordSwingDuration * 0.5f);
        float t = Mathf.Clamp01(Mathf.Max(0f, swordSlashDelay) / halfDuration);
        return Quaternion.Slerp(
            Quaternion.Euler(0f, 0f, 0f),
            Quaternion.Euler(0f, 0f, swordSwingAngle),
            t);
    }

    // Draws the real sword sweep and slash hit area in the Scene view.
    private void OnDrawGizmosSelected()
    {
        if (!showSwordDamageGizmos)
        {
            return;
        }

        Vector2 damageOrigin = GetSwordDamageOrigin();
        Vector3 facingDir = transform.localScale.x > 0f ? Vector3.right : Vector3.left;
        float sectorRange = Mathf.Max(0.05f, attackRange);
        float halfAngle = Mathf.Clamp(attackAngle * 0.5f, 0f, 180f);

        DrawDamageSector(damageOrigin, facingDir, sectorRange, halfAngle, new Color(1f, 0.25f, 0.1f, 0.18f));

        Vector2 slashCenter = GetSwordSlashWorldPosition();
        Vector2 slashDir = GetSwordSlashDamageDirection(damageOrigin, facingDir, slashCenter);
        float slashRange = GetSwordSlashDamageRange(damageOrigin, slashCenter);
        float slashHalfAngle = Mathf.Clamp(swordSlashDamageAngle * 0.5f, 0f, 180f);
        DrawDamageSector(damageOrigin, slashDir, slashRange, slashHalfAngle, new Color(0.15f, 0.85f, 1f, 0.24f));
    }

    // Calculates the damage range needed to cover the slash effect area.
    private float GetSwordSlashDamageRange(Vector2 damageOrigin, Vector2 slashCenter)
    {
        float distanceToSlash = Vector2.Distance(damageOrigin, slashCenter);
        return Mathf.Max(0.05f, distanceToSlash + swordSlashDamageRadius);
    }

    // Calculates the direction from the sword damage origin toward the slash center.
    private static Vector2 GetSwordSlashDamageDirection(Vector2 damageOrigin, Vector2 fallbackDir, Vector2 slashCenter)
    {
        Vector2 slashDir = slashCenter - damageOrigin;
        if (slashDir.sqrMagnitude <= 0.0001f)
        {
            return fallbackDir.sqrMagnitude > 0f ? fallbackDir.normalized : Vector2.right;
        }

        return slashDir.normalized;
    }

    // Draws a filled and outlined damage sector in the Scene view.
    private static void DrawDamageSector(Vector3 origin, Vector3 facingDir, float radius, float halfAngle, Color color)
    {
#if UNITY_EDITOR
        Vector3 normalizedFacing = facingDir.sqrMagnitude > 0f ? facingDir.normalized : Vector3.right;
        Vector3 startDir = Quaternion.Euler(0f, 0f, -halfAngle) * normalizedFacing;
        Handles.color = color;
        Handles.DrawSolidArc(origin, Vector3.forward, startDir, halfAngle * 2f, radius);
#endif

        Color outlineColor = new Color(color.r, color.g, color.b, 1f);
        Gizmos.color = outlineColor;
        DrawWireSector(origin, facingDir, radius, halfAngle);
    }

    // Draws the outline of a sector using gizmo lines and an arc.
    private static void DrawWireSector(Vector3 origin, Vector3 facingDir, float radius, float halfAngle)
    {
        Vector3 normalizedFacing = facingDir.sqrMagnitude > 0f ? facingDir.normalized : Vector3.right;
        Vector3 upperLine = Quaternion.Euler(0f, 0f, halfAngle) * normalizedFacing * radius;
        Vector3 lowerLine = Quaternion.Euler(0f, 0f, -halfAngle) * normalizedFacing * radius;

        Gizmos.DrawLine(origin, origin + upperLine);
        Gizmos.DrawLine(origin, origin + lowerLine);
        Gizmos.DrawWireSphere(origin, 0.04f);

        const int segments = 28;
        Vector3 previousPoint = origin + lowerLine;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 nextPoint = origin + Quaternion.Euler(0f, 0f, angle) * normalizedFacing * radius;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}
