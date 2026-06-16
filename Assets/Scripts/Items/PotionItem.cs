using TMPro;
using UnityEngine;

/// <summary>
/// Health upgrade potion dropped from chests. It increases the player's maximum health,
/// heals by the same amount, records the pickup, and displays feedback text.
/// </summary>
public class PotionItem : ChestDropItem
{
    [Header("Life Potion Effect")]
    [SerializeField] private float healthIncreaseAmount = 1f;

    [Header("Pickup Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupSfxVolume = 0.9f;

    [Header("Pickup Popup")]
    [SerializeField] private string popupText = "+1 Max HP";
    [SerializeField] private TMP_FontAsset popupFontAsset;
    [SerializeField] private Color popupColor = Color.red;
    [SerializeField] private Color popupOutlineColor = new Color(0.12f, 0f, 0f, 1f);
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
            playerStats.IncreaseMaxHealthAndHeal(healthIncreaseAmount);
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
        GameObject popupObject = new GameObject("LifePotionPopup");
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

/// <summary>
/// Simple popup-text helper used by potion rewards. It moves the text upward over time,
/// fades it out, and destroys the popup when the feedback duration is finished.
/// </summary>
public class PopupMotion : MonoBehaviour
{
    private TextMeshPro _tmp;
    private float _duration;
    private float _floatDistance;
    private float _elapsed;
    private Vector3 _startPos;
    private Color _startColor;

    // Stores popup animation settings before the text starts moving.
    public void Initialize(TextMeshPro tmp, float duration, float floatDistance)
    {
        _tmp = tmp;
        _duration = Mathf.Max(0.01f, duration);
        _floatDistance = floatDistance;
        _startPos = transform.position;
        _startColor = _tmp != null ? _tmp.color : Color.white;
    }

    // Moves the popup upward and fades the text until it expires.
    private void Update()
    {
        if (_tmp == null)
        {
            Destroy(gameObject);
            return;
        }

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        transform.position = Vector3.Lerp(_startPos, _startPos + Vector3.up * _floatDistance, t);

        Color c = _startColor;
        c.a = Mathf.Lerp(_startColor.a, 0f, t);
        _tmp.color = c;

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
