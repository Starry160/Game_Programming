using System.Collections;
using UnityEngine;

public class SolarInteractionManager : MonoBehaviour
{
    public static SolarInteractionManager Instance { get; private set; }

    [Header("Scene References")]
    public SolarCameraController cameraController;
    public SolarUIController uiController;
    public AudioSource audioSource;

    private Coroutine feedbackRoutine;
    private CelestialSelectable currentSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<SolarCameraController>();
        }

        if (uiController == null)
        {
            uiController = FindObjectOfType<SolarUIController>();
        }

        if (audioSource == null)
        {
            audioSource = FindObjectOfType<AudioSource>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToOverview();
        }
    }

    public void SelectObject(CelestialSelectable selectable)
    {
        if (selectable == null) return;
        currentSelected = selectable;

        if (cameraController != null)
        {
            cameraController.FocusOn(selectable.transform);
        }

        if (uiController != null)
        {
            uiController.ShowInfo(selectable);
        }

        if (audioSource != null && selectable.clickSound != null)
        {
            audioSource.PlayOneShot(selectable.clickSound);
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }
        feedbackRoutine = StartCoroutine(PlayVisualFeedback(selectable));
    }

    public void ReturnToOverview()
    {
        currentSelected = null;

        if (cameraController != null)
        {
            cameraController.ReturnToMainView();
        }

        if (uiController != null)
        {
            uiController.HideInfo();
        }
    }

    public void ReturnToOverviewByButton()
    {
        ReturnToOverview();
    }

    private IEnumerator PlayVisualFeedback(CelestialSelectable selectable)
    {
        if (selectable == null || selectable.targetRenderer == null)
        {
            yield break;
        }

        Transform target = selectable.targetRenderer.transform;
        Vector3 originalScale = target.localScale;
        Vector3 enlargedScale = originalScale * selectable.pulseScale;

        Material material = selectable.targetRenderer.material;
        Color originalColor = material.color;

        material.color = selectable.flashColor;
        target.localScale = enlargedScale;

        yield return new WaitForSeconds(selectable.flashDuration);

        target.localScale = originalScale;
        material.color = originalColor;
        feedbackRoutine = null;
    }
}
