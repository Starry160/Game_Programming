using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>本局战绩统计：击杀、药水与存活时间。</summary>
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Update()
    {
        if (!_isCounting)
        {
            return;
        }

        survivalTime += Time.deltaTime;
        SyncToGlobalSnapshot();
    }

    public void AddKill()
    {
        killCount++;
        SyncToGlobalSnapshot();
    }

    public void AddPotion()
    {
        potionCollected++;
        SyncToGlobalSnapshot();
    }

    public void StopTimer()
    {
        _isCounting = false;
        SyncToGlobalSnapshot();
    }

    public void ResetStats()
    {
        killCount = 0;
        potionCollected = 0;
        survivalTime = 0f;
        SyncToGlobalSnapshot();
    }

    /// <summary>
    /// 回到主菜单时调用：清空战绩并停止计时，等待下一局开始。
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
    /// 点击开始新游戏时调用：确保新一局从 0 开始并重新计时。
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

    private void SyncToGlobalSnapshot()
    {
        GlobalData.persistedKillCount = killCount;
        GlobalData.persistedPotionCollected = potionCollected;
        GlobalData.persistedSurvivalTime = survivalTime;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_hasInitializedThisRun)
        {
            return;
        }

        // 只在 TalentRoom 传送门到达 Level_01 后开始计时。
        if (!_hasStartedRunTimer && string.Equals(scene.name, "Level_01", System.StringComparison.Ordinal))
        {
            _hasStartedRunTimer = true;
            _isCounting = true;
            Debug.Log($"[RunStatsManager] Timer started on scene '{scene.name}'. survivalTime={survivalTime:F2}");
            return;
        }

        // 计时起点前始终不累计。
        if (!_hasStartedRunTimer)
        {
            _isCounting = false;
        }
    }
}
