using TMPro;
using UnityEngine;

/// <summary>无敌药水：授予限时无敌并显示飘字。</summary>
public class InvincibilityPotionItem : ChestDropItem
{
    [Header("Invincibility Effect")]
    [SerializeField] private float invincibilityDuration = 10f;

    [Header("Pickup Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupSfxVolume = 0.9f;

    [Header("Pickup Popup")]
    [SerializeField] private string popupText = "Invincible 10s";
    [SerializeField] private TMP_FontAsset popupFontAsset;
    [SerializeField] private Color popupColor = new Color(1f, 0.85f, 0.25f, 1f);
    [SerializeField] private Color popupOutlineColor = new Color(0.2f, 0.1f, 0f, 1f);
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.9f, 0f);
    [SerializeField] private float popupFontSize = 3.5f;
    [SerializeField, Range(0f, 1f)] private float popupOutlineWidth = 0.2f;
    [SerializeField] private float popupDuration = 0.85f;
    [SerializeField] private float popupFloatDistance = 0.8f;
    [SerializeField] private int popupSortingOrder = 200;

    // 拾取：GrantTemporaryInvincibility + 飘字 + 音效。
    protected override void OnPickedByPlayer(Collider2D player)
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = player.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.GrantTemporaryInvincibility(invincibilityDuration);
            SpawnPopup(playerStats.transform.position + popupOffset);
            Debug.Log($"[InvincibilityPotionItem] {name} 被玩家拾取，获得无敌 {invincibilityDuration:F1} 秒。");
        }
        else
        {
            Debug.LogWarning($"[InvincibilityPotionItem] {name} 被拾取，但未找到 PlayerStats。");
        }

        PlayPickupSfx();
        base.OnPickedByPlayer(player);
    }

    // 播放拾取音效。
    private void PlayPickupSfx()
    {
        if (pickupSfx == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupSfxVolume);
    }

    // 创建无敌飘字。
    private void SpawnPopup(Vector3 worldPosition)
    {
        GameObject popupObject = new GameObject("InvincibilityPotionPopup");
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
