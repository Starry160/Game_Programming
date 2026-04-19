using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DoorController : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("门关闭时显示的图片。")]
    [SerializeField] private Sprite _closedSprite;

    [Tooltip("门打开时显示的图片。")]
    [SerializeField] private Sprite _openSprite;

    [Header("Physics")]
    [Tooltip("阻挡玩家通行的实体碰撞体（不是触发器）。")]
    [SerializeField] private Collider2D _solidCollider;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化为关门状态，确保运行时视觉与碰撞一致。
        if (_closedSprite != null)
        {
            _spriteRenderer.sprite = _closedSprite;
        }

        if (_solidCollider != null)
        {
            _solidCollider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        OpenDoor();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        CloseDoor();
    }

    private void OpenDoor()
    {
        if (_openSprite != null)
        {
            _spriteRenderer.sprite = _openSprite;
        }

        if (_solidCollider != null)
        {
            _solidCollider.enabled = false;
        }
    }

    private void CloseDoor()
    {
        if (_closedSprite != null)
        {
            _spriteRenderer.sprite = _closedSprite;
        }

        if (_solidCollider != null)
        {
            _solidCollider.enabled = true;
        }
    }
}
