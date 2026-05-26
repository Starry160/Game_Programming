using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>游戏内暂停菜单：Esc 暂停、返回主菜单确认。</summary>
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

    // 自动绑定 UI 引用并注册按钮事件。
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

    // 按层级路径查找暂停/确认面板与按钮。
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

    // 绑定各按钮 OnClick。
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

    // 检测 Esc 键。
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscPressed();
        }
    }

    // Esc：关闭确认框 / 恢复游戏 / 打开暂停。
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

    // 切换暂停（timeScale=0，显示面板）。
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

    // 恢复游戏时间并隐藏面板。
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

    // 显示返回主菜单确认框。
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

    // 取消退出，关闭确认框。
    public void CancelQuit()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    // 确认退出：ResetRunState 并加载主菜单。
    public void ConfirmQuitToMainMenu()
    {
        // 返回主菜单即视为放弃当前 run，清空跨场景职业/武器/生命等运行态。
        GlobalData.ResetRunState();
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
