using TMPro;
using UnityEngine;

/// <summary>
/// This class inherits from UIelement and handles displaying player lives as text.
/// </summary>
public class LivesDisplay : UIelement
{
    [Tooltip("The text UI to use for displaying lives")]
    public TextMeshProUGUI displayText = null;

    [Tooltip("The player's health component that stores current lives")]
    public Health playerHealth = null;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerHealth = playerObject.GetComponent<Health>();
            }
        }

        DisplayLives();
    }

    /// <summary>
    /// Description:
    /// Updates the lives text display.
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    public void DisplayLives()
    {
        if (displayText != null)
        {
            int lives = 0;
            if (playerHealth != null)
            {
                lives = Mathf.Max(0, playerHealth.currentLives);
            }
            displayText.text = "Lives: " + lives.ToString();
        }
    }

    /// <summary>
    /// Description:
    /// Overrides UpdateUI and updates the lives display.
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    public override void UpdateUI()
    {
        base.UpdateUI();
        DisplayLives();
    }
}
