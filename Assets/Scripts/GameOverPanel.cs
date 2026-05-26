using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>战绩结算面板：展示数据并返回主菜单。</summary>
public class GameOverPanel : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _killsText;
    [SerializeField] private TextMeshProUGUI _potionsText;
    [SerializeField] private TextMeshProUGUI _timeText;

    [Header("Scene")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    public void ShowPanel()
    {
        ShowPanel(0.35f);
    }

    public void ShowPanel(float fadeDuration)
    {
        gameObject.SetActive(true);
        FillTexts();

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
        GlobalData.ResetRunState();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuSceneName);
    }

    private void FillTexts()
    {
        int kills = 0;
        int potions = 0;
        float time = 0f;

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
}
