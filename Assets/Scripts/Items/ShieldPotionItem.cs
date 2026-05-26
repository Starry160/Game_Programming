using TMPro;
using UnityEngine;

/// <summary>护盾药水：提升护盾上限并填满增量。</summary>
public class ShieldPotionItem : ChestDropItem
{
    [Header("Shield Potion Effect")]
    [SerializeField] private float shieldIncreaseAmount = 1f;

    [Header("Pickup Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupSfxVolume = 0.9f;

    [Header("Pickup Popup")]
    [SerializeField] private string popupText = "+1 Max Shield";
    [SerializeField] private TMP_FontAsset popupFontAsset;
    [SerializeField] private Color popupColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] private Color popupOutlineColor = new Color(0f, 0.1f, 0.2f, 1f);
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 0.9f, 0f);
    [SerializeField] private float popupFontSize = 3.5f;
    [SerializeField, Range(0f, 1f)] private float popupOutlineWidth = 0.2f;
    [SerializeField] private float popupDuration = 0.75f;
    [SerializeField] private float popupFloatDistance = 0.7f;
    [SerializeField] private int popupSortingOrder = 200;

    // 拾取：IncreaseMaxShieldAndFill + 飘字 + 音效。
    protected override void OnPickedByPlayer(Collider2D player)
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = player.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.IncreaseMaxShieldAndFill(shieldIncreaseAmount);
            SpawnPopup(playerStats.transform.position + popupOffset);
            Debug.Log($"[ShieldPotionItem] {name} 被玩家拾取，护盾上限与当前护盾 +{shieldIncreaseAmount}。");
        }
        else
        {
            Debug.LogWarning($"[ShieldPotionItem] {name} 被拾取，但未找到 PlayerStats。");
        }

        PlayPickupSfx();

        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.AddPotion();
        }

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

    // 创建护盾飘字。
    private void SpawnPopup(Vector3 worldPosition)
    {
        GameObject popupObject = new GameObject("ShieldPotionPopup");
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
