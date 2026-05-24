using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Scene Transition")]
    [Tooltip("勾选后，玩家进入这扇门会触发场景跳转。")]
    public bool isExitDoor;

    [Tooltip("跳转的目标场景名称（需在 Build Settings 中已添加）。")]
    public string targetSceneName;

    private SpriteRenderer _spriteRenderer;
    private bool _isOpen;
    private bool _hasTransitioned;

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

        _isOpen = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.LogWarning($"[PortalTrace] DoorController '{name}' trigger entered by '{other.name}' at pos={other.transform.position}");

        OpenDoor();

        // 出口门：仅在门成功开启后再触发场景跳转，避免与门的状态冲突。
        if (isExitDoor && _isOpen)
        {
            TryLoadTargetScene();
        }
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

        _isOpen = true;
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

        _isOpen = false;
    }

    private void TryLoadTargetScene()
    {
        // 防止 LoadScene 期间触发器多次回调导致重复加载。
        if (_hasTransitioned)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"[DoorController] {name} 标记为出口门，但未配置 targetSceneName。", this);
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.Equals(targetSceneName, currentScene, System.StringComparison.Ordinal))
        {
            Debug.LogWarning($"[PortalTrace] DoorController '{name}' blocked self-reload of current scene '{currentScene}'.");
            return;
        }

        _hasTransitioned = true;
        Debug.LogWarning($"[PortalTrace] DoorController '{name}' loading scene '{targetSceneName}'. PlayerPos={GameObject.FindWithTag("Player")?.transform.position ?? Vector3.zero}");
        SceneManager.LoadScene(targetSceneName);
    }
}
