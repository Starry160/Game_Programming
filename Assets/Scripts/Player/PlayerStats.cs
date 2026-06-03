using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

/// <summary>玩家生命/护盾、受击无敌、药水增益与 HUD 同步。</summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float invulnerabilityDuration = 1.5f;
    public float flashInterval = 0.1f;

    [Header("Shield")]
    public float maxShield = 50f;
    public float currentShield;
    [Tooltip("受到伤害后，至少经过这段时间未再受伤，护盾才会开始恢复。")]
    public float shieldRegenDelay = 3f;
    [Tooltip("护盾每次恢复 1 点之间的时间间隔。")]
    public float shieldRegenInterval = 5f;

    [Header("UI References")]
    public Image healthFillImage;
    public Image shieldFillImage;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;
    [Header("Debug Safety")]
    [Tooltip("调试期：受击后短时间内检测异常坐标跳变，并打印日志。")]
    public bool enablePostHitTeleportGuard = false;
    [Tooltip("判定为异常重置的最小位移距离。")]
    public float teleportDistanceThreshold = 6f;
    [Tooltip("受击后监控异常位移的时间窗口。")]
    public float teleportDetectWindow = 0.8f;
    [Tooltip("检测到异常位移时是否强制拉回受击前位置。")]
    public bool restoreOnTeleportDetected = true;
    [Header("Invincibility VFX")]
    [Tooltip("临时无敌期间是否启用金色闪烁效果。")]
    public bool enableGoldInvincibilityFlash = true;
    [Tooltip("无敌闪烁时的金色。")]
    public Color invincibleFlashColor = new Color(1f, 0.85f, 0.2f, 0.85f);
    [Tooltip("无敌闪烁切换间隔（秒）。")]
    public float invincibleFlashInterval = 0.08f;
    [Tooltip("临时无敌期间是否启用轻微缩放呼吸效果。")]
    public bool enableInvincibleScalePulse = true;
    [Tooltip("无敌缩放呼吸最小倍率（相对基础缩放）。")]
    public float invincibleScaleMin = 1f;
    [Tooltip("无敌缩放呼吸最大倍率（相对基础缩放）。")]
    public float invincibleScaleMax = 1.05f;
    [Tooltip("无敌缩放呼吸频率（每秒循环次数）。")]
    public float invincibleScaleFrequency = 1f;

    private float nextShieldRegenTime;
    private bool isInvulnerable = false;
    private bool isTemporarilyInvincible = false;
    private bool _isDead = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Coroutine invulnerabilityCoroutine;
    private Coroutine temporaryInvincibilityCoroutine;
    private Coroutine _showGameOverCoroutine;
    private GameOverPanel _gameOverPanel;

    // 订阅场景加载以重新绑定 UI。
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 取消场景加载订阅。
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 从 GlobalData 恢复生命/护盾并初始化 UI。
    private void Start()
    {
        TryAutoBindUIReferences();
        _gameOverPanel = FindObjectOfType<GameOverPanel>(true);
        if (_gameOverPanel != null)
        {
            _gameOverPanel.PrepareForRuntime();
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (GlobalData.hasPersistedMaxHealth)
        {
            maxHealth = Mathf.Max(1f, GlobalData.persistedMaxHealth);
        }
        else
        {
            GlobalData.hasPersistedMaxHealth = true;
            GlobalData.persistedMaxHealth = maxHealth;
        }

        if (GlobalData.hasPersistedMaxShield)
        {
            maxShield = Mathf.Max(0f, GlobalData.persistedMaxShield);
        }
        else
        {
            GlobalData.hasPersistedMaxShield = true;
            GlobalData.persistedMaxShield = maxShield;
        }

        if (GlobalData.hasPersistedHealth)
        {
            currentHealth = Mathf.Clamp(GlobalData.persistedHealth, 0f, maxHealth);
        }
        else
        {
            currentHealth = maxHealth;
            GlobalData.hasPersistedHealth = true;
        }

        GlobalData.persistedHealth = currentHealth;
        currentShield = maxShield;
        nextShieldRegenTime = Time.time + shieldRegenDelay;
        UpdateUI();
    }

    // 切场景后重新查找 HUD 引用。
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAutoBindUIReferences();
        UpdateUI();
    }

    // 每帧处理护盾自动恢复。
    private void Update()
    {
        HandleShieldRegeneration();
    }

    // 受伤冷却后按间隔恢复护盾。
    private void HandleShieldRegeneration()
    {
        if (currentShield >= maxShield)
        {
            return;
        }

        if (Time.time < nextShieldRegenTime)
        {
            return;
        }

        currentShield = Mathf.Min(maxShield, currentShield + 1f);
        nextShieldRegenTime = Time.time + shieldRegenInterval;
        UpdateUI();
    }

    // 刷新血条/盾条填充与数值文本。
    public void UpdateUI()
    {
        if (healthText == null || shieldText == null || healthFillImage == null || shieldFillImage == null)
        {
            TryAutoBindUIReferences();
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = maxHealth > 0f ? currentHealth / maxHealth : 0f;
        }

        if (shieldFillImage != null)
        {
            shieldFillImage.fillAmount = maxShield > 0f ? currentShield / maxShield : 0f;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }

        if (shieldText != null)
        {
            shieldText.text = $"{Mathf.CeilToInt(currentShield)} / {Mathf.CeilToInt(maxShield)}";
        }
    }

    // 按路径或名称自动查找 HUD 组件。
    private void TryAutoBindUIReferences()
    {
        if (healthText == null)
        {
            GameObject healthTextObj = GameObject.Find("PlayerHUDCanvas/StatusPanel/HealthText");
            if (healthTextObj != null)
            {
                healthText = healthTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
        if (healthText == null)
        {
            healthText = FindActiveComponentByName<TextMeshProUGUI>("HealthText");
        }

        if (shieldText == null)
        {
            GameObject shieldTextObj = GameObject.Find("PlayerHUDCanvas/StatusPanel/ShieldText");
            if (shieldTextObj != null)
            {
                shieldText = shieldTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
        if (shieldText == null)
        {
            shieldText = FindActiveComponentByName<TextMeshProUGUI>("ShieldText");
        }

        if (healthFillImage == null)
        {
            GameObject healthFillObj = GameObject.Find("PlayerHUDCanvas/StatusPanel/HealthBar_BG/HealthFill");
            if (healthFillObj != null)
            {
                healthFillImage = healthFillObj.GetComponent<Image>();
            }
        }
        if (healthFillImage == null)
        {
            healthFillImage = FindActiveComponentByName<Image>("HealthFill");
        }

        if (shieldFillImage == null)
        {
            GameObject shieldFillObj = GameObject.Find("PlayerHUDCanvas/StatusPanel/ShieldBar_BG/ShieldFill");
            if (shieldFillObj != null)
            {
                shieldFillImage = shieldFillObj.GetComponent<Image>();
            }
        }
        if (shieldFillImage == null)
        {
            shieldFillImage = FindActiveComponentByName<Image>("ShieldFill");
        }
    }

    // 在场景中按 GameObject 名称查找激活的组件。
    private T FindActiveComponentByName<T>(string objectName) where T : Component
    {
        T[] components = FindObjectsOfType<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];
            if (component == null || component.gameObject == null)
            {
                continue;
            }

            if (!string.Equals(component.gameObject.name, objectName, StringComparison.Ordinal))
            {
                continue;
            }

            if (!component.gameObject.activeInHierarchy)
            {
                continue;
            }

            return component;
        }

        return null;
    }

    // 受到伤害：先扣护盾再扣血，触发无敌与受击反馈。
    public void TakeDamage(float amount)
    {
        if (_isDead)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        if (isInvulnerable || isTemporarilyInvincible)
        {
            return;
        }

        Vector3 preHitPosition = transform.position;
        string preHitScene = SceneManager.GetActiveScene().name;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHit();
        }

        HitFeedback feedback = GetComponent<HitFeedback>();
        if (feedback != null)
        {
            feedback.PlayFeedback();
        }

        // 任何受伤都会打断护盾恢复计时，重新等待 shieldRegenDelay（可在 Inspector 调整）。
        nextShieldRegenTime = Time.time + shieldRegenDelay;

        // 优先扣除护盾；仅当护盾不足时，剩余伤害才会扣生命值。
        if (currentShield > 0f)
        {
            if (amount <= currentShield)
            {
                currentShield -= amount;
                Debug.Log("玩家受击，当前血量：" + currentHealth);
                UpdateUI();
                StartInvulnerability();
                return;
            }

            amount -= currentShield;
            currentShield = 0f;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        GlobalData.persistedHealth = currentHealth;
        Debug.Log("玩家受击，当前血量：" + currentHealth);

        UpdateUI();

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            GlobalData.persistedHealth = currentHealth;
            UpdateUI();
            HandlePlayerDeath();
            return;
        }

        StartInvulnerability();
    }

    // 治疗并同步 GlobalData 与 UI。
    public void Heal(float amount)
    {
        if (_isDead)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        GlobalData.persistedHealth = currentHealth;
        UpdateUI();
    }

    // 授予临时无敌（金闪 + 缩放呼吸，不覆盖朝向符号）。
    public void GrantTemporaryInvincibility(float duration)
    {
        if (_isDead)
        {
            return;
        }

        if (duration <= 0f)
        {
            return;
        }

        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
            invulnerabilityCoroutine = null;
            isInvulnerable = false;
        }

        if (temporaryInvincibilityCoroutine != null)
        {
            StopCoroutine(temporaryInvincibilityCoroutine);
        }

        temporaryInvincibilityCoroutine = StartCoroutine(TemporaryInvincibilityRoutine(duration));
    }

    // 生命药水：提升上限并回复等量生命。
    public void IncreaseMaxHealthAndHeal(float amount)
    {
        if (_isDead)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        maxHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        GlobalData.hasPersistedMaxHealth = true;
        GlobalData.persistedMaxHealth = maxHealth;
        GlobalData.hasPersistedHealth = true;
        GlobalData.persistedHealth = currentHealth;

        UpdateUI();
    }

    // 护盾药水：提升护盾上限并填满增量。
    public void IncreaseMaxShieldAndFill(float amount)
    {
        if (_isDead)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        maxShield += amount;
        currentShield = Mathf.Min(maxShield, currentShield + amount);

        GlobalData.hasPersistedMaxShield = true;
        GlobalData.persistedMaxShield = maxShield;

        UpdateUI();
    }

    // 启动受击后短暂红闪无敌。
    private void StartInvulnerability()
    {
        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
        }

        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
    }

    // 受击无敌协程：红白闪烁。
    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float timer = 0f;

        while (timer < invulnerabilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvulnerable = false;
        invulnerabilityCoroutine = null;
    }

    // 药水无敌协程：金闪 + 缩放脉冲。
    private IEnumerator TemporaryInvincibilityRoutine(float duration)
    {
        isTemporarilyInvincible = true;
        float timer = 0f;
        float flashTimer = 0f;
        bool showFlashColor = false;
        Transform visualTarget = spriteRenderer != null ? spriteRenderer.transform : transform;
        Vector3 initialScale = visualTarget.localScale;
        Vector3 baseAbsScale = new Vector3(
            Mathf.Abs(initialScale.x),
            Mathf.Abs(initialScale.y),
            Mathf.Abs(initialScale.z));
        float minScale = Mathf.Max(0.01f, Mathf.Min(invincibleScaleMin, invincibleScaleMax));
        float maxScale = Mathf.Max(minScale, Mathf.Max(invincibleScaleMin, invincibleScaleMax));
        float scaleFreq = Mathf.Max(0.01f, invincibleScaleFrequency);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        while (timer < duration)
        {
            float dt = Time.deltaTime;
            timer += dt;

            if (enableGoldInvincibilityFlash && spriteRenderer != null)
            {
                flashTimer += dt;
                if (flashTimer >= Mathf.Max(0.01f, invincibleFlashInterval))
                {
                    flashTimer = 0f;
                    showFlashColor = !showFlashColor;
                    spriteRenderer.color = showFlashColor ? invincibleFlashColor : originalColor;
                }
            }

            if (enableInvincibleScalePulse && visualTarget != null)
            {
                float wave = (Mathf.Sin(timer * (Mathf.PI * 2f) * scaleFreq) + 1f) * 0.5f;
                float scaleMul = Mathf.Lerp(minScale, maxScale, wave);
                float facingSignX = Mathf.Sign(visualTarget.localScale.x);
                if (Mathf.Approximately(facingSignX, 0f))
                {
                    facingSignX = 1f;
                }

                visualTarget.localScale = new Vector3(
                    facingSignX * baseAbsScale.x * scaleMul,
                    baseAbsScale.y * scaleMul,
                    baseAbsScale.z);
            }

            yield return null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        if (visualTarget != null)
        {
            float facingSignX = Mathf.Sign(visualTarget.localScale.x);
            if (Mathf.Approximately(facingSignX, 0f))
            {
                facingSignX = 1f;
            }

            visualTarget.localScale = new Vector3(
                facingSignX * baseAbsScale.x,
                baseAbsScale.y,
                baseAbsScale.z);
        }

        isTemporarilyInvincible = false;
        temporaryInvincibilityCoroutine = null;
    }

    private void HandlePlayerDeath()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        isInvulnerable = false;
        isTemporarilyInvincible = false;

        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
            invulnerabilityCoroutine = null;
        }

        if (temporaryInvincibilityCoroutine != null)
        {
            StopCoroutine(temporaryInvincibilityCoroutine);
            temporaryInvincibilityCoroutine = null;
        }

        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.StopTimer();
        }

        DisablePlayerControl();

        if (_showGameOverCoroutine != null)
        {
            StopCoroutine(_showGameOverCoroutine);
        }
        _showGameOverCoroutine = StartCoroutine(ShowGameOverAfterDelay());
    }

    private void DisablePlayerControl()
    {
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        if (_gameOverPanel == null)
        {
            _gameOverPanel = FindObjectOfType<GameOverPanel>(true);
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.ShowPanel();
        }

        _showGameOverCoroutine = null;
    }

    // 调试：受击后监测异常位移。
    private void StartPostHitTeleportGuard(Vector3 preHitPosition, string preHitScene)
    {
        if (!enablePostHitTeleportGuard)
        {
            return;
        }

        StartCoroutine(PostHitTeleportGuardRoutine(preHitPosition, preHitScene));
    }

    // 调试协程：超阈值则拉回受击前位置。
    private IEnumerator PostHitTeleportGuardRoutine(Vector3 preHitPosition, string preHitScene)
    {
        float timer = 0f;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        while (timer < teleportDetectWindow)
        {
            timer += Time.deltaTime;

            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.Equals(currentScene, preHitScene, StringComparison.Ordinal))
            {
                Debug.LogWarning($"[HitTeleportGuard] 受击后场景发生变化: {preHitScene} -> {currentScene}");
                yield break;
            }

            float movedDistance = Vector3.Distance(transform.position, preHitPosition);
            if (movedDistance >= teleportDistanceThreshold)
            {
                Debug.LogWarning(
                    $"[HitTeleportGuard] 检测到受击后异常位移，before={preHitPosition}, after={transform.position}, dist={movedDistance:F2}");
                LogNearestPortalAndDoorHints();

                if (restoreOnTeleportDetected)
                {
                    transform.position = preHitPosition;
                    if (rb != null)
                    {
                        rb.velocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                    }
                    Debug.LogWarning("[HitTeleportGuard] 已强制拉回受击前位置（调试保护）。");
                }
                yield break;
            }

            yield return null;
        }
    }

    // 调试：打印最近传送门/门距离。
    private void LogNearestPortalAndDoorHints()
    {
        Vector3 playerPos = transform.position;

        LevelPortal[] portals = FindObjectsOfType<LevelPortal>(true);
        float nearestPortalDist = float.MaxValue;
        string nearestPortalName = "none";
        for (int i = 0; i < portals.Length; i++)
        {
            LevelPortal portal = portals[i];
            if (portal == null) continue;
            float d = Vector3.Distance(playerPos, portal.transform.position);
            if (d < nearestPortalDist)
            {
                nearestPortalDist = d;
                nearestPortalName = portal.name;
            }
        }

        DoorController[] doors = FindObjectsOfType<DoorController>(true);
        float nearestDoorDist = float.MaxValue;
        string nearestDoorName = "none";
        for (int i = 0; i < doors.Length; i++)
        {
            DoorController door = doors[i];
            if (door == null) continue;
            float d = Vector3.Distance(playerPos, door.transform.position);
            if (d < nearestDoorDist)
            {
                nearestDoorDist = d;
                nearestDoorName = door.name;
            }
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        bool rbSimulated = rb != null && rb.simulated;
        Debug.LogWarning(
            $"[HitTeleportGuard] nearestPortal={nearestPortalName} ({nearestPortalDist:F2}), nearestDoor={nearestDoorName} ({nearestDoorDist:F2}), rb.simulated={rbSimulated}");
    }
}
