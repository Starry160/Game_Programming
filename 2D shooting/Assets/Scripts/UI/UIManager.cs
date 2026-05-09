using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// A class which manages pages of UI elements
/// and the game's UI
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Page Management")]
    [Tooltip("The pages (Panels) managed by the UI Manager")]
    public List<UIPage> pages;
    [Tooltip("The index of the active page in the UI")]
    public int currentPage = 0;
    [Tooltip("The page (by index) switched to when the UI Manager starts up")]
    public int defaultPage = 0;

    [Header("Pause Settings")]
    [Tooltip("The index of the pause page in the pages list")]
    public int pausePageIndex = 1;
    [Tooltip("Whether or not to allow pausing")]
    public bool allowPause = true;

    [Header("Input Actions & Controls")]
    public InputAction pauseAction;
    
    [Header("Scene Navigation")]
    [Tooltip("Scene name for the game's main menu")]
    public string mainMenuSceneName = "MainMenu";

    // Whether or not the application is paused
    private bool isPaused = false;

    // A list of all UI element classes
    private List<UIelement> UIelements;

    // The event system handling UI navigation
    [HideInInspector]
    public EventSystem eventSystem;

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    private void OnEnable()
    {
        pauseAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    private void OnDisable()
    {
        pauseAction.Disable();
    }

    /// <summary>
    /// Description:
    /// Finds and stores all UIElements in the UIElements list
    /// Input:
    /// None
    /// Return:
    /// void (no return)
    /// </summary>
    private void SetUpUIElements()
    {
        UIelements = FindObjectsOfType<UIelement>().ToList();
    }

    /// <summary>
    /// Description:
    /// Gets the event system from the scene if one exists
    /// If one does not exist a warning will be displayed
    /// Input:
    /// None
    /// Return:
    /// void (no return)
    /// </summary>
    private void SetUpEventSystem()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("There is no event system in the scene but you are trying to use the UIManager. \n" +
                "All UI in Unity requires an Event System to run. \n" + 
                "You can add one by right clicking in hierarchy then selecting UI->EventSystem.");
        }
    }

    /// <summary>
    /// Description:
    /// If the game is paused, unpauses the game.
    /// If the game is not paused, pauses the game.
    /// Inputs:
    /// None
    /// Retuns:
    /// void (no return)
    /// </summary>
    public void TogglePause()
    {
        if (allowPause)
        {
            if (isPaused)
            {
                SetActiveAllPages(false);
                Time.timeScale = 1;
                isPaused = false;
            }
            else
            {
                GoToPage(pausePageIndex);
                Time.timeScale = 0;
                isPaused = true;
            }
        }      
    }

    /// <summary>
    /// Description:
    /// Goes through all UI elements and calls their UpdateUI function
    /// Input:
    /// None
    /// Return:
    /// void (no return)
    /// </summary>
    public void UpdateUI()
    {
        SetUpUIElements();
        foreach (UIelement uiElement in UIelements)
        {
            uiElement.UpdateUI();
        }
    }

    /// <summary>
    /// Standard Unity function called once when the script instance first exists in runtime of the game. Called before Start.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Description:
    /// Default Unity function that runs once when the script is first started and before Update
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void Start()
    {
        SetUpEventSystem();
        SetUpUIElements();
        UpdateUI();
    }

    /// <summary>
    /// Description:
    /// Default function from Unity that runs every frame
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void Update()
    {
        CheckPauseInput();
    }

    /// <summary>
    /// Description:
    /// If the input manager is set up, reads the pause input
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    private void CheckPauseInput()
    {
        if (pauseAction.triggered)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Description:
    /// Unpauses and returns to the main menu scene.
    /// This is intended for a Pause Menu "Main Menu" button.
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
    /// <summary>
    /// Description:
    /// Goes to a page by that page's index
    /// Inputs: 
    /// int page
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="pageIndex">The index in the page list to go to</param>
    public void GoToPage(int pageIndex)
    {
        if (pageIndex < pages.Count && pages[pageIndex] != null)
        {
            SetActiveAllPages(false);
            pages[pageIndex].gameObject.SetActive(true);
            pages[pageIndex].SetSelectedUIToDefault();
        }
    }

    /// <summary>
    /// Description:
    /// Goes to a page by that page's name
    /// Inputs: 
    /// string pageName
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="pageName">The name of the page in the game you want to go to, if their are duplicates this picks the first found</param>
    public void GoToPageByName(string pageName)
    {
        UIPage page = pages.Find(item => item.name == pageName);
        int pageIndex = pages.IndexOf(page);
        GoToPage(pageIndex);
    }

    /// <summary>
    /// Description:
    /// Turns all stored pages on or off depending on parameters
    /// Input: 
    /// bool enable
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="activated">The true or false value to set all page game object's activeness to</param>
    public void SetActiveAllPages(bool activated)
    {
        if (pages != null)
        {
            foreach (UIPage page in pages)
            {
                if (page != null)
                    page.gameObject.SetActive(activated);
            }
        }
    }
}
