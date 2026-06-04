using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss 顶部血量 UI：名称 + 三颗心（共 6 点，半心计）。
/// </summary>
public class BossHealthUI : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private FinalBossController targetBoss;
    [SerializeField] private string bossObjectName = "FinalBoss";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private string bossDisplayName = "Mecha-stone Golem";
    [SerializeField] private List<Image> heartImages = new List<Image>();
    [SerializeField] private bool autoExpandHeartSlots = true;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private readonly List<Image> runtimeHeartImages = new List<Image>();

    private void OnEnable()
    {
        TryBindBoss();
        SubscribeBossEvents();
        RefreshImmediate();
    }

    private void OnDisable()
    {
        UnsubscribeBossEvents();
    }

    private void OnDestroy()
    {
        CleanupRuntimeHearts();
    }

    private void Start()
    {
        if (bossNameText != null)
        {
            bossNameText.text = bossDisplayName;
        }

        RefreshImmediate();
    }

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

    private void SubscribeBossEvents()
    {
        if (targetBoss == null)
        {
            return;
        }

        targetBoss.HealthChanged -= HandleHealthChanged;
        targetBoss.HealthChanged += HandleHealthChanged;
    }

    private void UnsubscribeBossEvents()
    {
        if (targetBoss == null)
        {
            return;
        }

        targetBoss.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        RefreshHeartSlotsByMax(max);
        RefreshHearts(current);
    }

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
