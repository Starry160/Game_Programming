using UnityEngine;

/// <summary>
/// Cleans up temporary visual effect objects such as slash, hit, or explosion effects.
/// This prevents one-shot effects from staying in the scene after their animation has finished.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    // Schedules this temporary effect object to be removed after its lifetime.
    private void Start()
    {
        Destroy(gameObject, 0.3f);
    }
}
