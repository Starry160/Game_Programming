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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetStats();
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
}
