using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent run-stat tracker. It counts kills, potion pickups, and survival time across scenes,
/// then syncs those values into GlobalData so death and victory panels can display them.
/// </summary>
public class RunStatsManager : MonoBehaviour
{
    public static RunStatsManager Instance;

    [Header("Run Stats")]
    public int killCount;
    public int potionCollected;
    public float survivalTime;

    private bool _isCounting;
    private static bool _hasInitializedThisRun;
    private static bool _hasStartedRunTimer;

    // Creates the persistent run-stat singleton that tracks timer and kill count.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!_hasInitializedThisRun)
        {
            ResetStats();
            _hasInitializedThisRun = true;
            _hasStartedRunTimer = false;
        }

        _isCounting = _hasStartedRunTimer;
    }

    // Watches scene loads so run stats stay valid across gameplay scenes.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    // Stops watching scene-load events when the tracker is disabled.
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    // Advances the run timer while gameplay is active.
    private void Update()
    {
        if (!_isCounting)
        {
            return;
        }

        survivalTime += Time.deltaTime;
        SyncToGlobalSnapshot();
    }

    // Adds one kill to the run statistics.
    public void AddKill()
    {
        killCount++;
        SyncToGlobalSnapshot();
    }

    // Adds one potion pickup to the run statistics.
    public void AddPotion()
    {
        potionCollected++;
        SyncToGlobalSnapshot();
    }

    // Stops survival time accumulation for the current run.
    public void StopTimer()
    {
        _isCounting = false;
        SyncToGlobalSnapshot();
    }

    // Clears all current run counters.
    public void ResetStats()
    {
        killCount = 0;
        potionCollected = 0;
        survivalTime = 0f;
        SyncToGlobalSnapshot();
    }

    /// <summary>
    /// Clears counters and timer state when the player returns to the main menu.
    /// </summary>
    public static void ResetForMenu()
    {
        if (Instance != null)
        {
            Instance.ResetStats();
            Instance._isCounting = false;
        }

        _hasInitializedThisRun = false;
        _hasStartedRunTimer = false;
    }

    /// <summary>
    /// Starts a fresh run by resetting saved counters before gameplay begins.
    /// </summary>
    public static void BeginNewRun()
    {
        if (Instance != null)
        {
            Instance.ResetStats();
            Instance._isCounting = false;
        }
        else
        {
            GlobalData.persistedKillCount = 0;
            GlobalData.persistedPotionCollected = 0;
            GlobalData.persistedSurvivalTime = 0f;
        }

        _hasInitializedThisRun = true;
        _hasStartedRunTimer = false;
    }

    // Copies current run statistics into GlobalData.
    private void SyncToGlobalSnapshot()
    {
        GlobalData.persistedKillCount = killCount;
        GlobalData.persistedPotionCollected = potionCollected;
        GlobalData.persistedSurvivalTime = survivalTime;
    }

    // Starts or stops run timing based on the loaded scene.
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_hasInitializedThisRun)
        {
            return;
        }

        if (!_hasStartedRunTimer && string.Equals(scene.name, "Level_01", System.StringComparison.Ordinal))
        {
            _hasStartedRunTimer = true;
            _isCounting = true;
            return;
        }

        if (!_hasStartedRunTimer)
        {
            _isCounting = false;
        }
    }
}
