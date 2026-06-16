using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the pause menu during gameplay. It handles Escape input, time scale changes,
/// resume behavior, return-to-menu confirmation, and run-state cleanup when leaving a run.
/// </summary>
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

    // Finds pause menu UI references and ensures the game starts unpaused.
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

    // Finds pause menu panels and buttons from the scene hierarchy.
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

    // Connects pause menu buttons to their runtime actions.
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

    // Toggles pause state from keyboard input every frame.
    private void Update()
    {
        if (GameOverPanel.IsShowing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscPressed();
        }
    }

    // Uses Escape to close confirmation, resume, or open the pause menu.
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

    // Switches between paused and unpaused gameplay states.
    public void TogglePause()
    {
        if (GameOverPanel.IsShowing)
        {
            return;
        }

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

    // Restores time scale and hides the pause menu.
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

    // Shows the return-to-menu confirmation panel.
    public void ShowQuitConfirm()
    {
        if (GameOverPanel.IsShowing)
        {
            return;
        }

        if (!isPaused)
        {
            TogglePause();
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
    }

    // Closes the return-to-menu confirmation panel.
    public void CancelQuit()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    // Clears run state and loads the main menu scene.
    public void ConfirmQuitToMainMenu()
    {
        RunStatsManager.ResetForMenu();
        GlobalData.ResetRunState();
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
