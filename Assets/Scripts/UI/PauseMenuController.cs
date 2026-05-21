using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject confirmPanel;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button backToMainButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        AutoBindReferences();
        WireButtons();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        isPaused = false;
    }

    private void AutoBindReferences()
    {
        if (pausePanel == null)
        {
            Transform t = transform.Find("PausePanel");
            if (t != null) pausePanel = t.gameObject;
        }

        if (confirmPanel == null)
        {
            Transform t = transform.Find("PausePanel/ConfirmPanel");
            if (t != null) confirmPanel = t.gameObject;
        }

        if (pauseButton == null)
        {
            pauseButton = transform.Find("PauseButton")?.GetComponent<Button>();
        }

        if (resumeButton == null)
        {
            resumeButton = transform.Find("PausePanel/ResumeButton")?.GetComponent<Button>();
        }

        if (backToMainButton == null)
        {
            backToMainButton = transform.Find("PausePanel/BackToMainButton")?.GetComponent<Button>();
        }

        if (yesButton == null)
        {
            yesButton = transform.Find("PausePanel/ConfirmPanel/YesButton")?.GetComponent<Button>();
        }

        if (noButton == null)
        {
            noButton = transform.Find("PausePanel/ConfirmPanel/NoButton")?.GetComponent<Button>();
        }
    }

    private void WireButtons()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
            pauseButton.onClick.AddListener(TogglePause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (backToMainButton != null)
        {
            backToMainButton.onClick.RemoveListener(ShowQuitConfirm);
            backToMainButton.onClick.AddListener(ShowQuitConfirm);
        }

        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(ConfirmQuitToMainMenu);
            yesButton.onClick.AddListener(ConfirmQuitToMainMenu);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(CancelQuit);
            noButton.onClick.AddListener(CancelQuit);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscPressed();
        }
    }

    private void HandleEscPressed()
    {
        if (confirmPanel != null && confirmPanel.activeSelf)
        {
            CancelQuit();
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    public void ShowQuitConfirm()
    {
        if (!isPaused)
        {
            TogglePause();
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
    }

    public void CancelQuit()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    public void ConfirmQuitToMainMenu()
    {
        // 返回主菜单即视为放弃当前 run，清空跨场景职业/武器/生命等运行态。
        GlobalData.ResetRunState();
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
