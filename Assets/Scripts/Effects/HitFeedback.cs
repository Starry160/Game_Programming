using UnityEngine;
using System.Collections;

/// <summary>
/// Adds short visual feedback when a character is hit. It flashes sprites and can shake the
/// object locally, while restoring the original color and position after the effect.
/// </summary>
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

    // Captures renderers and original colors before hit flash effects modify them.
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        shakeTarget = spriteRenderer != null ? spriteRenderer.transform : transform;
        canShake = true;

        if (CompareTag("Player") && shakeTarget == transform)
        {
            canShake = false;
        }

        if (GetComponent<EnemyAI>() != null || GetComponent<MonsterAI>() != null)
        {
            canShake = false;
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        if (canShake && shakeTarget != null)
        {
            originalLocalPosition = shakeTarget.localPosition;
        }
    }

    // Starts the hit feedback routine and interrupts any previous feedback.
    public void PlayFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            RestoreState();
        }

        if (canShake && shakeTarget != null)
        {
            originalLocalPosition = shakeTarget.localPosition;
        }

        feedbackCoroutine = StartCoroutine(HitRoutine());
    }

    // Runs flash and optional shake feedback for a short duration.
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

    // Restores the original sprite color and local position after feedback.
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
