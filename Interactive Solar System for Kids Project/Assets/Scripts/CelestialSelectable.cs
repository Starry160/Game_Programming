using UnityEngine;

public class CelestialSelectable : MonoBehaviour
{
    public enum CelestialType
    {
        Planet,
        Moon
    }

    [Header("Identity")]
    public CelestialType type = CelestialType.Planet;
    public string displayName = "Earth";
    [TextArea(2, 4)] public string kidFact = "Earth is our home and full of life!";

    [Header("Feedback")]
    public AudioClip clickSound;
    public Renderer targetRenderer;
    public Color flashColor = new Color(0.6f, 0.9f, 1f);
    public float flashDuration = 0.35f;
    public float pulseScale = 1.1f;

    private void Reset()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnMouseDown()
    {
        SolarInteractionManager manager = SolarInteractionManager.Instance;
        if (manager != null)
        {
            manager.SelectObject(this);
        }
    }
}
