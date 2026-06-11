using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Lets the player change class, equip the matching weapon, and unlock progression.</summary>
[RequireComponent(typeof(Collider2D))]
public class CharacterTransformPedestal : MonoBehaviour
{
    [Header("Transform Config")]
    [Tooltip("Animator controller applied to the player after choosing this pedestal.")]
    public RuntimeAnimatorController newAnimatorController;

    [Header("Visual")]
    [Tooltip("Class icon object shown near this pedestal.")]
    public GameObject classIcon;

    [Tooltip("Hint object shown while the player can interact with the portal.")]
    public GameObject interactHint;

    [Header("Unlock")]
    [Tooltip("Portal enabled after the player completes this transformation.")]
    public GameObject targetPortal;

    [Header("Weapon")]
    [Tooltip("Weapon index stored in GlobalData for the transformed class.")]
    public int weaponIndex;

    [Header("Class Stats")]
    [Tooltip("Player max health after choosing this class.")]
    public float classMaxHealth;
    [Tooltip("Player max shield after choosing this class.")]
    public float classMaxShield;

    private bool canInteract;
    private GameObject _currentPlayer;
    private Collider2D _triggerCollider;

    // Records the pedestal animator and hides the transformation prompt initially.
    private void Awake()
    {
        _triggerCollider = GetComponent<Collider2D>();

        if (interactHint != null)
        {
            interactHint.SetActive(false);
        }
    }

    // Waits for the player to confirm character transformation while in range.
    private void Update()
    {
        if (!canInteract || _currentPlayer == null)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PerformTransform();
        }
    }

    // Allows the player to transform after stepping onto the pedestal trigger.
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

    // Hides the pedestal prompt when the player leaves transformation range.
    private void OnTriggerExit2D(Collider2D other)
    {
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

    // Stores the selected class, swaps weapon, unlocks the portal, and disables reuse.
    private void PerformTransform()
    {
        Animator playerAnimator = _currentPlayer.GetComponent<Animator>();
        if (playerAnimator != null && newAnimatorController != null)
        {
            playerAnimator.runtimeAnimatorController = newAnimatorController;
            GlobalData.chosenAnimatorController = newAnimatorController;
        }

        if (targetPortal != null)
        {
            targetPortal.SetActive(true);
        }

        WeaponManager weaponManager = _currentPlayer.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.SwitchWeapon(weaponIndex);
        }
        GlobalData.chosenWeaponIndex = weaponIndex;

        ApplyClassStats();

        // if (classIcon != null)
        // {
        //     classIcon.SetActive(false);
        // }

        // canInteract = false;
        // _currentPlayer = null;

        // if (_triggerCollider != null)
        // {
        //     _triggerCollider.enabled = false;
        // }
    }

    // Stores and applies the selected class survival stats.
    private void ApplyClassStats()
    {
        float safeMaxHealth = Mathf.Max(1f, classMaxHealth);
        float safeMaxShield = Mathf.Max(0f, classMaxShield);

        PlayerStats playerStats = _currentPlayer.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.ApplyClassBaseStats(safeMaxHealth, safeMaxShield);
            return;
        }

        GlobalData.hasPersistedMaxHealth = true;
        GlobalData.persistedMaxHealth = safeMaxHealth;
        GlobalData.hasPersistedHealth = true;
        GlobalData.persistedHealth = safeMaxHealth;
        GlobalData.hasPersistedMaxShield = true;
        GlobalData.persistedMaxShield = safeMaxShield;
    }
}
