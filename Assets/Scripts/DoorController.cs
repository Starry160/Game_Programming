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
    [Header("Animation (Optional)")]
    [Tooltip("若门有 Animator，可在这里绑定用于开关门动画的控制器。")]
    [SerializeField] private Animator _doorAnimator;
    [SerializeField] private string _openTriggerName = "Open";
    [SerializeField] private string _closeTriggerName = "Close";

    [Header("Scene Transition")]
    [Tooltip("勾选后，玩家进入这扇门会触发场景跳转。")]
    public bool isExitDoor;

    [Tooltip("跳转的目标场景名称（需在 Build Settings 中已添加）。")]
    public string targetSceneName;
    [Header("Room Lock")]
    [Tooltip("战斗开始后自动进入锁门逻辑，清怪前无法开门。")]
    public bool autoLockOnBattleStart = true;
    [Tooltip("敌人存活检测间隔（秒）。")]
    public float enemyCheckInterval = 0.5f;

    private SpriteRenderer _spriteRenderer;
    private bool _isOpen;
    private bool _hasTransitioned;
    private bool isLocked = false;
    private bool isRoomCleared = false;
    private float nextEnemyCheckTime = 0f;
    private float _nextLockedLogTime = 0f;
    private const float LOCKED_LOG_INTERVAL = 1f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_solidCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (!colliders[i].isTrigger)
                {
                    _solidCollider = colliders[i];
                    break;
                }
            }
        }

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
        nextEnemyCheckTime = 0f;
    }

    private void Update()
    {
        if (!autoLockOnBattleStart)
        {
            return;
        }

        if (Time.time < nextEnemyCheckTime)
        {
            return;
        }

        nextEnemyCheckTime = Time.time + Mathf.Max(0.1f, enemyCheckInterval);
        bool hasEnemiesAlive = CheckIfEnemiesAlive();
        isRoomCleared = !hasEnemiesAlive;
        isLocked = hasEnemiesAlive;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 铁律：锁死状态下，第一时间拦截，不执行任何开门逻辑与动画逻辑。
        if (isLocked)
        {
            LogLockedMessage();
            return;
        }

        if (!TryOpenForPlayer())
        {
            return;
        }

        // 出口门：仅在门成功开启后再触发场景跳转，避免与门的状态冲突。
        if (isExitDoor && _isOpen)
        {
            TryLoadTargetScene();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 铁律：锁死状态下，持续拦截，避免玩家卡门缝时误开门。
        if (isLocked)
        {
            LogLockedMessage();
            return;
        }

        if (TryOpenForPlayer() && isExitDoor && _isOpen)
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
        if (isLocked)
        {
            return;
        }

        if (_openSprite != null)
        {
            _spriteRenderer.sprite = _openSprite;
        }

        if (_doorAnimator != null)
        {
            if (!string.IsNullOrEmpty(_closeTriggerName))
            {
                _doorAnimator.ResetTrigger(_closeTriggerName);
            }
            if (!string.IsNullOrEmpty(_openTriggerName))
            {
                _doorAnimator.SetTrigger(_openTriggerName);
            }
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

        if (_doorAnimator != null)
        {
            // 关门时强制清掉开门 Trigger，防止动画状态机后台误触发开门。
            if (!string.IsNullOrEmpty(_openTriggerName))
            {
                _doorAnimator.ResetTrigger(_openTriggerName);
            }
            if (!string.IsNullOrEmpty(_closeTriggerName))
            {
                _doorAnimator.SetTrigger(_closeTriggerName);
            }
        }

        if (_solidCollider != null)
        {
            _solidCollider.enabled = true;
        }

        _isOpen = false;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (locked)
        {
            isRoomCleared = false;
            CloseDoor();
            return;
        }

        isRoomCleared = true;
    }

    private bool TryOpenForPlayer()
    {
        if (isLocked || !isRoomCleared)
        {
            LogLockedMessage();
            return false;
        }

        OpenDoor();
        return true;
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

    private bool CheckIfEnemiesAlive()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private void LogLockedMessage()
    {
        if (Time.time < _nextLockedLogTime)
        {
            return;
        }

        _nextLockedLogTime = Time.time + LOCKED_LOG_INTERVAL;
        Debug.Log("【地牢结界】房间内还有敌人未消灭，门无法打开！");
    }
}
