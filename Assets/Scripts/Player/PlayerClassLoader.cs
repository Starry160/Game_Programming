using UnityEngine;

/// <summary>
/// Restores the player's selected class after scene loading. It reads GlobalData, applies the
/// correct animator controller, switches the weapon, and disables attacks until a class exists.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerClassLoader : MonoBehaviour
{
    [SerializeField] private bool disableAttackAfterLoad = false;

    // Activates the character prefab that matches the class selected in the menu.
    private void Start()
    {
        ApplySelectedClass();

        if (disableAttackAfterLoad)
        {
            DisableAttack();
        }
    }

    // Applies the animator controller selected during class choice.
    private void ApplySelectedClass()
    {
        if (GlobalData.chosenAnimatorController == null)
        {
            return;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = GlobalData.chosenAnimatorController;
        }
    }

    // Disables player attack input for scenes that only show the character.
    private void DisableAttack()
    {
        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }
    }
}
