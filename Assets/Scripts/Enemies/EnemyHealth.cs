using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Tracks enemy heart health, hit feedback, and death fade-out.</summary>
[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float destroyDelay = 0.5f;

    [Header("Floating Heart")]
    public SpriteRenderer _heartSpriteRenderer;
    [SerializeField] private List<SpriteRenderer> _heartSpriteRenderers = new List<SpriteRenderer>();
    [SerializeField] private Sprite fullHeartSprite; // 262
    [SerializeField] private Sprite halfHeartSprite; // 263
    [SerializeField] private Sprite emptyHeartSprite; // 264

    private int _currentHealth;
    private bool _isDead;

    private MonoBehaviour _enemyAI;
    private Rigidbody2D _rb;
    private Collider2D[] _allColliders;
    private SpriteRenderer[] _allSpriteRenderers;

    // Connects this health component to the final boss controller when present.
    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        if (_enemyAI == null)
        {
            _enemyAI = GetComponent<MonsterAI>();
        }
        _rb = GetComponent<Rigidbody2D>();
        _allColliders = GetComponentsInChildren<Collider2D>(true);
        _allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        CacheHeartRenderers();
    }

    // Initializes current health after boss scaling or normal enemy defaults are known.
    private void Start()
    {
        _currentHealth = Mathf.Max(1, maxHealth);
        RefreshHeartSprite();
    }

    // Applies incoming damage and related hit feedback.
    public void TakeDamage(int amount = 1)
    {
        if (_isDead || amount <= 0)
        {
            return;
        }

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        RefreshHeartSprite();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        HitFeedback feedback = GetComponent<HitFeedback>();
        if (feedback != null)
        {
            feedback.PlayFeedback();
        }

        if (_currentHealth <= 0)
        {
            StartCoroutine(DieAfterDelay());
        }
    }

    // Updates enemy heart sprites to match current health.
    private void RefreshHeartSprite()
    {
        if (_heartSpriteRenderers == null || _heartSpriteRenderers.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _heartSpriteRenderers.Count; i++)
        {
            SpriteRenderer heartRenderer = _heartSpriteRenderers[i];
            if (heartRenderer == null)
            {
                continue;
            }

            int heartHealth = Mathf.Clamp(_currentHealth - (i * 2), 0, 2);
            if (heartHealth >= 2)
            {
                heartRenderer.sprite = fullHeartSprite;
            }
            else if (heartHealth == 1)
            {
                heartRenderer.sprite = halfHeartSprite;
            }
            else
            {
                heartRenderer.sprite = emptyHeartSprite;
            }
        }
    }

    // Disables enemy behavior, fades sprites, then destroys the enemy.
    private IEnumerator DieAfterDelay()
    {
        _isDead = true;

        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.AddKill();
        }

        if (_enemyAI != null)
        {
            _enemyAI.enabled = false;
        }

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.simulated = false;
        }

        for (int i = 0; i < _allColliders.Length; i++)
        {
            if (_allColliders[i] != null)
            {
                _allColliders[i].enabled = false;
            }
        }

        float fadeDuration = Mathf.Max(0.01f, destroyDelay);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(1f, 0f, t);
            SetAllSpritesAlpha(alpha);
            yield return null;
        }

        SetAllSpritesAlpha(0f);
        Destroy(gameObject);
    }

    // Applies a shared alpha value to every enemy sprite renderer.
    private void SetAllSpritesAlpha(float alpha)
    {
        for (int i = 0; i < _allSpriteRenderers.Length; i++)
        {
            SpriteRenderer sr = _allSpriteRenderers[i];
            if (sr == null)
            {
                continue;
            }

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    // Caches heart sprite renderers used by the floating health display.
    private void CacheHeartRenderers()
    {
        if (_heartSpriteRenderers == null)
        {
            _heartSpriteRenderers = new List<SpriteRenderer>();
        }

        _heartSpriteRenderers.RemoveAll(item => item == null);

        if (_heartSpriteRenderer != null && !_heartSpriteRenderers.Contains(_heartSpriteRenderer))
        {
            _heartSpriteRenderers.Add(_heartSpriteRenderer);
        }

        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < childRenderers.Length; i++)
        {
            SpriteRenderer sr = childRenderers[i];
            if (sr == null || sr.gameObject == gameObject)
            {
                continue;
            }

            if (!sr.gameObject.name.Contains("FloatingHeart"))
            {
                continue;
            }

            if (!_heartSpriteRenderers.Contains(sr))
            {
                _heartSpriteRenderers.Add(sr);
            }
        }

        _heartSpriteRenderers.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            return a.transform.localPosition.x.CompareTo(b.transform.localPosition.x);
        });
    }
}
