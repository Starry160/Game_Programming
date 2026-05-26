using System.Collections;
using UnityEngine;

/// <summary>敌人生命值（心形 UI）、受击与死亡淡出销毁。</summary>
[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float destroyDelay = 0.5f;

    [Header("Floating Heart")]
    public SpriteRenderer _heartSpriteRenderer;
    [SerializeField] private Sprite fullHeartSprite; // 262
    [SerializeField] private Sprite halfHeartSprite; // 263
    [SerializeField] private Sprite emptyHeartSprite; // 264

    private int _currentHealth;
    private bool _isDead;

    private EnemyAI _enemyAI;
    private Rigidbody2D _rb;
    private Collider2D[] _allColliders;
    private SpriteRenderer[] _allSpriteRenderers;

    // 缓存 AI、刚体与所有碰撞体/渲染器。
    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _rb = GetComponent<Rigidbody2D>();
        _allColliders = GetComponentsInChildren<Collider2D>(true);
        _allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    // 初始化血量并显示心形精灵。
    private void Start()
    {
        _currentHealth = Mathf.Max(1, maxHealth);
        RefreshHeartSprite();
    }

    // 扣血、播放受击反馈，血量为 0 时进入死亡流程。
    public void TakeDamage(int amount = 1)
    {
        if (_isDead || amount <= 0)
        {
            return;
        }

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        RefreshHeartSprite();

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

    // 根据当前血量切换满/半/空心精灵。
    private void RefreshHeartSprite()
    {
        if (_heartSpriteRenderer == null)
        {
            return;
        }

        if (_currentHealth >= 2)
        {
            _heartSpriteRenderer.sprite = fullHeartSprite;
        }
        else if (_currentHealth == 1)
        {
            _heartSpriteRenderer.sprite = halfHeartSprite;
        }
        else
        {
            _heartSpriteRenderer.sprite = emptyHeartSprite;
        }
    }

    // 禁用 AI/碰撞，淡出后销毁。
    private IEnumerator DieAfterDelay()
    {
        _isDead = true;

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

    // 设置敌人所有子 Sprite 的透明度。
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
}
