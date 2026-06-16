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
        GlobalData.hasPersistedShield = true;
        GlobalData.persistedShield = 45f;
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
        Assert.IsFalse(GlobalData.hasPersistedShield);
        Assert.AreEqual(0f, GlobalData.persistedShield);
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
    public void RunStatsManager_BeginNewRunWithoutInstance_ClearsPersistedRunCounters()
    {
        GlobalData.persistedKillCount = 9;
        GlobalData.persistedPotionCollected = 6;
        GlobalData.persistedSurvivalTime = 144f;

        RunStatsManager.BeginNewRun();

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

    [Test]
    public void PlayerStats_ApplyClassBaseStats_SetsAndPersistsHealthAndShield()
    {
        PlayerStats stats = CreatePlayerStats();

        stats.ApplyClassBaseStats(8f, 3f);

        Assert.AreEqual(8f, stats.maxHealth);
        Assert.AreEqual(8f, stats.currentHealth);
        Assert.AreEqual(3f, stats.maxShield);
        Assert.AreEqual(3f, stats.currentShield);
        Assert.IsTrue(GlobalData.hasPersistedMaxHealth);
        Assert.AreEqual(8f, GlobalData.persistedMaxHealth);
        Assert.IsTrue(GlobalData.hasPersistedHealth);
        Assert.AreEqual(8f, GlobalData.persistedHealth);
        Assert.IsTrue(GlobalData.hasPersistedMaxShield);
        Assert.AreEqual(3f, GlobalData.persistedMaxShield);
        Assert.IsTrue(GlobalData.hasPersistedShield);
        Assert.AreEqual(3f, GlobalData.persistedShield);
    }

    [Test]
    public void PlayerStats_StatUpgradeMethods_IgnoreNonPositiveAmounts()
    {
        PlayerStats stats = CreatePlayerStats();
        stats.maxHealth = 100f;
        stats.currentHealth = 40f;
        stats.maxShield = 50f;
        stats.currentShield = 10f;

        stats.IncreaseMaxHealthAndHeal(0f);
        stats.IncreaseMaxHealthAndHeal(-5f);
        stats.IncreaseMaxShieldAndFill(0f);
        stats.IncreaseMaxShieldAndFill(-5f);

        Assert.AreEqual(100f, stats.maxHealth);
        Assert.AreEqual(40f, stats.currentHealth);
        Assert.AreEqual(50f, stats.maxShield);
        Assert.AreEqual(10f, stats.currentShield);
        Assert.IsFalse(GlobalData.hasPersistedMaxHealth);
        Assert.IsFalse(GlobalData.hasPersistedMaxShield);
    }

    [Test]
    public void PlayerStats_PersistCurrentStats_SavesHealthAndShieldSnapshot()
    {
        PlayerStats stats = CreatePlayerStats();
        stats.maxHealth = 120f;
        stats.currentHealth = 80f;
        stats.maxShield = 40f;
        stats.currentShield = 15f;

        stats.PersistCurrentStats();

        Assert.IsTrue(GlobalData.hasPersistedMaxHealth);
        Assert.AreEqual(120f, GlobalData.persistedMaxHealth);
        Assert.IsTrue(GlobalData.hasPersistedHealth);
        Assert.AreEqual(80f, GlobalData.persistedHealth);
        Assert.IsTrue(GlobalData.hasPersistedMaxShield);
        Assert.AreEqual(40f, GlobalData.persistedMaxShield);
        Assert.IsTrue(GlobalData.hasPersistedShield);
        Assert.AreEqual(15f, GlobalData.persistedShield);
    }

    [Test]
    public void WeaponManager_SwitchWeapon_ActivatesOnlyRequestedWeapon()
    {
        WeaponManager manager = CreateWeaponManagerWithWeapons(3, out GameObject[] weapons);

        manager.SwitchWeapon(1);

        Assert.IsFalse(weapons[0].activeSelf);
        Assert.IsTrue(weapons[1].activeSelf);
        Assert.IsFalse(weapons[2].activeSelf);
    }

    [Test]
    public void WeaponManager_SwitchWeapon_InvalidIndex_HidesAllWeapons()
    {
        WeaponManager manager = CreateWeaponManagerWithWeapons(3, out GameObject[] weapons);
        manager.SwitchWeapon(2);

        manager.SwitchWeapon(5);

        Assert.IsFalse(weapons[0].activeSelf);
        Assert.IsFalse(weapons[1].activeSelf);
        Assert.IsFalse(weapons[2].activeSelf);
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

    private WeaponManager CreateWeaponManagerWithWeapons(int weaponCount, out GameObject[] weapons)
    {
        GameObject managerObject = new GameObject("WeaponManager_EditModeTest");
        createdObjects.Add(managerObject);

        WeaponManager manager = managerObject.AddComponent<WeaponManager>();
        weapons = new GameObject[weaponCount];
        for (int i = 0; i < weaponCount; i++)
        {
            GameObject weapon = new GameObject($"Weapon_{i}");
            weapon.transform.SetParent(managerObject.transform);
            weapon.SetActive(true);
            createdObjects.Add(weapon);
            weapons[i] = weapon;
        }

        manager.weapons = weapons;
        return manager;
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
