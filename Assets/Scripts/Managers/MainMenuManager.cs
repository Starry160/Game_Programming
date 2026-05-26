using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("开始游戏时加载的场景 Build Index。")]
    [SerializeField] private int _gameSceneBuildIndex = 1;

    /// <summary>
    /// 供主菜单 "开始游戏" 按钮 OnClick 调用。
    /// </summary>
    public void StartGame()
    {
        // 从主菜单开始新游戏时，兜底清空上一局遗留的全局运行态。
        GlobalData.ResetRunState();
        SceneManager.LoadScene(_gameSceneBuildIndex);
    }

    /// <summary>
    /// 供主菜单 "退出游戏" 按钮 OnClick 调用。
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Editor 环境下直接停止 Play 模式，便于调试。
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
