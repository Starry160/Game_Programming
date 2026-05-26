using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("子弹飞行速度（米/秒）。")]
    public float speed = 10f;

    [Tooltip("命中敌人造成的伤害值。")]
    public float damage = 20f;

    [Tooltip("存活时间（秒），超时自动销毁，防止飞出地图永久残留。")]
    public float lifeTime = 2f;

    [Tooltip("命中敌人或墙壁时生成的爆炸特效预制体（通常挂 AutoDestroy 脚本）。")]
    public GameObject explosionPrefab;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 沿自身 +X 方向匀速前进；方向由发射时的 rotation 决定。
        transform.position += transform.right * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 防误伤：刚出生就可能擦到法师自身的触发器，直接忽略。
        if (col.CompareTag("Player"))
        {
            return;
        }

        if (col.CompareTag("Enemy") || col.gameObject.name.Contains("Enemy"))
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = col.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1);
                Debug.Log($"{gameObject.name} 击中了敌人: {enemyHealth.name}, damage=1");
            }
            else
            {
                EnemyAI enemy = col.GetComponent<EnemyAI>();
                if (enemy == null)
                {
                    enemy = col.GetComponentInParent<EnemyAI>();
                }

                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"{gameObject.name} 击中了敌人(旧血量系统): {enemy.name}, damage={damage}");
                }
                else
                {
                    Debug.LogWarning($"[Projectile] 命中疑似敌人对象 {col.name}，但未找到 EnemyHealth/EnemyAI。");
                }
            }

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
            return;
        }

        if (IsEnvironmentCollider(col))
        {
            OnHitEnvironment(col.gameObject);
        }
    }

    private void OnHitEnvironment(GameObject environment)
    {
        Debug.Log($"{gameObject.name} 击中环境: {environment.name}");
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

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

        bool looksLikeDoor = col.CompareTag("DungeonDoor") || col.gameObject.name.Contains("Door");
        if (!looksLikeDoor)
        {
            return false;
        }

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
}
