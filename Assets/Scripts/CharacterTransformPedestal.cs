using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class CharacterTransformPedestal : MonoBehaviour
{
    [Header("Transform Config")]
    [Tooltip("替换到玩家 Animator 上的新动画控制器（相当于换一套“大脑”）。")]
    public RuntimeAnimatorController newAnimatorController;

    [Header("Visual")]
    [Tooltip("祭坛上方悬浮的职业图标物体，变身成功后会隐藏。")]
    public GameObject classIcon;

    [Tooltip("玩家靠近时显示的交互提示（例如“按 E 变身”Canvas 或文字）。")]
    public GameObject interactHint;

    [Header("Unlock")]
    [Tooltip("变身成功后要唤醒的传送门物体（场景中默认关闭）。")]
    public GameObject targetPortal;

    private bool canInteract;
    private GameObject _currentPlayer;
    private Collider2D _triggerCollider;

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider2D>();

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

        // 使用新输入系统直接读取键盘状态，与 PlayerController 保持风格统一。
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PerformTransform();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

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

        // 仅在离开的确实是当前记录的玩家时才重置状态，避免多 Player 场景下误清除。
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

    private void PerformTransform()
    {
        Animator playerAnimator = _currentPlayer.GetComponent<Animator>();
        if (playerAnimator != null && newAnimatorController != null)
        {
            // 无缝换脑：保留 Animator 状态接口，仅替换背后的控制器资源。
            playerAnimator.runtimeAnimatorController = newAnimatorController;
        }

        // 变身成功后唤醒关联的传送门（进入下一关的通道）。
        if (targetPortal != null)
        {
            targetPortal.SetActive(true);
        }

        // if (classIcon != null)
        // {
        //     classIcon.SetActive(false);
        // }

        // canInteract = false;
        // _currentPlayer = null;

        // 关闭触发器，防止玩家再次靠近重复变身。
        // if (_triggerCollider != null)
        // {
        //     _triggerCollider.enabled = false;
        // }
    }
}
