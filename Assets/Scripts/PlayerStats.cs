using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

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
    public float shieldRegenDelay = 5f;
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

    private float nextShieldRegenTime;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Coroutine invulnerabilityCoroutine;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        TryAutoBindUIReferences();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAutoBindUIReferences();
        UpdateUI();
    }

    private void Update()
    {
        HandleShieldRegeneration();
    }

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

    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        if (isInvulnerable)
        {
            return;
        }

        Vector3 preHitPosition = transform.position;
        string preHitScene = SceneManager.GetActiveScene().name;

        HitFeedback feedback = GetComponent<HitFeedback>();
        if (feedback != null)
        {
            feedback.PlayFeedback();
        }

        // 任何受伤都会打断护盾恢复计时，重新等待 5 秒（可在 Inspector 调整）。
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
            Debug.LogWarning("【测试安全阀】玩家理论上死掉了！但为了防止场景重启，现在强行满血复活进行调试！");
            currentHealth = maxHealth;
            GlobalData.persistedHealth = currentHealth;
            UpdateUI();
            return;
        }

        StartInvulnerability();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        GlobalData.persistedHealth = currentHealth;
        UpdateUI();
    }

    private void StartInvulnerability()
    {
        if (invulnerabilityCoroutine != null)
        {
            StopCoroutine(invulnerabilityCoroutine);
        }

        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
    }

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

    private void StartPostHitTeleportGuard(Vector3 preHitPosition, string preHitScene)
    {
        if (!enablePostHitTeleportGuard)
        {
            return;
        }

        StartCoroutine(PostHitTeleportGuardRoutine(preHitPosition, preHitScene));
    }

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
