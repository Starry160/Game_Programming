using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public static class UICharactersSetupTool
{
    private const string MenuPath = "Tools/Build MainMenu Character Showcase";
    private const string ShowcaseRootName = "UI_CharacterShowcase";
    private const string SpriteRoot = "Assets/Sprites/UICharacters";
    private const string AnimRoot = "Assets/Animations/UICharacters";
    private const float IdleFps = 12f;

    private static readonly string[] Roles = { "Player", "Mage", "Archer", "Knight", "Skeleton" };
    private static readonly string[] NodeNames =
    {
        "UI_Player_Animated",
        "UI_Mage_Animated",
        "UI_Archer_Animated",
        "UI_Knight_Animated",
        "UI_Skeleton_Animated"
    };

    [MenuItem(MenuPath)]
    public static void Build()
    {
        EnsureFolder("Assets", "Animations");
        EnsureFolder("Assets/Animations", "UICharacters");

        Canvas canvas = FindOrCreateCanvas();
        Transform root = FindOrCreateChild(canvas.transform, ShowcaseRootName);
        ConfigureRoot(root.GetComponent<RectTransform>());

        var missingRolePaths = new List<string>();
        var updatedRoles = new List<string>();

        for (int i = 0; i < Roles.Length; i++)
        {
            string role = Roles[i];
            string nodeName = NodeNames[i];

            Transform roleNode = FindOrCreateChild(root, nodeName);
            ConfigureRoleRect(roleNode.GetComponent<RectTransform>(), i, Roles.Length);

            Image image = GetOrAdd<Image>(roleNode.gameObject);
            image.raycastTarget = false;
            image.preserveAspect = true;

            GetOrAdd<Animator>(roleNode.gameObject);
            GetOrAdd<UICharacterAnimator>(roleNode.gameObject);

            string idleFolder = $"{SpriteRoot}/{role}/Idle";
            Sprite[] frames = LoadAndPrepareIdleSprites(idleFolder);
            if (frames == null || frames.Length == 0)
            {
                missingRolePaths.Add(idleFolder);
                continue;
            }

            image.sprite = frames[0];
            EnsureFolder(AnimRoot, role);

            string clipPath = $"{AnimRoot}/{role}/UI_{role}_Idle.anim";
            string controllerPath = $"{AnimRoot}/{role}/UI_{role}_Controller.controller";

            AnimationClip clip = BuildOrUpdateUiIdleClip(clipPath, frames);
            AnimatorController controller = BuildOrUpdateController(controllerPath, clip);

            Animator animator = roleNode.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            updatedRoles.Add(role);
        }

        EditorSceneDirty();
        Selection.activeTransform = root;

        string result = $"[UICharactersSetupTool] 完成。更新角色: {string.Join(", ", updatedRoles)}";
        if (missingRolePaths.Count > 0)
        {
            result += $"\n缺失 Idle 资源（已跳过）:\n- {string.Join("\n- ", missingRolePaths)}";
            Debug.LogWarning(result);
        }
        else
        {
            Debug.Log(result);
        }
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>(true);
        if (canvas != null)
        {
            EnsureEventSystemExists();
            return canvas;
        }

        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");

        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystemExists();
        return canvas;
    }

    private static void EnsureEventSystemExists()
    {
        if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(true) != null)
        {
            return;
        }

        GameObject esGo = new GameObject(
            "EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void ConfigureRoot(RectTransform rootRt)
    {
        if (rootRt == null)
        {
            return;
        }

        rootRt.anchorMin = new Vector2(0.5f, 0f);
        rootRt.anchorMax = new Vector2(0.5f, 0f);
        rootRt.pivot = new Vector2(0.5f, 0f);
        rootRt.sizeDelta = new Vector2(1400f, 360f);
        rootRt.anchoredPosition = new Vector2(0f, 60f);
    }

    private static void ConfigureRoleRect(RectTransform rt, int index, int total)
    {
        if (rt == null)
        {
            return;
        }

        const float spacing = 260f;
        float startX = -((total - 1) * spacing) * 0.5f;

        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(160f, 160f);
        rt.anchoredPosition = new Vector2(startX + index * spacing, 0f);
        rt.localScale = Vector3.one;
    }

    private static Sprite[] LoadAndPrepareIdleSprites(string idleFolder)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { idleFolder });
        if (guids == null || guids.Length == 0)
        {
            return Array.Empty<Sprite>();
        }

        var entries = new List<(string name, Sprite sprite)>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            PrepareTextureImporter(path);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                entries.Add((sprite.name, sprite));
            }
        }

        entries.Sort((a, b) => EditorUtility.NaturalCompare(a.name, b.name));

        Sprite[] frames = new Sprite[entries.Count];
        for (int i = 0; i < entries.Count; i++)
        {
            frames[i] = entries[i].sprite;
        }

        return frames;
    }

    private static void PrepareTextureImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool dirty = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            dirty = true;
        }

        if (importer.filterMode != FilterMode.Point)
        {
            importer.filterMode = FilterMode.Point;
            dirty = true;
        }

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            dirty = true;
        }

        if (dirty)
        {
            importer.SaveAndReimport();
        }
    }

    private static AnimationClip BuildOrUpdateUiIdleClip(string clipPath, Sprite[] frames)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.frameRate = IdleFps;
        clip.ClearCurves();

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(Image),
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / IdleFps,
                value = frames[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
        SetLoop(clip, true);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimatorController BuildOrUpdateController(string controllerPath, AnimationClip idleClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState idleState = FindOrCreateState(sm, "UI_Idle");
        idleState.motion = idleClip;
        sm.defaultState = idleState;

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimatorState FindOrCreateState(AnimatorStateMachine sm, string stateName)
    {
        ChildAnimatorState[] states = sm.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state.name == stateName)
            {
                return states[i].state;
            }
        }

        return sm.AddState(stateName);
    }

    private static void SetLoop(AnimationClip clip, bool loop)
    {
        SerializedObject so = new SerializedObject(clip);
        SerializedProperty settings = so.FindProperty("m_AnimationClipSettings");
        if (settings != null)
        {
            SerializedProperty loopTime = settings.FindPropertyRelative("m_LoopTime");
            if (loopTime != null)
            {
                loopTime.boolValue = loop;
            }
        }

        so.ApplyModifiedProperties();
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c != null)
        {
            return c;
        }

        return Undo.AddComponent<T>(go);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string full = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(full))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void EditorSceneDirty()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
