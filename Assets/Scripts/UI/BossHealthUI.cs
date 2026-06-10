using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the final boss name and heart-based health meter.
/// </summary>
public class BossHealthUI : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private FinalBossController targetBoss;
    [SerializeField] private string bossObjectName = "FinalBoss";
    [SerializeField] private RoomController targetRoom;
    [SerializeField] private string roomObjectName = "TestRoom_01";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private string bossDisplayName = "Mecha-stone Golem";
    [SerializeField] private List<Image> heartImages = new List<Image>();
    [SerializeField] private bool autoExpandHeartSlots = true;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private float hideFadeDuration = 0.25f;

    private readonly List<Image> runtimeHeartImages = new List<Image>();
    private CanvasGroup _canvasGroup;
    private Coroutine _hideFadeRoutine;

    // Hooks scene-load events so the boss bar can rebind after level transitions.
    private void OnEnable()
    {
        EnsureCanvasGroup();
        SetPanelVisible(false);
        TryBindRoom();
        SubscribeRoomEvents();
        TryBindBoss();
        SubscribeBossEvents();
        RefreshImmediate();
    }

    // Removes scene-load callbacks while the boss bar is disabled.
    private void OnDisable()
    {
        StopHideFadeRoutine();
        UnsubscribeRoomEvents();
        UnsubscribeBossEvents();
    }

    // Clears boss-health subscriptions when the UI object is destroyed.
    private void OnDestroy()
    {
        CleanupRuntimeHearts();
    }

    // Finds the active final boss and prepares the health bar for runtime display.
    private void Start()
    {
        if (bossNameText != null)
        {
            bossNameText.text = bossDisplayName;
        }

        UpdateVisibilityByRoomState();
        RefreshImmediate();
    }

    // Finds the final boss health source for the boss UI.
    private void TryBindBoss()
    {
        if (targetBoss != null)
        {
            return;
        }

        GameObject boss = GameObject.Find(bossObjectName);
        if (boss != null)
        {
            targetBoss = boss.GetComponent<FinalBossController>();
        }
    }

    // Subscribes the UI to boss health changes.
    private void SubscribeBossEvents()
    {
        if (targetBoss == null)
        {
            return;
        }

        targetBoss.HealthChanged -= HandleHealthChanged;
        targetBoss.HealthChanged += HandleHealthChanged;
    }

    // Removes boss health event subscriptions.
    private void UnsubscribeBossEvents()
    {
        if (targetBoss == null)
        {
            return;
        }

        targetBoss.HealthChanged -= HandleHealthChanged;
    }

    // Refreshes the heart display after boss health changes.
    private void HandleHealthChanged(float current, float max)
    {
        RefreshHeartSlotsByMax(max);
        RefreshHearts(current);
    }

    // Shows the boss UI when the linked room battle begins.
    private void HandleRoomBattleStarted(RoomController room)
    {
        StopHideFadeRoutine();
        SetPanelVisible(true);
        RefreshImmediate();
    }

    // Updates boss UI visibility and hearts without waiting for a fade.
    private void RefreshImmediate()
    {
        TryBindBoss();
        if (targetBoss == null)
        {
            return;
        }

        RefreshHeartSlotsByMax(targetBoss.MaxHealth);
        RefreshHearts(targetBoss.CurrentHealth);
    }

    // Finds the room controller that controls boss UI visibility.
    private void TryBindRoom()
    {
        if (targetRoom != null)
        {
            return;
        }

        GameObject roomObject = GameObject.Find(roomObjectName);
        if (roomObject != null)
        {
            targetRoom = roomObject.GetComponent<RoomController>();
        }
    }

    // Subscribes to room battle and clear events.
    private void SubscribeRoomEvents()
    {
        if (targetRoom == null)
        {
            return;
        }

        targetRoom.RoomBattleStarted -= HandleRoomBattleStarted;
        targetRoom.RoomBattleStarted += HandleRoomBattleStarted;
        targetRoom.RoomCleared -= HandleRoomCleared;
        targetRoom.RoomCleared += HandleRoomCleared;
    }

    // Removes room event subscriptions.
    private void UnsubscribeRoomEvents()
    {
        if (targetRoom == null)
        {
            return;
        }

        targetRoom.RoomBattleStarted -= HandleRoomBattleStarted;
        targetRoom.RoomCleared -= HandleRoomCleared;
    }

    // Hides the boss UI after the linked room is cleared.
    private void HandleRoomCleared(RoomController room)
    {
        StopHideFadeRoutine();
        _hideFadeRoutine = StartCoroutine(FadeOutAndHideRoutine());
    }

    // Shows or hides the boss UI based on room battle state.
    private void UpdateVisibilityByRoomState()
    {
        TryBindRoom();
        bool shouldShow = targetRoom != null && targetRoom.IsBattleStarted;
        SetPanelVisible(shouldShow);
    }

    // Ensures the panel has a CanvasGroup for fading.
    private void EnsureCanvasGroup()
    {
        if (_canvasGroup != null)
        {
            return;
        }

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // Applies the target visible state to the boss UI panel.
    private void SetPanelVisible(bool visible)
    {
        EnsureCanvasGroup();
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    // Fades the boss UI out before disabling it.
    private IEnumerator FadeOutAndHideRoutine()
    {
        EnsureCanvasGroup();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        float duration = Mathf.Max(0.01f, hideFadeDuration);
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _hideFadeRoutine = null;
    }

    // Stops any active hide fade before changing visibility.
    private void StopHideFadeRoutine()
    {
        if (_hideFadeRoutine == null)
        {
            return;
        }

        StopCoroutine(_hideFadeRoutine);
        _hideFadeRoutine = null;
    }

    // Converts boss health into full, half, and empty heart sprites.
    private void RefreshHearts(float currentHealth)
    {
        if (heartImages == null || heartImages.Count == 0)
        {
            return;
        }

        int currentHP = Mathf.Clamp(Mathf.CeilToInt(currentHealth), 0, heartImages.Count * 2);
        for (int i = 0; i < heartImages.Count; i++)
        {
            Image img = heartImages[i];
            if (img == null)
            {
                continue;
            }

            int heartHp = Mathf.Clamp(currentHP - i * 2, 0, 2);
            if (heartHp >= 2)
            {
                img.sprite = fullHeartSprite;
            }
            else if (heartHp == 1)
            {
                img.sprite = halfHeartSprite;
            }
            else
            {
                img.sprite = emptyHeartSprite;
            }
        }
    }

    // Creates enough heart slots for the boss max health value.
    private void RefreshHeartSlotsByMax(float maxHealth)
    {
        if (heartImages == null || heartImages.Count == 0)
        {
            return;
        }

        int expectedHearts = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(0f, maxHealth) * 0.5f));
        EnsureHeartSlots(expectedHearts);

        int slotCount = heartImages.Count;
        for (int i = 0; i < heartImages.Count; i++)
        {
            Image img = heartImages[i];
            if (img == null)
            {
                continue;
            }

            img.enabled = i < Mathf.Min(expectedHearts, slotCount);
        }
    }

    // Creates missing heart images for the boss health meter.
    private void EnsureHeartSlots(int requiredHearts)
    {
        if (!autoExpandHeartSlots)
        {
            return;
        }

        if (requiredHearts <= heartImages.Count || heartImages.Count == 0)
        {
            return;
        }

        Image template = heartImages[heartImages.Count - 1];
        if (template == null)
        {
            return;
        }

        RectTransform templateRect = template.rectTransform;
        float spacing = GetHeartSpacing();
        int missing = requiredHearts - heartImages.Count;

        for (int i = 0; i < missing; i++)
        {
            Image clone = Instantiate(template, template.transform.parent);
            clone.name = $"{template.name}_Auto_{runtimeHeartImages.Count + 1}";

            RectTransform cloneRect = clone.rectTransform;
            Vector2 basePos = templateRect.anchoredPosition;
            float offsetX = spacing * (i + 1);
            cloneRect.anchoredPosition = new Vector2(basePos.x + offsetX, basePos.y);

            clone.enabled = true;
            heartImages.Add(clone);
            runtimeHeartImages.Add(clone);
        }
    }

    // Calculates spacing between boss heart icons.
    private float GetHeartSpacing()
    {
        if (heartImages.Count >= 2 && heartImages[0] != null && heartImages[1] != null)
        {
            float spacingByLayout = heartImages[1].rectTransform.anchoredPosition.x
                - heartImages[0].rectTransform.anchoredPosition.x;
            if (Mathf.Abs(spacingByLayout) > 0.01f)
            {
                return spacingByLayout;
            }
        }

        Image fallback = heartImages[heartImages.Count - 1];
        if (fallback != null)
        {
            float width = fallback.rectTransform.rect.width;
            if (width > 0.01f)
            {
                return width;
            }
        }

        return 48f;
    }

    // Destroys heart icons generated at runtime.
    private void CleanupRuntimeHearts()
    {
        for (int i = 0; i < runtimeHeartImages.Count; i++)
        {
            Image img = runtimeHeartImages[i];
            if (img != null)
            {
                Destroy(img.gameObject);
            }
        }

        runtimeHeartImages.Clear();
    }
}
