using UnityEngine;
using TMPro;

public class SolarUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public TextMeshProUGUI titleText; // Displays the selected planet or moon name.
    public TextMeshProUGUI factText;  // Displays the child-friendly fact text.

    [Header("Return Hint")]
    public TextMeshProUGUI hintText;  // Displays the return-to-overview hint.
    [TextArea(1, 2)] public string returnHint = "Press ESC or tap Return to overview.";

    private void Start()
    {
        HideInfo();
        if (hintText != null)
        {
            hintText.text = returnHint;
        }
    }

    public void ShowInfo(CelestialSelectable selectable)
    {
        if (selectable == null) return;

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        if (titleText != null)
        {
            string typeLabel = selectable.type == CelestialSelectable.CelestialType.Planet ? "Planet" : "Moon";
            titleText.text = selectable.displayName + " (" + typeLabel + ")";
        }

        if (factText != null)
        {
            factText.text = selectable.kidFact;
        }
    }

    public void HideInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
}
