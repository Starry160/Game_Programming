using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryIntroManager : MonoBehaviour
{
    [Header("Story")]
    [Tooltip("剧情文本数组，每个元素代表一页。")]
    [TextArea(3, 8)]
    public string[] storyPages;

    [Header("UI")]
    [Tooltip("用于显示剧情文字的 TMP 文本组件。")]
    public TextMeshProUGUI storyText;

    [Header("Scene")]
    [Tooltip("剧情结束后跳转到的目标场景 Build Index。")]
    [SerializeField] private int _gameSceneBuildIndex = 2;

    private int _currentPage;

    private void Start()
    {
        _currentPage = 0;
        ShowCurrentPage();
    }

    /// <summary>
    /// 翻到下一页；若已经是最后一页则加载游戏主场景。
    /// </summary>
    public void NextPage()
    {
        _currentPage++;

        if (_currentPage >= storyPages.Length)
        {
            SceneManager.LoadScene(_gameSceneBuildIndex);
            return;
        }

        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        if (storyText == null || storyPages == null || storyPages.Length == 0)
        {
            return;
        }

        storyText.text = storyPages[_currentPage];
    }
}
