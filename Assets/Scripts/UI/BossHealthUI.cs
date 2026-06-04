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
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

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
        RefreshHearts(current);
    }

    private void RefreshImmediate()
    {
        TryBindBoss();
        if (targetBoss == null)
        {
            return;
        }

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
}
