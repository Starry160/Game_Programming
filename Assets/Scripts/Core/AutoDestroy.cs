using UnityEngine;

/// <summary>Destroys one-shot visual effects after a short lifetime.</summary>
public class AutoDestroy : MonoBehaviour
{
    // Schedules this temporary effect object to be removed after its lifetime.
    private void Start()
    {
        Destroy(gameObject, 0.3f);
    }
}
