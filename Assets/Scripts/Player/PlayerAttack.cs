using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Dispatches sword, staff, and bow attacks based on the selected class weapon.</summary>
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
                StartCoroutine(SwordAttack());
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

        // Sword damage happens at the start of the swing so the visual follows the hit immediately.
        PerformDamage();
        PlayAttackSfx(swordSfx);

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
            projectile.damage = projectileDamage;
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

    // Applies melee damage to enemies inside the attack sector.
    private void PerformDamage()
    {
        if (attackPoint == null)
        {
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers);

        Vector2 facingDir = transform.localScale.x > 0f ? Vector2.right : Vector2.left;
        float halfAngle = attackAngle * 0.5f;

        // Only targets inside the forward-facing attack cone should receive melee damage.
        for (int i = 0; i < hitEnemies.Length; i++)
        {
            Collider2D enemy = hitEnemies[i];
            if (enemy == null)
            {
                continue;
            }

            Vector2 dirToEnemy = ((Vector2)enemy.transform.position - (Vector2)attackPoint.position).normalized;
            float angle = Vector2.Angle(facingDir, dirToEnemy);

            if (angle <= halfAngle)
            {
                // Boss damage uses its own controller, while normal enemies use health or legacy AI.
                FinalBossController bossController = enemy.GetComponent<FinalBossController>();
                if (bossController == null)
                {
                    bossController = enemy.GetComponentInParent<FinalBossController>();
                }

                if (bossController != null)
                {
                    bossController.TakeDamage(Mathf.Max(0f, bossMeleeDamagePerHit));
                    continue;
                }

                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                {
                    enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
                }

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(1);
                }
                else
                {
                    EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                    if (enemyAI == null)
                    {
                        enemyAI = enemy.GetComponentInParent<EnemyAI>();
                    }

                    if (enemyAI != null)
                    {
                        enemyAI.TakeDamage(attackDamage);
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerAttack] Sector hit {enemy.name}, but EnemyHealth/EnemyAI was not found.", enemy);
                    }
                }
            }
        }
    }

    // Draws the sword melee radius and attack cone in the Scene view.
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Vector3 facingDir = transform.localScale.x > 0f ? Vector3.right : Vector3.left;
        Vector3 upperLine = Quaternion.Euler(0f, 0f, attackAngle / 2f) * facingDir * attackRange;
        Vector3 lowerLine = Quaternion.Euler(0f, 0f, -attackAngle / 2f) * facingDir * attackRange;

        Gizmos.DrawLine(attackPoint.position, attackPoint.position + upperLine);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + lowerLine);
    }
}
