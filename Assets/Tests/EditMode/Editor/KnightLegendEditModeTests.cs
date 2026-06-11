using NUnit.Framework;
using UnityEngine;

public class KnightLegendEditModeTests
{
    private readonly System.Collections.Generic.List<GameObject> createdObjects =
        new System.Collections.Generic.List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        ClearRunState();
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            if (createdObjects[i] != null)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }
        }

        createdObjects.Clear();
        ClearRunState();
    }

    [Test]
    public void GlobalData_ResetRunState_ClearsClassStatsAndRunResults()
    {
        GlobalData.chosenAnimatorController = new AnimatorOverrideController();
        GlobalData.chosenWeaponIndex = 2;
        GlobalData.hasPersistedHealth = true;
        GlobalData.persistedHealth = 25f;
        GlobalData.hasPersistedMaxHealth = true;
        GlobalData.persistedMaxHealth = 150f;
        GlobalData.hasPersistedMaxShield = true;
        GlobalData.persistedMaxShield = 75f;
        GlobalData.persistedKillCount = 4;
        GlobalData.persistedPotionCollected = 3;
        GlobalData.persistedSurvivalTime = 91f;

        GlobalData.ResetRunState();

        Assert.IsNull(GlobalData.chosenAnimatorController);
        Assert.AreEqual(-1, GlobalData.chosenWeaponIndex);
        Assert.IsFalse(GlobalData.hasPersistedHealth);
        Assert.AreEqual(0f, GlobalData.persistedHealth);
        Assert.IsFalse(GlobalData.hasPersistedMaxHealth);
        Assert.AreEqual(0f, GlobalData.persistedMaxHealth);
        Assert.IsFalse(GlobalData.hasPersistedMaxShield);
        Assert.AreEqual(0f, GlobalData.persistedMaxShield);
        Assert.AreEqual(0, GlobalData.persistedKillCount);
        Assert.AreEqual(0, GlobalData.persistedPotionCollected);
        Assert.AreEqual(0f, GlobalData.persistedSurvivalTime);
    }

    [Test]
    public void RunStatsManager_AddKillAndPotion_SyncsGlobalSnapshot()
    {
        RunStatsManager manager = CreateRunStatsManager();

        manager.AddKill();
        manager.AddKill();
        manager.AddPotion();
        manager.survivalTime = 32.5f;
        manager.StopTimer();

        Assert.AreEqual(2, manager.killCount);
        Assert.AreEqual(1, manager.potionCollected);
        Assert.AreEqual(2, GlobalData.persistedKillCount);
        Assert.AreEqual(1, GlobalData.persistedPotionCollected);
        Assert.AreEqual(32.5f, GlobalData.persistedSurvivalTime);
    }

    [Test]
    public void RunStatsManager_ResetForMenu_ClearsCountersAndGlobalSnapshot()
    {
        RunStatsManager manager = CreateRunStatsManager();
        manager.AddKill();
        manager.AddPotion();
        manager.survivalTime = 12f;
        manager.StopTimer();

        RunStatsManager.ResetForMenu();

        Assert.AreEqual(0, manager.killCount);
        Assert.AreEqual(0, manager.potionCollected);
        Assert.AreEqual(0f, manager.survivalTime);
        Assert.AreEqual(0, GlobalData.persistedKillCount);
        Assert.AreEqual(0, GlobalData.persistedPotionCollected);
        Assert.AreEqual(0f, GlobalData.persistedSurvivalTime);
    }

    [Test]
    public void PlayerStats_IncreaseMaxHealthAndHeal_UpdatesCurrentAndPersistedHealth()
    {
        PlayerStats stats = CreatePlayerStats();
        stats.maxHealth = 100f;
        stats.currentHealth = 40f;

        stats.IncreaseMaxHealthAndHeal(25f);

        Assert.AreEqual(125f, stats.maxHealth);
        Assert.AreEqual(65f, stats.currentHealth);
        Assert.IsTrue(GlobalData.hasPersistedMaxHealth);
        Assert.AreEqual(125f, GlobalData.persistedMaxHealth);
        Assert.IsTrue(GlobalData.hasPersistedHealth);
        Assert.AreEqual(65f, GlobalData.persistedHealth);
    }

    [Test]
    public void PlayerStats_IncreaseMaxShieldAndFill_UpdatesCurrentAndPersistedShield()
    {
        PlayerStats stats = CreatePlayerStats();
        stats.maxShield = 50f;
        stats.currentShield = 10f;

        stats.IncreaseMaxShieldAndFill(15f);

        Assert.AreEqual(65f, stats.maxShield);
        Assert.AreEqual(25f, stats.currentShield);
        Assert.IsTrue(GlobalData.hasPersistedMaxShield);
        Assert.AreEqual(65f, GlobalData.persistedMaxShield);
    }

    private RunStatsManager CreateRunStatsManager()
    {
        GameObject gameObject = new GameObject("RunStatsManager_EditModeTest");
        createdObjects.Add(gameObject);
        RunStatsManager manager = gameObject.AddComponent<RunStatsManager>();

        // EditMode tests do not always mirror PlayMode lifecycle timing, so bind the
        // singleton explicitly before testing static reset paths.
        RunStatsManager.Instance = manager;
        manager.ResetStats();
        return manager;
    }

    private PlayerStats CreatePlayerStats()
    {
        GameObject gameObject = new GameObject("PlayerStats_EditModeTest");
        createdObjects.Add(gameObject);
        return gameObject.AddComponent<PlayerStats>();
    }

    private static void ClearRunState()
    {
        RunStatsManager.ResetForMenu();

        if (RunStatsManager.Instance != null)
        {
            Object.DestroyImmediate(RunStatsManager.Instance.gameObject);
        }

        RunStatsManager.Instance = null;
        GlobalData.ResetRunState();
    }
}
