using System;
using UnityEditor;
using UnityEngine;

public static class EnemyPhysicsSetup
{
    private const string MenuPath = "Tools/Configure Enemy Anti-Jamming";
    private const string PlayerLayerName = "Player";
    private const string EnemyLayerName = "Enemy";
    private const string EnemySensorLayerName = "EnemySensor";
    private const string SensorChildName = "SeparationSensor";

    [MenuItem(MenuPath)]
    public static void Configure()
    {
        int playerLayer = EnsureLayer(PlayerLayerName);
        int enemyLayer = EnsureLayer(EnemyLayerName);
        int enemySensorLayer = EnsureLayer(EnemySensorLayerName);

        if (playerLayer < 0 || enemyLayer < 0 || enemySensorLayer < 0)
        {
            Debug.LogError("[EnemyPhysicsSetup] Layer creation failed. Check whether TagManager.asset is writable.");
            return;
        }

        ConfigureCollisionMatrix(playerLayer, enemyLayer, enemySensorLayer);
        AssignLayersToSceneObjects(playerLayer, enemyLayer);
        SetupEnemySensors(enemySensorLayer);

        EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[EnemyPhysicsSetup] Setup complete: layers, collision matrix, and enemy sensors configured.");
    }

    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0)
        {
            return existing;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        if (layersProp == null)
        {
            return -1;
        }

        for (int i = 8; i <= 31; i++)
        {
            SerializedProperty slot = layersProp.GetArrayElementAtIndex(i);
            if (slot == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(slot.stringValue))
            {
                slot.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                tagManager.Update();
                return LayerMask.NameToLayer(layerName);
            }
        }

        Debug.LogError($"[EnemyPhysicsSetup] No free layer slot is available for {layerName}.");
        return -1;
    }

    private static void ConfigureCollisionMatrix(int playerLayer, int enemyLayer, int enemySensorLayer)
    {
        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, false);

        Physics2D.IgnoreLayerCollision(enemyLayer, 0, false);

        for (int i = 0; i < 32; i++)
        {
            bool shouldIgnore = i != enemySensorLayer;
            Physics2D.IgnoreLayerCollision(enemySensorLayer, i, shouldIgnore);
        }

        Physics2D.IgnoreLayerCollision(enemySensorLayer, enemySensorLayer, false);
    }

    private static void AssignLayersToSceneObjects(int playerLayer, int enemyLayer)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                Undo.RecordObject(players[i], "Assign Player Layer");
                players[i].layer = playerLayer;
            }
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                Undo.RecordObject(enemies[i], "Assign Enemy Layer");
                enemies[i].layer = enemyLayer;
            }
        }
    }

    private static void SetupEnemySensors(int enemySensorLayer)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            Collider2D mainCollider = enemy.GetComponent<Collider2D>();
            if (rb == null || mainCollider == null)
            {
                Debug.LogWarning($"[EnemyPhysicsSetup] Skipping {enemy.name}: missing Rigidbody2D or Collider2D.");
                continue;
            }

            Transform sensor = enemy.transform.Find(SensorChildName);
            if (sensor == null)
            {
                GameObject sensorGo = new GameObject(SensorChildName);
                Undo.RegisterCreatedObjectUndo(sensorGo, "Create SeparationSensor");
                sensorGo.transform.SetParent(enemy.transform, false);
                sensor = sensorGo.transform;
            }

            GameObject sensorObj = sensor.gameObject;
            Undo.RecordObject(sensorObj, "Setup SeparationSensor");
            sensorObj.layer = enemySensorLayer;
            sensor.localPosition = Vector3.zero;
            sensor.localRotation = Quaternion.identity;
            sensor.localScale = Vector3.one;

            CircleCollider2D circle = sensorObj.GetComponent<CircleCollider2D>();
            if (circle == null)
            {
                circle = Undo.AddComponent<CircleCollider2D>(sensorObj);
            }

            circle.isTrigger = true;
            circle.radius = EstimateSensorRadius(mainCollider) * 1.2f;

            EnemyPushSeparation push = sensorObj.GetComponent<EnemyPushSeparation>();
            if (push == null)
            {
                Undo.AddComponent<EnemyPushSeparation>(sensorObj);
            }
        }
    }

    private static float EstimateSensorRadius(Collider2D mainCollider)
    {
        switch (mainCollider)
        {
            case CircleCollider2D c:
                return Mathf.Max(0.2f, c.radius);
            case CapsuleCollider2D cap:
                return Mathf.Max(0.2f, Mathf.Max(cap.size.x, cap.size.y) * 0.5f);
            case BoxCollider2D b:
                return Mathf.Max(0.2f, Mathf.Max(b.size.x, b.size.y) * 0.5f);
            default:
                Bounds bounds = mainCollider.bounds;
                return Mathf.Max(0.2f, Mathf.Max(bounds.extents.x, bounds.extents.y));
        }
    }
}
