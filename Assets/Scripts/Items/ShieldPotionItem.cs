using TMPro;
using UnityEngine;

/// <summary>
/// Shield upgrade potion dropped from chests. It increases maximum shield, fills the gained shield,
/// records the pickup, and shows sound and popup feedback.
/// </summary>
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
    [SerializeField] private float popupFontSize;
    [SerializeField, Range(0f, 1f)] private float popupOutlineWidth = 0.2f;
    [SerializeField] private float popupDuration;
    [SerializeField] private float popupFloatDistance;
    [SerializeField] private int popupSortingOrder = 200;

    // Applies the pickup effect before destroying the reward object.
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
        }

        PlayPickupSfx();

        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.AddPotion();
        }

        base.OnPickedByPlayer(player);
    }

    // Plays the pickup sound at the collected item position.
    private void PlayPickupSfx()
    {
        if (pickupSfx == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupSfxVolume);
    }

    // Creates floating pickup text to show the reward effect.
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
