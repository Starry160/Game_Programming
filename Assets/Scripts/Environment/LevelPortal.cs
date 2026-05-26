using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelPortal : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("要跳转到的目标场景名称（需在 Build Settings 中已添加）。")]
    public string targetSceneName;

    [Header("UI")]
    [Tooltip("玩家靠近时显示的交互提示（例如“按 E 进入”Canvas 或文字）。")]
    public GameObject interactHint;

    [Header("Animation")]
    [Tooltip("传送门的 Animator，用于播放激活动画。Trigger 参数名固定为 Activate。")]
    public Animator portalAnimator;

    [Tooltip("吸入动画时长（秒），同时也是切场景前的总延迟。")]
    public float teleportDelay = 1.5f;

    private bool canInteract;
    private GameObject _currentPlayer;

    private void Awake()
    {
        // 默认隐藏交互提示，只有玩家靠近时才出现。
        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }
    }

    private void Update()
    {
        if (!canInteract || _currentPlayer == null)
        {
            return;
        }

        // 与项目内其他交互脚本风格一致，直接读新输入系统的键盘状态。
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            BeginTeleport();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.LogWarning($"[PortalTrace] LevelPortal '{name}' trigger entered by '{other.name}' at pos={other.transform.position}");
        canInteract = true;
        _currentPlayer = other.gameObject;

        if (interactHint != null)
        {
            interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 仅当离开的确实是当前记录的玩家时才清空，避免误清除。
        if (other.gameObject == _currentPlayer)
        {
            canInteract = false;
            _currentPlayer = null;

            if (interactHint != null)
            {
                interactHint.SetActive(false);
            }
        }
    }

    private void BeginTeleport()
    {
        // 上锁 + 关闭提示，防止延迟期间被狂按或误操作。
        canInteract = false;

        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }

        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger("Activate");
        }

        // 冻结玩家的物理模拟，防止被吸入过程中还能移动/被碰撞推开。
        if (_currentPlayer != null)
        {
            Rigidbody2D rb = _currentPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
            }
        }

        StartCoroutine(SuckInRoutine());
    }

    private IEnumerator SuckInRoutine()
    {
        // 缓存玩家 Transform 与初始状态，保证整段插值稳定。
        Transform playerTransform = _currentPlayer != null ? _currentPlayer.transform : null;
        Vector3 startPosition = playerTransform != null ? playerTransform.position : Vector3.zero;
        Vector3 startScale = playerTransform != null ? playerTransform.localScale : Vector3.one;
        Vector3 portalCenter = transform.position;

        float timer = 0f;
        while (timer < teleportDelay)
        {
            timer += Time.deltaTime;

            if (playerTransform != null)
            {
                float t = Mathf.Clamp01(timer / teleportDelay);
                playerTransform.position = Vector3.Lerp(startPosition, portalCenter, t);
                playerTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            }

            yield return null;
        }

        // 确保最终精确落位，避免帧误差残留。
        if (playerTransform != null)
        {
            playerTransform.position = portalCenter;
            playerTransform.localScale = Vector3.zero;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"[LevelPortal] {name} 未配置 targetSceneName，无法跳转。", this);
            yield break;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.Equals(targetSceneName, currentScene, System.StringComparison.Ordinal))
        {
            Debug.LogWarning($"[PortalTrace] LevelPortal '{name}' blocked self-reload of current scene '{currentScene}'.");
            yield break;
        }

        string playerName = _currentPlayer != null ? _currentPlayer.name : "null";
        Vector3 playerPos = _currentPlayer != null ? _currentPlayer.transform.position : Vector3.zero;
        Debug.LogWarning($"[PortalTrace] LevelPortal '{name}' loading scene '{targetSceneName}'. TriggerPlayer={playerName}, PlayerPos={playerPos}");
        SceneManager.LoadScene(targetSceneName);
    }
}
