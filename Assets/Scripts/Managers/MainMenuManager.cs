using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Handles main menu start and quit actions.</summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Build index loaded when the player starts the game.")]
    [SerializeField] private int _gameSceneBuildIndex = 1;

    /// <summary>
    /// Starts a fresh run from the main menu.
    /// </summary>
    public void StartGame()
    {
        GlobalData.ResetRunState();
        RunStatsManager.BeginNewRun();
        SceneManager.LoadScene(_gameSceneBuildIndex);
    }

    /// <summary>
    /// Quits play mode in the editor or exits the built game.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
