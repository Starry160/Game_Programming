using TMPro;
using UnityEngine;

/// <summary>生命药水：提升生命上限并治疗，显示拾取飘字。</summary>
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
    [SerializeField] private float popupFontSize = 3.5f;
    [SerializeField, Range(0f, 1f)] private float popupOutlineWidth = 0.2f;
    [SerializeField] private float popupDuration = 0.75f;
    [SerializeField] private float popupFloatDistance = 0.7f;
    [SerializeField] private int popupSortingOrder = 200;

    // 拾取：IncreaseMaxHealthAndHeal + 飘字 + 音效。
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
            Debug.Log($"[PotionItem] {name} 被玩家拾取，生命上限与当前生命 +{healthIncreaseAmount}。");
        }
        else
        {
            Debug.LogWarning($"[PotionItem] {name} 被拾取，但未找到 PlayerStats。");
        }

        PlayPickupSfx();

        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.AddPotion();
        }

        base.OnPickedByPlayer(player);
    }

    // 在拾取点播放音效。
    private void PlayPickupSfx()
    {
        if (pickupSfx == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupSfxVolume);
    }

    // 动态创建 TMP 飘字并挂 PopupMotion。
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

/// <summary>拾取飘字：上移并淡出后销毁。</summary>
public class PopupMotion : MonoBehaviour
{
    private TextMeshPro _tmp;
    private float _duration;
    private float _floatDistance;
    private float _elapsed;
    private Vector3 _startPos;
    private Color _startColor;

    // 配置飘字时长与上浮距离。
    public void Initialize(TextMeshPro tmp, float duration, float floatDistance)
    {
        _tmp = tmp;
        _duration = Mathf.Max(0.01f, duration);
        _floatDistance = floatDistance;
        _startPos = transform.position;
        _startColor = _tmp != null ? _tmp.color : Color.white;
    }

    // 每帧上移并降低 alpha，结束销毁。
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
