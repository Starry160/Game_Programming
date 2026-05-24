using UnityEngine;
using System.Collections;

public class HitFeedback : MonoBehaviour
{
    [Header("Feedback Settings")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Vector3 originalLocalPosition;
    private Transform shakeTarget;
    private bool canShake = true;
    private Coroutine feedbackCoroutine;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        shakeTarget = spriteRenderer != null ? spriteRenderer.transform : transform;
        canShake = true;

        // 玩家受击如果直接抖动根节点，会推动碰撞体触发传送门/触发器。
        // 因此玩家默认只做闪红，不做位移抖动，避免物理副作用。
        if (CompareTag("Player") && shakeTarget == transform)
        {
            canShake = false;
        }

        // 敌人（如 TestEnemy 系列）同样默认禁用位移抖动，避免碰撞体抖动带来物理副作用。
        if (GetComponent<EnemyAI>() != null)
        {
            canShake = false;
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"[HitFeedback] {name} 未找到 SpriteRenderer，只有位移抖动会生效。", this);
        }

        if (canShake && shakeTarget != null)
        {
            originalLocalPosition = shakeTarget.localPosition;
        }
    }

    public void PlayFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            RestoreState();
        }

        feedbackCoroutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
        }

        float elapsed = 0f;
        float maxDuration = Mathf.Max(flashDuration, shakeDuration);
        while (elapsed < maxDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed <= shakeDuration)
            {
                if (canShake && shakeTarget != null)
                {
                    float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
                    float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
                    shakeTarget.localPosition = originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
                }
            }

            if (elapsed > flashDuration && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            yield return null;
        }

        RestoreState();
        feedbackCoroutine = null;
    }

    private void RestoreState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (canShake && shakeTarget != null)
        {
            shakeTarget.localPosition = originalLocalPosition;
        }
    }
}
