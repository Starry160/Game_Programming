using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("子弹飞行速度（米/秒）。")]
    public float speed = 10f;

    [Tooltip("命中造成的伤害值。")]
    public int damage = 10;

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

        if (col.CompareTag("Enemy"))
        {
            Debug.Log($"{gameObject.name} 击中了敌人: {col.name}");
            // TODO: 之后在这里调用敌人扣血接口，例如 col.GetComponent<IDamageable>()?.TakeDamage(damage);
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
            return;
        }

        if (col.CompareTag("Wall"))
        {
            Debug.Log($"{gameObject.name} 撞墙了！");
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
