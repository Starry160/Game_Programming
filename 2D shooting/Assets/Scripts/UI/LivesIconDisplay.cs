using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays remaining player lives by enabling/disabling a list of life icons.
/// </summary>
public class LivesIconDisplay : UIelement
{
    [Tooltip("The player's Health component that tracks currentLives")]
    public Health targetHealth = null;

    [Tooltip("Life icons in order (left to right). Index 0 stays on for life #1, etc.")]
    public List<GameObject> lifeIcons = new List<GameObject>();

    private void Start()
    {
        if (targetHealth == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                targetHealth = playerObject.GetComponent<Health>();
            }
        }

        UpdateUI();
    }

    public override void UpdateUI()
    {
        base.UpdateUI();

        int livesToShow = 0;
        if (targetHealth != null)
        {
            livesToShow = Mathf.Max(0, targetHealth.currentLives);
        }

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].SetActive(i < livesToShow);
            }
        }
    }
}
