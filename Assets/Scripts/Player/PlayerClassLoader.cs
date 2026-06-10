using UnityEngine;

/// <summary>Applies the previously selected class animator when a level loads.</summary>
[RequireComponent(typeof(Animator))]
public class PlayerClassLoader : MonoBehaviour
{
    // Activates the character prefab that matches the class selected in the menu.
    private void Start()
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
}
