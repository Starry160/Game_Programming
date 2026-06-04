using UnityEngine;

/// <summary>投射物：沿朝向飞行，命中敌人/环境后爆炸销毁。</summary>
public class Projectile : MonoBehaviour
{
    public enum TargetSide
    {
        Enemy,
        Player
    }

    [Header("Stats")]
    [Tooltip("子弹飞行速度（米/秒）。")]
    public float speed = 10f;

    [Tooltip("命中敌人造成的伤害值。")]
    public float damage = 20f;

    [Tooltip("存活时间（秒），超时自动销毁，防止飞出地图永久残留。")]
    public float lifeTime = 2f;

    [Tooltip("命中敌人或墙壁时生成的爆炸特效预制体（通常挂 AutoDestroy 脚本）。")]
    public GameObject explosionPrefab;

    [Header("Targeting")]
    [Tooltip("该投射物命中的目标阵营。")]
    public TargetSide targetSide = TargetSide.Enemy;
    [Tooltip("发射者 Tag（用于忽略同阵营发射者自身碰撞）。")]
    public string ownerTag = "Player";
    [Tooltip("发射者根节点（用于忽略出生后与自己碰撞）。")]
    public Transform ownerTransform;

    // 超时自动销毁。
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // 沿 transform.right 方向移动。
    private void Update()
    {
        // 沿自身 +X 方向匀速前进；方向由发射时的 rotation 决定。
        transform.position += transform.right * (speed * Time.deltaTime);
    }

    // 触发碰撞：敌人伤害或环境阻挡。
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(ownerTag) && col.CompareTag(ownerTag))
        {
            return;
        }

        if (ownerTransform != null &&
            (col.transform == ownerTransform || col.transform.IsChildOf(ownerTransform)))
        {
            return;
        }

        // 敌方投射物（target=Player）应穿过所有敌人，避免 Monster/Skeleton 互相挡弹。
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

    private bool IsPlayerCollider(Collider2D col)
    {
        return col.CompareTag("Player");
    }

    private void DamageEnemy(Collider2D col)
    {
        FinalBossController finalBoss = col.GetComponent<FinalBossController>();
        if (finalBoss == null)
        {
            finalBoss = col.GetComponentInParent<FinalBossController>();
        }

        if (finalBoss != null)
        {
            finalBoss.TakeDamage(damage);
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
            enemy.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning($"[Projectile] 命中疑似敌人对象 {col.name}，但未找到 EnemyHealth/EnemyAI。");
        }
    }

    private void DamagePlayer(Collider2D col)
    {
        PlayerStats playerStats = col.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = col.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
    }

    private void SpawnExplosionAndDestroy()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // 击中墙/关闭的门：生成特效并销毁。
    private void OnHitEnvironment(GameObject environment)
    {
        Debug.Log($"{gameObject.name} 击中环境: {environment.name}");
        SpawnExplosionAndDestroy();
    }

    // 判断是否为墙或仍有关闭碰撞体的门。
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
            // 门开启时通常会禁用“实体阻挡碰撞体”，但保留触发器做交互。
            // 只有在门仍有可阻挡的非 Trigger 碰撞体时，才视为环境命中。
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

        // 通用兜底：除敌人/玩家以外，任何实体非 Trigger 碰撞体都可阻挡投射物。
        // 这样可覆盖 TestCorridor_01 这类未打 Tag/无 DoorController 的普通阻挡体。
        return col.enabled && !col.isTrigger;
    }
}
