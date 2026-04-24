using UnityEngine;
using UnityEngine.UI;

public class SolarUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public Text titleText;
    public Text factText;

    [Header("Return Hint")]
    public Text hintText;
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
