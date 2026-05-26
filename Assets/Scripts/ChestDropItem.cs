using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ChestDropItem : MonoBehaviour
{
    [Header("Scripted Move")]
    [SerializeField] protected float moveDuration = 0.5f;

    [Header("Idle Float")]
    [SerializeField] private bool enableIdleFloat = true;
    [SerializeField] private float floatAmplitude = 0.04f;
    [SerializeField] private float floatFrequency = 1.8f;

    [Header("Idle Pulse")]
    [SerializeField] private bool enableAlphaPulse = true;
    [SerializeField] private float pulseMinAlpha = 0.75f;
    [SerializeField] private float pulseMaxAlpha = 1f;
    [SerializeField] private float pulseFrequency = 1.6f;

    protected Rigidbody2D rb;
    protected Collider2D itemCollider;
    protected bool canBePicked;
    protected Coroutine moveRoutine;
    private Coroutine floatRoutine;
    private Coroutine pulseRoutine;
    private Vector3 idleAnchorPosition;
    private SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        DisablePhysicsSimulation();
    }

    public virtual void PopOut(Vector2 moveVector)
    {
        canBePicked = false;
        StopIdleFloat();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = false;
        }

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        Vector3 targetPosition = transform.position + new Vector3(moveVector.x, moveVector.y, 0f);
        moveRoutine = StartCoroutine(SmoothMoveThenEnablePickup(targetPosition));
    }

    protected virtual IEnumerator SmoothMoveThenEnablePickup(Vector3 targetPosition)
    {
        Vector3 start = transform.position;
        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(start, targetPosition, eased);
            yield return null;
        }

        transform.position = targetPosition;
        idleAnchorPosition = targetPosition;
        canBePicked = true;
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }
        StartIdleFloat();
        moveRoutine = null;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBePicked)
        {
            return;
        }

        TryPickup(other);
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (!canBePicked)
        {
            return;
        }

        TryPickup(other);
    }

    protected virtual void OnPickedByPlayer(Collider2D player)
    {
        StopIdleFloat();
        Destroy(gameObject);
    }

    protected void DisablePhysicsSimulation()
    {
        if (rb == null)
        {
            return;
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        // Keep simulation ON so trigger callbacks can still fire.
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void StartIdleFloat()
    {
        if (enableIdleFloat && floatRoutine == null)
        {
            floatRoutine = StartCoroutine(IdleFloatRoutine());
        }

        if (enableAlphaPulse && pulseRoutine == null)
        {
            pulseRoutine = StartCoroutine(AlphaPulseRoutine());
        }
    }

    private void StopIdleFloat()
    {
        if (floatRoutine == null)
        {
            // no-op
        }
        else
        {
            StopCoroutine(floatRoutine);
            floatRoutine = null;
            transform.position = idleAnchorPosition;
        }

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        ResetSpriteAlpha();
    }

    private IEnumerator IdleFloatRoutine()
    {
        while (canBePicked)
        {
            float yOffset = Mathf.Sin(Time.time * (Mathf.PI * 2f) * floatFrequency) * floatAmplitude;
            transform.position = idleAnchorPosition + new Vector3(0f, yOffset, 0f);
            yield return null;
        }

        floatRoutine = null;
    }

    private IEnumerator AlphaPulseRoutine()
    {
        float minA = Mathf.Clamp01(Mathf.Min(pulseMinAlpha, pulseMaxAlpha));
        float maxA = Mathf.Clamp01(Mathf.Max(pulseMinAlpha, pulseMaxAlpha));
        float freq = Mathf.Max(0.01f, pulseFrequency);

        while (canBePicked)
        {
            float wave = (Mathf.Sin(Time.time * (Mathf.PI * 2f) * freq) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minA, maxA, wave);
            SetSpriteAlpha(alpha);
            yield return null;
        }

        pulseRoutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color c = spriteRenderer.color;
        c.a = Mathf.Clamp01(alpha);
        spriteRenderer.color = c;
    }

    private void ResetSpriteAlpha()
    {
        SetSpriteAlpha(1f);
    }

    private void TryPickup(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        bool isPlayer = other.CompareTag("Player") ||
                        other.GetComponent<PlayerStats>() != null ||
                        other.GetComponentInParent<PlayerStats>() != null;
        if (!isPlayer)
        {
            return;
        }

        canBePicked = false;
        OnPickedByPlayer(other);
    }
}
