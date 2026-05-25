using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ChestAnimationBuilder
{
    private const string ChestTilesetPath = "Assets/Sprites/Tile/0x72_DungeonTilesetII_v1.7.png";
    private const string OutputFolder = "Assets/Animations/Chest";
    private const string IdleClipPath = OutputFolder + "/Chest_Idle.anim";
    private const string OpenClipPath = OutputFolder + "/Chest_Open.anim";
    private const string ControllerPath = OutputFolder + "/Chest_Controller.controller";

    private const string OpenTriggerName = "open";
    private const float OpenFrameInterval = 0.1f;
    private const float OpenFps = 10f;

    [MenuItem("Tools/Build Chest Animation")]
    public static void BuildChestAnimation()
    {
        EnsureOutputFolderExists();

        // Force rebuild to avoid stale/broken assets.
        DeleteAssetIfExists(IdleClipPath);
        DeleteAssetIfExists(OpenClipPath);
        DeleteAssetIfExists(ControllerPath);

        Sprite idleSprite = FindChestSpriteByFrameOrLogError(286);
        Sprite openHalfSprite = FindChestSpriteByFrameOrLogError(298);
        Sprite openFullSprite = FindChestSpriteByFrameOrLogError(299);

        if (idleSprite == null)
        {
            return;
        }

        if (openHalfSprite == null || openFullSprite == null)
        {
            return;
        }

        AnimationClip idleClip = BuildClip(IdleClipPath, new[] { idleSprite }, 1f, false);
        AnimationClip openClip = BuildClip(OpenClipPath, new[] { openHalfSprite, openFullSprite }, OpenFrameInterval, false);

        BuildController(ControllerPath, idleClip, openClip);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Chest animation assets rebuilt successfully at Assets/Animations/Chest.");
    }

    private static void EnsureOutputFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets/Animations", "Chest");
        }
    }

    private static Sprite FindChestSpriteByFrameOrLogError(int frameIndex)
    {
        Sprite sprite = FindChestSpriteByFrame(frameIndex);
        if (sprite == null)
        {
            Debug.LogError($"ChestAnimationBuilder: Could not find chest sprite frame _{frameIndex}.");
        }

        return sprite;
    }

    private static Sprite FindChestSpriteByFrame(int frameIndex)
    {
        string suffix = "_" + frameIndex;

        // 1) Prefer sprites sliced from the known dungeon tileset texture.
        Object[] atlasAssets = AssetDatabase.LoadAllAssetsAtPath(ChestTilesetPath);
        for (int i = 0; i < atlasAssets.Length; i++)
        {
            Sprite sprite = atlasAssets[i] as Sprite;
            if (sprite != null && sprite.name.EndsWith(suffix))
            {
                return sprite;
            }
        }

        // 2) Fallback: search globally by frame suffix, in case the atlas got renamed/moved.
        List<Sprite> matches = new List<Sprite>();
        string[] spriteGuids = AssetDatabase.FindAssets($"*{suffix} t:Sprite");
        for (int i = 0; i < spriteGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && sprite.name.EndsWith(suffix))
            {
                matches.Add(sprite);
            }
        }

        if (matches.Count == 0)
        {
            return null;
        }

        if (matches.Count > 1)
        {
            Debug.LogWarning($"ChestAnimationBuilder: Found multiple sprites ending with {suffix}. Using '{matches[0].name}' from '{AssetDatabase.GetAssetPath(matches[0])}'.");
        }

        return matches[0];
    }

    private static void DeleteAssetIfExists(string assetPath)
    {
        if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
    }

    private static AnimationClip BuildClip(string clipPath, IReadOnlyList<Sprite> frames, float frameInterval, bool loopTime)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip
            {
                frameRate = OpenFps
            };
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.frameRate = OpenFps;
        clip.ClearCurves();

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameInterval,
                value = frames[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
        SetClipLoop(clip, loopTime);

        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void SetClipLoop(AnimationClip clip, bool loopTime)
    {
        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty settings = serializedClip.FindProperty("m_AnimationClipSettings");
        if (settings != null)
        {
            SerializedProperty loopProperty = settings.FindPropertyRelative("m_LoopTime");
            if (loopProperty != null)
            {
                loopProperty.boolValue = loopTime;
            }
        }

        serializedClip.ApplyModifiedProperties();
    }

    private static void BuildController(string controllerPath, AnimationClip idleClip, AnimationClip openClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        EnsureTriggerParameter(controller, OpenTriggerName);

        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine stateMachine = layer.stateMachine;

        AnimatorState idleState = GetOrCreateState(stateMachine, "Chest_Idle", new Vector3(250f, 120f, 0f));
        AnimatorState openState = GetOrCreateState(stateMachine, "Chest_Open", new Vector3(540f, 120f, 0f));

        idleState.motion = idleClip;
        openState.motion = openClip;
        stateMachine.defaultState = idleState;

        RemoveTransition(idleState, openState);
        AnimatorStateTransition transition = idleState.AddTransition(openState);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.hasFixedDuration = true;
        transition.exitTime = 0f;
        transition.AddCondition(AnimatorConditionMode.If, 0f, OpenTriggerName);

        EditorUtility.SetDirty(controller);
    }

    private static void EnsureTriggerParameter(AnimatorController controller, string parameterName)
    {
        for (int i = 0; i < controller.parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = controller.parameters[i];
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return;
            }
        }

        controller.AddParameter(parameterName, AnimatorControllerParameterType.Trigger);
    }

    private static AnimatorState GetOrCreateState(AnimatorStateMachine stateMachine, string stateName, Vector3 position)
    {
        ChildAnimatorState[] states = stateMachine.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state.name == stateName)
            {
                return states[i].state;
            }
        }

        return stateMachine.AddState(stateName, position);
    }

    private static void RemoveTransition(AnimatorState fromState, AnimatorState toState)
    {
        List<AnimatorStateTransition> transitionsToRemove = new List<AnimatorStateTransition>();

        for (int i = 0; i < fromState.transitions.Length; i++)
        {
            AnimatorStateTransition transition = fromState.transitions[i];
            if (transition.destinationState == toState)
            {
                transitionsToRemove.Add(transition);
            }
        }

        for (int i = 0; i < transitionsToRemove.Count; i++)
        {
            fromState.RemoveTransition(transitionsToRemove[i]);
        }
    }
}
