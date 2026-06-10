using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

/// <summary>Manages player health, shield, invincibility, potion effects, and HUD updates.</summary>
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
    [Tooltip("Delay after taking damage before shield regeneration starts.")]
    public float shieldRegenDelay = 3f;
    [Tooltip("Time between each shield regeneration tick.")]
    public float shieldRegenInterval = 5f;

    [Header("UI References")]
    public Image healthFillImage;
    public Image shieldFillImage;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;
    [Header("Debug Safety")]
    [Tooltip("Debug guard that detects abnormal movement after taking damage.")]
    public bool enablePostHitTeleportGuard = false;
    [Tooltip("Minimum movement distance treated as abnormal after a hit.")]
    public float teleportDistanceThreshold = 6f;
    [Tooltip("Time window for detecting abnormal movement after a hit.")]
    public float teleportDetectWindow = 0.8f;
    [Tooltip("Restore the pre-hit position when abnormal movement is detected.")]
    public bool restoreOnTeleportDetected = true;
    [Header("Invincibility VFX")]
    [Tooltip("Enable gold flashing during temporary invincibility.")]
    public bool enableGoldInvincibilityFlash = true;
    [Tooltip("Gold flash color used during temporary invincibility.")]
    public Color invincibleFlashColor = new Color(1f, 0.85f, 0.2f, 0.85f);
    [Tooltip("Flash interval used during temporary invincibility.")]
    public float invincibleFlashInterval = 0.08f;
    [Tooltip("Enable slight scale pulsing during temporary invincibility.")]
    public bool enableInvincibleScalePulse = true;
    [Tooltip("Minimum scale multiplier during invincibility pulsing.")]
    public float invincibleScaleMin = 1f;
    [Tooltip("Maximum scale multiplier during invincibility pulsing.")]
    public float invincibleScaleMax = 1.05f;
    [Tooltip("Scale pulse frequency during temporary invincibility.")]
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

    // Watches scene loads so HUD references can be rebound after transitions.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Stops watching scene loads while the player stats component is disabled.
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Restores persistent health data and prepares HUD, shield, and hit feedback state.
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
            // Max health persists across scene transitions and class room changes.
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
            // Current health is clamped in case max health changed before this scene loaded.
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

    // Rebinds HUD and result panel references after scene changes.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAutoBindUIReferences();
        UpdateUI();
    }

    // Runs shield regeneration timing while the player is alive.
    private void Update()
    {
        HandleShieldRegeneration();
    }

    // Regenerates shield after the player has avoided damage long enough.
    private void HandleShieldRegeneration()
    {
        if (currentShield >= maxShield)
        {
            return;
        }

        // Shield only starts regenerating after the post-hit delay has elapsed.
        if (Time.time < nextShieldRegenTime)
        {
            return;
        }

        currentShield = Mathf.Min(maxShield, currentShield + 1f);
        nextShieldRegenTime = Time.time + shieldRegenInterval;
        UpdateUI();
    }

    // Synchronizes health and shield bars with the current stat values.
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

    // Finds health and shield HUD references by scene hierarchy or name.
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

    // Consumes shield first, then health, and starts hit feedback and invulnerability.
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

        nextShieldRegenTime = Time.time + shieldRegenDelay;

        // Shield absorbs incoming damage first; any overflow is passed to health.
        if (currentShield > 0f)
        {
            if (amount <= currentShield)
            {
                currentShield -= amount;
                Debug.Log("Player took damage. Current health: " + currentHealth);
                UpdateUI();
                StartInvulnerability();
                return;
            }

            amount -= currentShield;
            currentShield = 0f;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        GlobalData.persistedHealth = currentHealth;
        Debug.Log("Player took damage. Current health: " + currentHealth);

        UpdateUI();

        // Death flow is triggered immediately once health reaches zero.
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

    // Applies direct health damage that bypasses shield protection.
    public void TakeTrueDamage(float amount)
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerHit();
        }

        HitFeedback feedback = GetComponent<HitFeedback>();
        if (feedback != null)
        {
            feedback.PlayFeedback();
        }

        nextShieldRegenTime = Time.time + shieldRegenDelay;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        GlobalData.persistedHealth = currentHealth;
        Debug.Log("Player took damage. Current health: " + currentHealth);

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

    // Restores player health without exceeding max health.
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

    // Starts the potion invincibility effect without overwriting player facing.
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

        // Potion invincibility replaces the normal post-hit invulnerability visual effect.
        temporaryInvincibilityCoroutine = StartCoroutine(TemporaryInvincibilityRoutine(duration));
    }

    // Raises max health and heals by the same amount.
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

    // Raises max shield and fills the gained shield amount.
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

    // Starts short post-hit invulnerability and red flash feedback.
    private void StartInvulnerability()
    {
        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
        }

        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
    }

    // Runs red flash feedback during post-hit invulnerability.
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

    // Runs gold flash and scale pulse during potion invincibility.
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

        // Keep the original color as the base so the flash restores cleanly after each pulse.
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
                // Preserve facing sign while pulsing scale, otherwise invincibility could flip the sprite.
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

    // Stops player control and prepares the delayed game-over result panel.
    private void HandlePlayerDeath()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        isInvulnerable = false;
        isTemporarilyInvincible = false;

        // Stop every active protection coroutine so the death state cannot be overwritten later.
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

    // Disables player movement and attack scripts during the ending flow.
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

    // Waits briefly before showing the game-over result panel.
    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        if (_gameOverPanel == null)
        {
            _gameOverPanel = FindObjectOfType<GameOverPanel>(true);
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.ShowPanel(GameOverPanel.ResultType.Death);
        }

        _showGameOverCoroutine = null;
    }

    // Starts debug monitoring for abnormal movement after damage.
    private void StartPostHitTeleportGuard(Vector3 preHitPosition, string preHitScene)
    {
        if (!enablePostHitTeleportGuard)
        {
            return;
        }

        StartCoroutine(PostHitTeleportGuardRoutine(preHitPosition, preHitScene));
    }

    // Detects and optionally restores suspicious post-hit movement.
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
                Debug.LogWarning($"[HitTeleportGuard] Scene changed after hit: {preHitScene} -> {currentScene}");
                yield break;
            }

            float movedDistance = Vector3.Distance(transform.position, preHitPosition);
            if (movedDistance >= teleportDistanceThreshold)
            {
                Debug.LogWarning(
                    $"[HitTeleportGuard] Abnormal movement after hit detected, before={preHitPosition}, after={transform.position}, dist={movedDistance:F2}");
                LogNearestPortalAndDoorHints();

                if (restoreOnTeleportDetected)
                {
                    transform.position = preHitPosition;
                    if (rb != null)
                    {
                        rb.velocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                    }
                    Debug.LogWarning("[HitTeleportGuard] Restored the pre-hit position for debug protection.");
                }
                yield break;
            }

            yield return null;
        }
    }

    // Logs nearby portal and door distances for debugging movement issues.
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
