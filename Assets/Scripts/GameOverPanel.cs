using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>战绩结算面板：展示数据并返回主菜单。</summary>
public class GameOverPanel : MonoBehaviour
{
    public enum ResultType
    {
        Death,
        Victory
    }

    public static bool IsShowing { get; private set; }

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _killsText;
    [SerializeField] private TextMeshProUGUI _potionsText;
    [SerializeField] private TextMeshProUGUI _timeText;

    [Header("Death Visual")]
    [SerializeField] private string _deathTitle = "Battle Results";
    [SerializeField] private Color _deathPanelColor = new Color(0f, 0f, 0f, 0.78f);
    [SerializeField] private Color _deathTitleColor = new Color(0.91823894f, 0.06641343f, 0.20361686f, 1f);

    [Header("Victory Visual")]
    [SerializeField] private Image _panelBackground;
    [SerializeField] private string _victoryTitle = "你成功通过了试炼";
    [SerializeField] private Color _victoryPanelColor = new Color(0.16f, 0.28f, 0.5f, 0.9f);
    [SerializeField] private Color _victoryTitleColor = new Color(1f, 0.87f, 0.35f, 1f);

    [Header("Scene")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        EnsureCanvasGroup();
        HideImmediately();
        IsShowing = false;
    }

    /// <summary>
    /// 运行时准备：即使场景里默认 Inactive，也会自动启用并保持隐藏待命。
    /// </summary>
    public void PrepareForRuntime()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        EnsureCanvasGroup();
        HideImmediately();
        IsShowing = false;
    }

    public void ShowPanel()
    {
        ShowPanel(ResultType.Death, 0.35f);
    }

    public void ShowPanel(ResultType resultType)
    {
        ShowPanel(resultType, 0.35f);
    }

    public void ShowPanel(float fadeDuration)
    {
        ShowPanel(ResultType.Death, fadeDuration);
    }

    public void ShowPanel(ResultType resultType, float fadeDuration)
    {
        gameObject.SetActive(true);
        EnsureCanvasGroup();
        ApplyVisual(resultType);
        FillTexts();
        IsShowing = true;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeInRoutine(fadeDuration));
        }
    }

    public void OnMainMenuButtonPressed()
    {
        IsShowing = false;
        RunStatsManager.ResetForMenu();
        GlobalData.ResetRunState();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuSceneName);
    }

    private void FillTexts()
    {
        int kills = GlobalData.persistedKillCount;
        int potions = GlobalData.persistedPotionCollected;
        float time = GlobalData.persistedSurvivalTime;

        if (RunStatsManager.Instance != null)
        {
            kills = RunStatsManager.Instance.killCount;
            potions = RunStatsManager.Instance.potionCollected;
            time = RunStatsManager.Instance.survivalTime;
        }

        if (_killsText != null)
        {
            _killsText.text = $"Kill Enemy: {kills}";
        }

        if (_potionsText != null)
        {
            _potionsText.text = $"Get Potion: {potions}";
        }

        if (_timeText != null)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(time));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            _timeText.text = $"Survive: {minutes:00}:{seconds:00}";
        }
    }

    private void ApplyVisual(ResultType resultType)
    {
        string targetTitle = resultType == ResultType.Victory ? _victoryTitle : _deathTitle;
        Color targetTitleColor = resultType == ResultType.Victory ? _victoryTitleColor : _deathTitleColor;
        Color targetPanelColor = resultType == ResultType.Victory ? _victoryPanelColor : _deathPanelColor;

        if (_titleText != null)
        {
            _titleText.text = targetTitle;
            _titleText.color = targetTitleColor;
        }

        if (_panelBackground != null)
        {
            _panelBackground.color = targetPanelColor;
        }
    }

    private IEnumerator FadeInRoutine(float fadeDuration)
    {
        if (_canvasGroup == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _fadeCoroutine = null;
    }

    private void EnsureCanvasGroup()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void HideImmediately()
    {
        if (_canvasGroup == null)
        {
            return;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
