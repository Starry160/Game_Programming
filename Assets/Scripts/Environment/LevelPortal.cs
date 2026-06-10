using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>Handles player interaction, portal pull-in animation, and scene loading.</summary>
[RequireComponent(typeof(Collider2D))]
public class LevelPortal : MonoBehaviour
{
    [Header("Unlock Condition (Optional)")]
    [Tooltip("Room that must be cleared before this portal becomes visible.")]
    [SerializeField] private RoomController _requiredRoomController;

    [Tooltip("Keep the portal hidden until the linked room has been cleared.")]
    [SerializeField] private bool _showAfterRoomCleared = false;

    [Header("Scene")]
    [Tooltip("Scene name to load; it must be included in Build Settings.")]
    public string targetSceneName;

    [Header("UI")]
    [Tooltip("Hint object shown while the player can interact with the portal.")]
    public GameObject interactHint;

    [Header("Animation")]
    [Tooltip("Animator used for the portal activation animation.")]
    public Animator portalAnimator;
    [Tooltip("Animator trigger used to play the portal activation animation.")]
    [SerializeField] private string _activateTriggerName = "Activate";

    [Tooltip("Duration of the pull-in animation before loading the scene.")]
    public float teleportDelay = 1.5f;

    private bool canInteract;
    private GameObject _currentPlayer;
    private bool _isUnlocked = true;
    private bool _hasActivateTrigger = true;

    // Applies room-clear unlock rules and prepares the portal prompt and animation trigger.
    private void Awake()
    {
        _isUnlocked = !_showAfterRoomCleared;

        // Room-gated portals stay hidden until their linked room reports a clear event.
        if (_showAfterRoomCleared)
        {
            if (_requiredRoomController == null)
            {
                _requiredRoomController = GetComponentInParent<RoomController>();
            }

            if (_requiredRoomController != null)
            {
                _requiredRoomController.RoomCleared += HandleRequiredRoomCleared;
                _isUnlocked = _requiredRoomController.IsRoomCleared;
            }

            SetPortalVisibility(_isUnlocked);
        }

        CacheAnimatorTriggerFlag();

        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }
    }

    // Detaches from the room-clear event when this portal is destroyed.
    private void OnDestroy()
    {
        if (_requiredRoomController != null)
        {
            _requiredRoomController.RoomCleared -= HandleRequiredRoomCleared;
        }
    }

    // Waits for the interact key while the player is inside an unlocked portal.
    private void Update()
    {
        if (!canInteract || _currentPlayer == null)
        {
            return;
        }

        // Pressing E begins the pull-in sequence instead of loading the scene immediately.
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            BeginTeleport();
        }
    }

    // Enables portal interaction when the player enters an unlocked portal trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isUnlocked)
        {
            return;
        }

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

    // Disables portal interaction when the player leaves the portal trigger.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!_isUnlocked)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

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

    // Starts the portal interaction and freezes player physics during the pull-in.
    private void BeginTeleport()
    {
        canInteract = false;

        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }

        // Play the optional portal animation, then freeze the player for the pull-in effect.
        TryPlayPortalActivateAnimation();

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

    // Pulls the player into the portal before loading the configured target scene.
    private IEnumerator SuckInRoutine()
    {
        Transform playerTransform = _currentPlayer != null ? _currentPlayer.transform : null;
        Vector3 startPosition = playerTransform != null ? playerTransform.position : Vector3.zero;
        Vector3 startScale = playerTransform != null ? playerTransform.localScale : Vector3.one;
        Vector3 portalCenter = transform.position;

        float timer = 0f;
        while (timer < teleportDelay)
        {
            timer += Time.deltaTime;

            // Lerp both position and scale so the player appears to be absorbed by the portal.
            if (playerTransform != null)
            {
                float t = Mathf.Clamp01(timer / teleportDelay);
                playerTransform.position = Vector3.Lerp(startPosition, portalCenter, t);
                playerTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            }

            yield return null;
        }

        if (playerTransform != null)
        {
            playerTransform.position = portalCenter;
            playerTransform.localScale = Vector3.zero;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"[LevelPortal] {name} has no targetSceneName configured, so it cannot load a scene.", this);
            yield break;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.Equals(targetSceneName, currentScene, System.StringComparison.Ordinal))
        {
            // Avoid reloading the active scene when a portal is accidentally pointed at itself.
            yield break;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    // Unlocks the portal when its required room has been cleared.
    private void HandleRequiredRoomCleared(RoomController clearedRoom)
    {
        if (clearedRoom != _requiredRoomController)
        {
            return;
        }

        _isUnlocked = true;
        SetPortalVisibility(true);
    }

    // Shows or hides the portal visuals and trigger collider.
    private void SetPortalVisibility(bool visible)
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = visible;
        }

        // Hidden portals disable visuals and trigger interaction together.
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = visible;
        }

        if (portalAnimator != null)
        {
            portalAnimator.gameObject.SetActive(visible);
        }

        if (!visible && interactHint != null)
        {
            interactHint.SetActive(false);
        }
    }

    // Checks whether the portal animator supports the optional activation trigger.
    private void CacheAnimatorTriggerFlag()
    {
        _hasActivateTrigger = true;
        if (portalAnimator == null || string.IsNullOrEmpty(_activateTriggerName))
        {
            return;
        }

        // Cache trigger availability once so missing animator triggers do not spam warnings.
        _hasActivateTrigger = false;
        AnimatorControllerParameter[] parameters = portalAnimator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter != null &&
                parameter.type == AnimatorControllerParameterType.Trigger &&
                parameter.name == _activateTriggerName)
            {
                _hasActivateTrigger = true;
                break;
            }
        }

    }

    // Plays the portal activation trigger when the animator supports it.
    private void TryPlayPortalActivateAnimation()
    {
        if (portalAnimator == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_activateTriggerName) || !_hasActivateTrigger)
        {
            return;
        }

        portalAnimator.SetTrigger(_activateTriggerName);
    }
}
