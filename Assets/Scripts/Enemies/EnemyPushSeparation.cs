using UnityEngine;

/// <summary>
/// Soft separation for enemy swarms.
/// Attach this on SeparationSensor trigger child.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class EnemyPushSeparation : MonoBehaviour
{
    [Header("Push Tuning")]
    [Tooltip("怪物之间互相挤开的基础推力。")]
    [SerializeField] private float pushStrength = 6f;

    [Tooltip("每次触发最多施加的位移速度（防止抖动）。")]
    [SerializeField] private float maxPushVelocity = 1.8f;

    [Tooltip("避免零距离归一化导致 NaN。")]
    [SerializeField] private float minDistanceEpsilon = 0.001f;

    [Header("References")]
    [Tooltip("默认自动使用父级刚体。")]
    [SerializeField] private Rigidbody2D ownerRigidbody;

    [Tooltip("默认自动使用父级根物体。")]
    [SerializeField] private Transform ownerRoot;

    private Collider2D _sensor;

    private void Awake()
    {
        _sensor = GetComponent<Collider2D>();
        _sensor.isTrigger = true;

        if (ownerRigidbody == null)
        {
            ownerRigidbody = GetComponentInParent<Rigidbody2D>();
        }

        if (ownerRoot == null)
        {
            ownerRoot = ownerRigidbody != null ? ownerRigidbody.transform : transform.root;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (ownerRigidbody == null || ownerRoot == null || other == null)
        {
            return;
        }

        Rigidbody2D otherRb = other.attachedRigidbody;
        if (otherRb == null || otherRb == ownerRigidbody)
        {
            return;
        }

        Transform otherRoot = otherRb.transform;
        if (otherRoot == null || !otherRoot.CompareTag("Enemy"))
        {
            return;
        }

        Vector2 selfPos = ownerRoot.position;
        Vector2 otherPos = otherRoot.position;
        Vector2 delta = selfPos - otherPos;

        float sqrDist = delta.sqrMagnitude;
        if (sqrDist <= minDistanceEpsilon * minDistanceEpsilon)
        {
            // 完全重叠时给一个稳定伪随机方向，避免“永远粘在一起”。
            float angle = (ownerRoot.GetInstanceID() & 255) * Mathf.Deg2Rad * 1.40625f;
            delta = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            sqrDist = 1f;
        }

        Vector2 pushDir = delta / Mathf.Sqrt(sqrDist);
        float distanceFactor = Mathf.Clamp01(1f - Mathf.Sqrt(sqrDist)); // 越近推得越明显
        Vector2 push = pushDir * (pushStrength * (0.35f + distanceFactor)) * Time.fixedDeltaTime;

        Vector2 targetVelocity = ownerRigidbody.velocity + push;
        ownerRigidbody.velocity = Vector2.ClampMagnitude(targetVelocity, maxPushVelocity);
    }
}
