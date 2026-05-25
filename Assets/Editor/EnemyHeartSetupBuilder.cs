using UnityEditor;
using UnityEngine;

public static class EnemyHeartSetupBuilder
{
    private const string MenuPath = "Tools/Setup Enemy Floating Heart";
    private const string HeartName = "FloatingHeart";
    private const string TilesetPath = "Assets/Sprites/Tile/0x72_DungeonTilesetII_v1.7.png";

    [MenuItem(MenuPath)]
    public static void SetupEnemyFloatingHeart()
    {
        GameObject enemy = Selection.activeGameObject;
        if (enemy == null)
        {
            Debug.LogError("[EnemyHeartSetupBuilder] 请先在 Hierarchy 选中一个小骷髅敌人对象。");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(enemy, "Setup Enemy Floating Heart");

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = Undo.AddComponent<EnemyHealth>(enemy);
        }

        Transform heartTransform = enemy.transform.Find(HeartName);
        GameObject heartObject;
        if (heartTransform == null)
        {
            heartObject = new GameObject(HeartName);
            Undo.RegisterCreatedObjectUndo(heartObject, "Create FloatingHeart");
            heartObject.transform.SetParent(enemy.transform);
        }
        else
        {
            heartObject = heartTransform.gameObject;
        }

        heartObject.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        heartObject.transform.localRotation = Quaternion.identity;
        heartObject.transform.localScale = Vector3.one;

        SpriteRenderer enemyRenderer = enemy.GetComponent<SpriteRenderer>();
        SpriteRenderer heartRenderer = heartObject.GetComponent<SpriteRenderer>();
        if (heartRenderer == null)
        {
            heartRenderer = Undo.AddComponent<SpriteRenderer>(heartObject);
        }

        heartRenderer.sprite = FindHeartSpriteByFrame(262);
        if (enemyRenderer != null)
        {
            heartRenderer.sortingLayerID = enemyRenderer.sortingLayerID;
            heartRenderer.sortingOrder = enemyRenderer.sortingOrder + 1;
        }

        enemyHealth._heartSpriteRenderer = heartRenderer;

        SerializedObject so = new SerializedObject(enemyHealth);
        so.FindProperty("fullHeartSprite").objectReferenceValue = FindHeartSpriteByFrame(262);
        so.FindProperty("halfHeartSprite").objectReferenceValue = FindHeartSpriteByFrame(263);
        so.FindProperty("emptyHeartSprite").objectReferenceValue = FindHeartSpriteByFrame(264);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(heartObject);
        EditorUtility.SetDirty(enemyHealth);
        Debug.Log($"[EnemyHeartSetupBuilder] {enemy.name} 的悬浮爱心配置完成。");
    }

    private static Sprite FindHeartSpriteByFrame(int frameIndex)
    {
        string suffix = "_" + frameIndex;
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(TilesetPath);
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
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

        Debug.LogError($"[EnemyHeartSetupBuilder] 未找到心形帧 _{frameIndex}。");
        return null;
    }
}
