using UnityEditor;
using UnityEngine;

/// <summary>编辑器工具：创建带 PotionItem 的药水预制体。</summary>
public static class PotionSetupBuilder
{
    private const string MenuPath = "Tools/Create Potion Prefab";
    private const string PotionPrefabPath = "Assets/Prefabs/Potion_241.prefab";
    private const string TilesetPath = "Assets/Sprites/Tile/0x72_DungeonTilesetII_v1.7.png";

    // 菜单入口：生成 Potion_241 预制体。
    [MenuItem(MenuPath)]
    public static void CreatePotionPrefab()
    {
        EnsureFolder("Assets", "Prefabs");

        Sprite potionSprite = FindPotionSpriteByFrame(241);
        if (potionSprite == null)
        {
            Debug.LogError("[PotionSetupBuilder] 未找到药水精灵 _241，无法创建预制体。");
            return;
        }

        GameObject root = new GameObject("Potion_241");
        Undo.RegisterCreatedObjectUndo(root, "Create Potion Prefab Root");

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = potionSprite;

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        BoxCollider2D box = root.AddComponent<BoxCollider2D>();
        box.sharedMaterial = null;
        box.isTrigger = true;

        root.AddComponent<PotionItem>();

        PrefabUtility.SaveAsPrefabAsset(root, PotionPrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[PotionSetupBuilder] 药水预制体已创建：{PotionPrefabPath}");
    }

    // 从图集按帧后缀查找药水 Sprite。
    private static Sprite FindPotionSpriteByFrame(int frameIndex)
    {
        string suffix = "_" + frameIndex;
        Object[] atlasAssets = AssetDatabase.LoadAllAssetsAtPath(TilesetPath);
        for (int i = 0; i < atlasAssets.Length; i++)
        {
            Sprite sprite = atlasAssets[i] as Sprite;
            if (sprite != null && sprite.name.EndsWith(suffix))
            {
                return sprite;
            }
        }

        string[] guids = AssetDatabase.FindAssets($"*{suffix} t:Sprite");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && sprite.name.EndsWith(suffix))
            {
                return sprite;
            }
        }

        return null;
    }

    // 若不存在则创建 Asset 子文件夹。
    private static void EnsureFolder(string parent, string child)
    {
        string fullPath = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
