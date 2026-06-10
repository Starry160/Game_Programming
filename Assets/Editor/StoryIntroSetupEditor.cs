using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Creates the story intro canvas, page button, and manager from the Unity editor.</summary>
public static class StoryIntroSetupEditor
{
    private const string MENU_PATH = "Tools/Generate Story Intro UI (Auto Setup)";

    [MenuItem(MENU_PATH)]
    public static void AutoSetup()
    {
        // 1. Canvas + CanvasScaler + GraphicRaycaster
        Canvas canvas = CreateCanvas();
        EnsureEventSystem();
        CreateFullscreenBackground(canvas.transform);
        TextMeshProUGUI storyText = CreateCenteredStoryText(canvas.transform);
        Button pageButton = CreateFullscreenInvisibleButton(canvas.transform);
        StoryIntroManager manager = CreateStoryManager(storyText);

        UnityEventTools.AddPersistentListener(pageButton.onClick, manager.NextPage);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = manager.gameObject;
        EditorUtility.DisplayDialog("Story Intro Setup",
            "Story intro UI has been generated. Configure storyPages on StoryManager.", "OK");
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasGo = new GameObject("StoryCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Story Canvas");

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject esGo = new GameObject("EventSystem",
            typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
    }

    private static void CreateFullscreenBackground(Transform parent)
    {
        GameObject bgGo = new GameObject("Background", typeof(Image));
        Undo.RegisterCreatedObjectUndo(bgGo, "Create Background");
        bgGo.transform.SetParent(parent, false);

        Image image = bgGo.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        StretchFullscreen(bgGo.GetComponent<RectTransform>());
    }

    private static TextMeshProUGUI CreateCenteredStoryText(Transform parent)
    {
        GameObject textGo = new GameObject("StoryText", typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(textGo, "Create Story Text");
        textGo.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "Enter story text here...";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.fontSize = 42f;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        RectTransform rect = tmp.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(200f, 200f);
        rect.offsetMax = new Vector2(-200f, -200f);

        return tmp;
    }

    private static Button CreateFullscreenInvisibleButton(Transform parent)
    {
        GameObject btnGo = new GameObject("NextPageButton", typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(btnGo, "Create Page Button");
        btnGo.transform.SetParent(parent, false);

        Image image = btnGo.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;

        Button button = btnGo.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;

        StretchFullscreen(btnGo.GetComponent<RectTransform>());

        btnGo.transform.SetAsLastSibling();

        return button;
    }

    private static StoryIntroManager CreateStoryManager(TextMeshProUGUI storyText)
    {
        GameObject mgrGo = new GameObject("StoryManager", typeof(StoryIntroManager));
        Undo.RegisterCreatedObjectUndo(mgrGo, "Create Story Manager");

        StoryIntroManager manager = mgrGo.GetComponent<StoryIntroManager>();
        manager.storyText = storyText;
        manager.storyPages = new string[]
        {
            "Page 1: Long ago, a forgotten memory slept deep within the dungeon...",
            "Page 2: With only courage, the hero stepped into the endless corridor.",
            "Page 3: Click the screen to begin your adventure."
        };

        return manager;
    }

    private static void StretchFullscreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
