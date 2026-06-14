using UnityEngine;

/// <summary>Applies the previously selected class animator when a level loads.</summary>
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

    private void DisableAttack()
    {
        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }
    }
}
