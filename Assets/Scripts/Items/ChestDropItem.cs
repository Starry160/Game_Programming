using System.Collections;
using UnityEngine;

/// <summary>宝箱掉落物基类：脚本位移、 idle 浮动/闪烁、触发拾取。</summary>
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

    // 缓存组件并关闭物理模拟（保留 trigger）。
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        DisablePhysicsSimulation();
    }

    // 由宝箱调用：沿 moveVector 平滑移动后允许拾取。
    public virtual void PopOut(Vector2 moveVector)
    {
        canBePicked = false;
        StopIdleFloat();
        if (itemCollider != null)
        {
            // Keep pickup items non-blocking during the whole lifecycle.
            itemCollider.isTrigger = true;
        }

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        Vector3 targetPosition = transform.position + new Vector3(moveVector.x, moveVector.y, 0f);
        moveRoutine = StartCoroutine(SmoothMoveThenEnablePickup(targetPosition));
    }

    // 缓动到目标点，结束后开 trigger 与 idle 动画。
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

    // 进入触发区尝试拾取。
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBePicked)
        {
            return;
        }

        TryPickup(other);
    }

    // 停留在触发区内继续尝试拾取。
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (!canBePicked)
        {
            return;
        }

        TryPickup(other);
    }

    // 拾取成功：子类先应用效果，再销毁自身。
    protected virtual void OnPickedByPlayer(Collider2D player)
    {
        StopIdleFloat();
        Destroy(gameObject);
    }

    // 设为 Kinematic 且 simulated=true 以接收 Trigger。
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

    // 启动上下浮动与透明度脉冲。
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

    // 停止 idle 效果并复位位置/透明度。
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

    // 正弦上下浮动。
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

    // 透明度周期性变化。
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

    // 设置 Sprite 透明度。
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

    // 恢复完全不透明。
    private void ResetSpriteAlpha()
    {
        SetSpriteAlpha(1f);
    }

    // 检测 Player 标签或 PlayerStats 后调用 OnPickedByPlayer。
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
