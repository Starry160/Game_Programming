using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class StoryIntroSetupEditor
{
    private const string MENU_PATH = "Tools/一键生成剧情界面 (Auto Setup)";

    [MenuItem(MENU_PATH)]
    public static void AutoSetup()
    {
        // 1. Canvas + CanvasScaler + GraphicRaycaster
        Canvas canvas = CreateCanvas();
        // 2. EventSystem（UI 交互必备）
        EnsureEventSystem();
        // 3. 全屏黑色背景
        CreateFullscreenBackground(canvas.transform);
        // 4. 居中文本
        TextMeshProUGUI storyText = CreateCenteredStoryText(canvas.transform);
        // 5. 全屏透明翻页热区按钮
        Button pageButton = CreateFullscreenInvisibleButton(canvas.transform);
        // 6. StoryManager 空物体并挂载脚本
        StoryIntroManager manager = CreateStoryManager(storyText);

        // 7. 自动绑定按钮 OnClick → manager.NextPage（持久化绑定，可在 Inspector 看到）
        UnityEventTools.AddPersistentListener(pageButton.onClick, manager.NextPage);

        // 标脏当前场景，提示保存
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = manager.gameObject;
        EditorUtility.DisplayDialog("Story Intro Setup",
            "剧情界面已生成完毕。\n请在 StoryManager 上配置 storyPages 文本即可。", "OK");
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
        tmp.text = "在这里输入剧情文本…";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.fontSize = 42f;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        // 在画面中央留出一块大的可读区域（左右各留 200，上下各留 200）
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
        // 透明 Image 仍可作为 raycast 目标用于点击检测。
        image.raycastTarget = true;

        Button button = btnGo.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;

        StretchFullscreen(btnGo.GetComponent<RectTransform>());

        // 让按钮位于最上层，覆盖文字与背景。
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
            "第一页：很久以前，地牢深处沉睡着一段被遗忘的记忆…",
            "第二页：勇者带着仅有的勇气，踏入了无尽的回廊。",
            "第三页：点击屏幕开始你的冒险。"
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
