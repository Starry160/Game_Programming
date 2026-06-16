using UnityEngine;

/// <summary>
/// Adds gentle separation between nearby enemies so groups do not stack on top of each other.
/// Attach this to a trigger child object around an enemy that owns the Rigidbody2D.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class EnemyPushSeparation : MonoBehaviour
{
    [Header("Push Tuning")]
    [Tooltip("Base force used to push overlapping enemies apart.")]
    [SerializeField] private float pushStrength = 6f;

    [Tooltip("Maximum separation velocity applied per trigger update.")]
    [SerializeField] private float maxPushVelocity = 1.8f;

    [Tooltip("Small threshold that prevents zero-distance normalization issues.")]
    [SerializeField] private float minDistanceEpsilon = 0.001f;

    [Header("References")]
    [Tooltip("Rigidbody moved by this separation sensor.")]
    [SerializeField] private Rigidbody2D ownerRigidbody;

    [Tooltip("Root object used to identify the owner enemy.")]
    [SerializeField] private Transform ownerRoot;

    private Collider2D _sensor;

    // Finds the enemy Rigidbody2D used for gentle anti-overlap movement.
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

    // Handles objects that remain inside this trigger area.
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
            float angle = (ownerRoot.GetInstanceID() & 255) * Mathf.Deg2Rad * 1.40625f;
            delta = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            sqrDist = 1f;
        }

        Vector2 pushDir = delta / Mathf.Sqrt(sqrDist);
        float distanceFactor = Mathf.Clamp01(1f - Mathf.Sqrt(sqrDist)); // Push harder when enemies are closer together.
        Vector2 push = pushDir * (pushStrength * (0.35f + distanceFactor)) * Time.fixedDeltaTime;

        Vector2 targetVelocity = ownerRigidbody.velocity + push;
        ownerRigidbody.velocity = Vector2.ClampMagnitude(targetVelocity, maxPushVelocity);
    }
}
