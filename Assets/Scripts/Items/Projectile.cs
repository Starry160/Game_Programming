using UnityEngine;

/// <summary>Moves projectiles forward, applies hit damage, and destroys them on impact.</summary>
public class Projectile : MonoBehaviour
{
    public enum TargetSide
    {
        Enemy,
        Player
    }

    [Header("Launch Defaults")]
    [Tooltip("Default movement speed in units per second. MonsterAI can override this when it fires the projectile.")]
    public float speed = 10f;

    [Tooltip("Damage dealt to the final boss per projectile hit. This is separate from regular damage so boss hearts stay balanced.")]
    public float bossDamagePerHit = 1f;

    [Tooltip("Maximum lifetime before the projectile destroys itself.")]
    public float lifeTime = 2f;

    [Tooltip("Effect spawned when the projectile hits a target or wall.")]
    public GameObject explosionPrefab;

    [Header("Targeting Defaults")]
    [Tooltip("Default faction this projectile is allowed to damage. MonsterAI overrides monster fireballs to target the player.")]
    public TargetSide targetSide = TargetSide.Enemy;
    [Tooltip("Default owner tag ignored by this projectile. Player projectiles usually ignore Player; monster projectiles use Enemy.")]
    public string ownerTag = "Player";
    [Tooltip("Runtime owner transform ignored by this projectile so it cannot hit its caster or caster child colliders.")]
    public Transform ownerTransform;

    private float _runtimeDamage;

    // Receives launch-time damage from the script that spawned this projectile.
    public void SetDamage(float amount)
    {
        _runtimeDamage = Mathf.Max(0f, amount);
    }

    // Schedules projectile cleanup so missed shots do not remain forever.
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Moves the projectile forward based on its speed each frame.
    private void Update()
    {
        transform.position += transform.right * (speed * Time.deltaTime);
    }

    // Routes projectile collisions to enemy damage, player damage, or environment impact.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ownerTag) && col.CompareTag(ownerTag))
        {
            // Ignore the shooter by tag before doing any expensive target checks.
            return;
        }

        if (ownerTransform != null &&
            (col.transform == ownerTransform || col.transform.IsChildOf(ownerTransform)))
        {
            // Owner transform catches child colliders that may not share the owner tag.
            return;
        }

        if (targetSide == TargetSide.Player && IsEnemyCollider(col))
        {
            return;
        }

        if (targetSide == TargetSide.Enemy && IsEnemyCollider(col))
        {
            DamageEnemy(col);
            SpawnExplosionAndDestroy();
            return;
        }

        if (targetSide == TargetSide.Player && IsPlayerCollider(col))
        {
            DamagePlayer(col);
            SpawnExplosionAndDestroy();
            return;
        }

        if (IsEnvironmentCollider(col))
        {
            OnHitEnvironment(col.gameObject);
        }
    }

    // Returns whether the projectile collider belongs to an enemy target.
    private bool IsEnemyCollider(Collider2D col)
    {
        if (col == null)
        {
            return false;
        }

        if (col.GetComponent<FinalBossController>() != null || col.GetComponentInParent<FinalBossController>() != null)
        {
            return true;
        }

        if (col.CompareTag("Enemy") || col.gameObject.name.Contains("Enemy"))
        {
            return true;
        }

        if (col.GetComponent<EnemyHealth>() != null || col.GetComponentInParent<EnemyHealth>() != null)
        {
            return true;
        }

        if (col.GetComponent<EnemyAI>() != null || col.GetComponentInParent<EnemyAI>() != null)
        {
            return true;
        }

        if (col.GetComponent<MonsterAI>() != null || col.GetComponentInParent<MonsterAI>() != null)
        {
            return true;
        }

        return false;
    }

    // Returns whether the projectile collider belongs to the player target.
    private bool IsPlayerCollider(Collider2D col)
    {
        return col.CompareTag("Player");
    }

    // Applies projectile damage to enemy or boss health components.
    private void DamageEnemy(Collider2D col)
    {
        FinalBossController finalBoss = col.GetComponent<FinalBossController>();
        if (finalBoss == null)
        {
            finalBoss = col.GetComponentInParent<FinalBossController>();
        }

        if (finalBoss != null)
        {
            // Boss health uses a smaller per-hit value than regular projectile damage.
            finalBoss.TakeDamage(Mathf.Max(0f, bossDamagePerHit));
            return;
        }

        EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = col.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(1);
            return;
        }

        EnemyAI enemy = col.GetComponent<EnemyAI>();
        if (enemy == null)
        {
            enemy = col.GetComponentInParent<EnemyAI>();
        }

        if (enemy != null)
        {
            enemy.TakeDamage(_runtimeDamage);
        }
        else
        {
            Debug.LogWarning($"[Projectile] Hit likely enemy object {col.name}, but EnemyHealth/EnemyAI was not found.");
        }
    }

    // Applies projectile damage to the player stats component.
    private void DamagePlayer(Collider2D col)
    {
        PlayerStats playerStats = col.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = col.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.TakeDamage(_runtimeDamage);
        }
    }

    // Spawns the impact effect and destroys this projectile.
    private void SpawnExplosionAndDestroy()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Handles projectile impact against walls, doors, or obstacles.
    private void OnHitEnvironment(GameObject environment)
    {
        Debug.Log($"{gameObject.name} hit environment: {environment.name}");
        SpawnExplosionAndDestroy();
    }

    // Treats walls, closed doors, and solid obstacles as projectile blockers.
    private bool IsEnvironmentCollider(Collider2D col)
    {
        if (col == null || col.gameObject == null)
        {
            return false;
        }

        if (col.CompareTag("Wall"))
        {
            return true;
        }

        bool hasDoorController = col.GetComponent<DoorController>() != null || col.GetComponentInParent<DoorController>() != null;
        bool looksLikeDoorOrGate =
            col.CompareTag("DungeonDoor") ||
            col.gameObject.name.Contains("Door") ||
            col.gameObject.name.Contains("Gate") ||
            hasDoorController;

        if (looksLikeDoorOrGate)
        {
            Collider2D[] sameObjectColliders = col.GetComponents<Collider2D>();
            for (int i = 0; i < sameObjectColliders.Length; i++)
            {
                Collider2D c = sameObjectColliders[i];
                if (c != null && c.enabled && !c.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        return col.enabled && !col.isTrigger;
    }
}
