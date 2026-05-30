using UnityEngine;

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
        }

        _isCounting = true;
    }

    private void Update()
    {
        if (!_isCounting)
        {
            return;
        }

        survivalTime += Time.deltaTime;
    }

    public void AddKill()
    {
        killCount++;
    }

    public void AddPotion()
    {
        potionCollected++;
    }

    public void StopTimer()
    {
        _isCounting = false;
    }

    public void ResetStats()
    {
        killCount = 0;
        potionCollected = 0;
        survivalTime = 0f;
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
    }

    /// <summary>
    /// 点击开始新游戏时调用：确保新一局从 0 开始并重新计时。
    /// </summary>
    public static void BeginNewRun()
    {
        if (Instance != null)
        {
            Instance.ResetStats();
            Instance._isCounting = true;
        }

        _hasInitializedThisRun = true;
    }
}
