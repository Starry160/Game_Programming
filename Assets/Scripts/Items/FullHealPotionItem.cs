using TMPro;
using UnityEngine;

/// <summary>场景放置型生命药水：保持 ChestDropItem 的 idle 浮动效果，拾取后直接回满血。</summary>
public class FullHealPotionItem : ChestDropItem
{
    [Header("Scene Setup")]
    [Tooltip("场景内直接摆放时，开局自动启用拾取与上下浮动效果。")]
    [SerializeField] private bool autoEnableIdleOnStart = true;

    [Header("Pickup Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupSfxVolume = 0.9f;

    [Header("Pickup Popup")]
    [SerializeField] private string popupText = "HP Full";
    [SerializeField] private TMP_FontAsset popupFontAsset;
    [SerializeField] private Color popupColor = Color.red;
    [SerializeField] private Color popupOutlineColor = new Color(0.12f, 0f, 0f, 1f);
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.9f, 0f);
    [SerializeField] private float popupFontSize = 3.5f;
    [SerializeField, Range(0f, 1f)] private float popupOutlineWidth = 0.2f;
    [SerializeField] private float popupDuration = 0.75f;
    [SerializeField] private float popupFloatDistance = 0.7f;
    [SerializeField] private int popupSortingOrder = 200;

    // 场景直摆药水：用零位移 PopOut 启动同款 idle 效果与可拾取状态。
    private void Start()
    {
        if (!autoEnableIdleOnStart)
        {
            return;
        }

        if (moveRoutine != null || canBePicked)
        {
            return;
        }

        PopOut(Vector2.zero);
    }

    // 拾取后直接回满当前生命值上限。
    protected override void OnPickedByPlayer(Collider2D player)
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = player.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.Heal(playerStats.maxHealth);
            SpawnPopup(playerStats.transform.position + popupOffset);
            Debug.Log($"[FullHealPotionItem] {name} 被玩家拾取，生命值已回满。");
        }
        else
        {
            Debug.LogWarning($"[FullHealPotionItem] {name} 被拾取，但未找到 PlayerStats。");
        }

        PlayPickupSfx();

        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.AddPotion();
        }

        base.OnPickedByPlayer(player);
    }

    private void PlayPickupSfx()
    {
        if (pickupSfx == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupSfxVolume);
    }

    private void SpawnPopup(Vector3 worldPosition)
    {
        GameObject popupObject = new GameObject("FullHealPotionPopup");
        popupObject.transform.position = worldPosition;

        TextMeshPro tmp = popupObject.AddComponent<TextMeshPro>();
        tmp.text = popupText;
        if (popupFontAsset != null)
        {
            tmp.font = popupFontAsset;
        }
        tmp.fontSize = popupFontSize;
        tmp.color = popupColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.sortingOrder = popupSortingOrder;
        tmp.outlineColor = popupOutlineColor;
        tmp.outlineWidth = popupOutlineWidth;
        tmp.enableWordWrapping = false;

        PopupMotion popupMotion = popupObject.AddComponent<PopupMotion>();
        popupMotion.Initialize(tmp, popupDuration, popupFloatDistance);
    }
}
