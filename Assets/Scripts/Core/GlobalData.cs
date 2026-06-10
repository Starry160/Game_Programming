using UnityEngine;

/// <summary>Stores selected class, weapon, player stats, and run results across scene loads.</summary>
public static class GlobalData
{
    /// <summary>Animator controller selected at the class pedestal.</summary>
    public static RuntimeAnimatorController chosenAnimatorController;

    /// <summary>Weapon index selected at the class pedestal.</summary>
    public static int chosenWeaponIndex = -1;

    /// <summary>Whether the next scene should reuse the player's current health.</summary>
    public static bool hasPersistedHealth = false;

    /// <summary>Player health value carried into the next scene.</summary>
    public static float persistedHealth = 0f;

    /// <summary>Whether the next scene should reuse the player's max health.</summary>
    public static bool hasPersistedMaxHealth = false;

    /// <summary>Player max health value carried into the next scene.</summary>
    public static float persistedMaxHealth = 0f;

    /// <summary>Whether the next scene should reuse the player's max shield.</summary>
    public static bool hasPersistedMaxShield = false;

    /// <summary>Player max shield value carried into the next scene.</summary>
    public static float persistedMaxShield = 0f;

    /// <summary>Enemy kills recorded for the current run.</summary>
    public static int persistedKillCount = 0;
    /// <summary>Potions collected during the current run.</summary>
    public static int persistedPotionCollected = 0;
    /// <summary>Survival time recorded for the current run.</summary>
    public static float persistedSurvivalTime = 0f;

    /// <summary>Clears persistent run data when returning to the main menu.</summary>
    public static void ResetRunState()
    {
        chosenAnimatorController = null;
        chosenWeaponIndex = -1;
        hasPersistedHealth = false;
        persistedHealth = 0f;
        hasPersistedMaxHealth = false;
        persistedMaxHealth = 0f;
        hasPersistedMaxShield = false;
        persistedMaxShield = 0f;
        persistedKillCount = 0;
        persistedPotionCollected = 0;
        persistedSurvivalTime = 0f;
    }
}
