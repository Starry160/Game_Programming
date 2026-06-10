using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Displays story pages one by one before loading the gameplay scene.</summary>
public class StoryIntroManager : MonoBehaviour
{
    [Header("Story")]
    [Tooltip("Story text pages shown before gameplay starts.")]
    [TextArea(3, 8)]
    public string[] storyPages;

    [Header("UI")]
    [Tooltip("TMP text component used to display the current story page.")]
    public TextMeshProUGUI storyText;

    [Header("Scene")]
    [Tooltip("Build index loaded after the final story page.")]
    [SerializeField] private int _gameSceneBuildIndex = 2;

    private int _currentPage;

    // Shows the story panel on the title scene until the player continues.
    private void Start()
    {
        _currentPage = 0;
        ShowCurrentPage();
    }

    /// <summary>
    /// Advances to the next story page or loads the gameplay scene.
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

    // Writes the current story page into the TMP text field.
    private void ShowCurrentPage()
    {
        if (storyText == null || storyPages == null || storyPages.Length == 0)
        {
            return;
        }

        storyText.text = storyPages[_currentPage];
    }
}
