using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Controls door sprites, locking, collision, and optional scene transitions.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DoorController : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("When enabled, this door loads another scene after opening.")]
    [SerializeField] private Sprite _closedSprite;

    [Tooltip("When enabled, this door loads another scene after opening.")]
    [SerializeField] private Sprite _openSprite;

    [Header("Physics")]
    [Tooltip("When enabled, this door loads another scene after opening.")]
    [SerializeField] private Collider2D _solidCollider;
    [Header("Animation (Optional)")]
    [Tooltip("When enabled, this door loads another scene after opening.")]
    [SerializeField] private Animator _doorAnimator;
    [SerializeField] private string _openTriggerName = "Open";
    [SerializeField] private string _closeTriggerName = "Close";

    [Header("Scene Transition")]
    [Tooltip("When enabled, this door loads another scene after opening.")]
    public bool isExitDoor;

    [Tooltip("Scene name to load; it must be included in Build Settings.")]
    public string targetSceneName;
    [Header("Room Lock")]
    [Tooltip("Automatically locks the door while enemies remain in the room.")]
    public bool autoLockOnBattleStart = true;
    [Tooltip("Delay between room enemy checks in seconds.")]
    public float enemyCheckInterval = 0.5f;

    private SpriteRenderer _spriteRenderer;
    private bool _isOpen;
    private bool _hasTransitioned;
    private bool isLocked = false;
    private bool isRoomCleared = false;
    private float nextEnemyCheckTime = 0f;
    private float _nextLockedLogTime = 0f;
    private const float LOCKED_LOG_INTERVAL = 1f;

    // Captures the door collider, renderer, and closed-state visuals for later restore.
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

    // Opens the door when the nearby player presses the interact key and the room is clear.
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

    // Tracks the player entering door range and shows the interact prompt when allowed.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (isLocked)
        {
            LogLockedMessage();
            return;
        }

        if (!TryOpenForPlayer())
        {
            return;
        }

        if (isExitDoor && _isOpen)
        {
            TryLoadTargetScene();
        }
    }

    // Handles objects that remain inside this trigger area.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

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

    // Clears door interaction state once the player leaves the trigger area.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        CloseDoor();
    }

    // Switches the door to its open visual and disables blocking collision.
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

    // Restores the closed door visual and blocking collider.
    private void CloseDoor()
    {
        if (_closedSprite != null)
        {
            _spriteRenderer.sprite = _closedSprite;
        }

        if (_doorAnimator != null)
        {
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

    /// <summary>Controls door sprites, locking, collision, and optional scene transitions.</summary>
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

    // Attempts to open the door only if the room is clear and unlocked.
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

    // Loads the configured exit scene once and prevents duplicate transition calls.
    private void TryLoadTargetScene()
    {
        if (_hasTransitioned)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"[DoorController] {name} is marked as an exit door, but targetSceneName is not configured.", this);
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.Equals(targetSceneName, currentScene, System.StringComparison.Ordinal))
        {
            return;
        }

        _hasTransitioned = true;
        PersistPlayerStatsBeforeSceneLoad();
        SceneManager.LoadScene(targetSceneName);
    }

    // Saves the player's current health and shield before this door changes scenes.
    private static void PersistPlayerStatsBeforeSceneLoad()
    {
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.PersistCurrentStats();
        }
    }

    // Checks whether any active Enemy-tagged objects remain in the scene.
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

    // Shows a throttled message when the room is still locked by enemies.
    private void LogLockedMessage()
    {
        if (Time.time < _nextLockedLogTime)
        {
            return;
        }

        _nextLockedLogTime = Time.time + LOCKED_LOG_INTERVAL;
        Debug.Log("[Dungeon Seal] Enemies remain in the room. The door cannot open!");
    }
}
