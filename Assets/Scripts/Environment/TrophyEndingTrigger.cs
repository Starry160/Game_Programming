using System.Collections;
using UnityEngine;

/// <summary>
/// Final Room 奖杯交互：玩家靠近后奖杯放大，并弹出结算面板结束本局。
/// </summary>
public class TrophyEndingTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float triggerDistance = 1.25f;

    [Header("Trophy Scale")]
    [SerializeField] private float scaleMultiplier = 1.8f;
    [SerializeField] private float scaleDuration = 1.1f;
    [SerializeField] private float panelDelayAfterScale = 0.25f;
    [Header("FX During Scale")]
    [SerializeField] private bool enablePulseGlow = true;
    [SerializeField] private Color glowColor = new Color(1f, 0.95f, 0.6f, 1f);
    [SerializeField] private float glowPulseSpeed = 7f;
    [SerializeField] private float glowPulseStrength = 0.2f;
    [Header("Result Panel")]
    [SerializeField] private string hudCanvasObjectName = "PlayerHUDCanvas";
    [SerializeField] private GameOverPanel gameOverPanel;

    private Transform _playerTransform;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseScale;
    private Vector3 _baseLocalPosition;
    private bool _isEndingStarted;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _baseLocalPosition = transform.localPosition;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        TryFindPlayer();
    }

    private void Update()
    {
        if (_isEndingStarted)
        {
            return;
        }

        if (_playerTransform == null)
        {
            TryFindPlayer();
            return;
        }

        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        if (distance <= Mathf.Max(0.05f, triggerDistance))
        {
            StartCoroutine(PlayEndingRoutine());
        }
    }

    private IEnumerator PlayEndingRoutine()
    {
        _isEndingStarted = true;

        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.StopTimer();
        }

        DisablePlayerControl();

        Vector3 targetScale = _baseScale * Mathf.Max(1f, scaleMultiplier);
        float duration = Mathf.Max(0.05f, scaleDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float pulse = enablePulseGlow
                ? Mathf.Sin(elapsed * Mathf.Max(0.1f, glowPulseSpeed) * Mathf.PI * 2f) * Mathf.Max(0f, glowPulseStrength)
                : 0f;
            float scalePulseFactor = 1f + pulse * 0.08f;
            transform.localScale = Vector3.Lerp(_baseScale, targetScale, t) * scalePulseFactor;

            if (enablePulseGlow && _spriteRenderer != null)
            {
                float glowLerp = Mathf.Clamp01(t + Mathf.Max(0f, pulse) * 0.6f);
                _spriteRenderer.color = Color.Lerp(Color.white, glowColor, glowLerp);
            }
            yield return null;
        }
        transform.localScale = targetScale;
        transform.localPosition = _baseLocalPosition;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white;
        }

        if (panelDelayAfterScale > 0f)
        {
            yield return new WaitForSeconds(panelDelayAfterScale);
        }

        GameOverPanel panel = ResolveGameOverPanel();
        if (panel != null)
        {
            panel.PrepareForRuntime();
            panel.ShowPanel(GameOverPanel.ResultType.Victory);
        }
        else
        {
            Debug.LogWarning("[TrophyEndingTrigger] 未找到 GameOverPanel，无法显示结算界面。", this);
        }
    }

    private void DisablePlayerControl()
    {
        if (_playerTransform == null)
        {
            return;
        }

        PlayerController controller = _playerTransform.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        PlayerAttack attack = _playerTransform.GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }

        PlayerFacing facing = _playerTransform.GetComponent<PlayerFacing>();
        if (facing != null)
        {
            facing.enabled = false;
        }

        Rigidbody2D rb = _playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    private GameOverPanel ResolveGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            return gameOverPanel;
        }

        // Prefer explicit HUD path first, because panel is usually nested and may be inactive.
        GameObject hudRoot = GameObject.Find(hudCanvasObjectName);
        if (hudRoot != null)
        {
            gameOverPanel = hudRoot.GetComponentInChildren<GameOverPanel>(true);
            if (gameOverPanel != null)
            {
                return gameOverPanel;
            }
        }

        gameOverPanel = FindObjectOfType<GameOverPanel>(true);
        return gameOverPanel;
    }
}
