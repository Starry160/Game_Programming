using TMPro;
using UnityEngine;

/// <summary>Restores the player to full health when collected in the scene.</summary>
public class FullHealPotionItem : ChestDropItem
{
    [Header("Scene Setup")]
    [Tooltip("Enable pickup and idle floating automatically when placed directly in a scene.")]
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

    // Caches potion visuals and prepares the pickup interaction radius.
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
            playerStats.Heal(playerStats.maxHealth);
            SpawnPopup(playerStats.transform.position + popupOffset);
            Debug.Log($"[FullHealPotionItem] {name} picked up. Health fully restored.");
        }
        else
        {
            Debug.LogWarning($"[FullHealPotionItem] {name} was picked up, but PlayerStats was not found.");
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
