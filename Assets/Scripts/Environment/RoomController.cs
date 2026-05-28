using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>房间战斗：玩家进入锁门、清怪后开门并显示宝箱。</summary>
public class RoomController : MonoBehaviour
{
    [Header("Room Data")]
    public List<GameObject> enemiesInRoom = new List<GameObject>();
    public List<GameObject> roomGates = new List<GameObject>();

    [Header("Check Settings")]
    [SerializeField] private float enemyCheckInterval = 0.2f;
    [SerializeField] private string playerTag = "Player";

    [Header("Treasure Reveal")]
    [SerializeField] private GameObject _treasureBox;

    [Header("Gate Hide Timing")]
    [Tooltip("清怪后，DungeonDoor 延迟隐藏时间（秒）。")]
    [SerializeField] private float dungeonDoorHideDelay = 0.8f;
    [Tooltip("DungeonDoor 渐隐时长（秒）。")]
    [SerializeField] private float dungeonDoorFadeDuration = 0.35f;

    [Header("Trigger Fallback")]
    [Tooltip("优先使用这个触发区做玩家进入检测（推荐绑定 RoomTriggerZone）。")]
    [SerializeField] private Collider2D roomTriggerZone;

    private bool isRoomCleared = false;
    private bool isBattleStarted = false;
    private float nextCheckTime = 0f;

    // 自动绑定房间触发区。
    private void Awake()
    {
        AutoBindTriggerZone();
    }

    // 开局隐藏宝箱。
    private void Start()
    {
        if (_treasureBox != null)
        {
            _treasureBox.SetActive(false);
        }
    }

    // 编辑器菜单：从子物体 DoorController 同步 roomGates 列表。
    [ContextMenu("Sync Room Gates From Children")]
    private void SyncRoomGatesFromChildren()
    {
        DoorController[] childDoors = GetComponentsInChildren<DoorController>(true);
        roomGates.Clear();
        for (int i = 0; i < childDoors.Length; i++)
        {
            roomGates.Add(childDoors[i].gameObject);
        }

        Debug.Log($"[RoomController] 已同步 {roomGates.Count} 扇门到 roomGates（仅这些门会被房间逻辑控制）。", this);
    }

    // Inspector 变更时校验引用并去重。
    private void OnValidate()
    {
        AutoBindTriggerZone();

        if (roomTriggerZone == null)
        {
            Debug.LogWarning("[RoomController] 未找到 roomTriggerZone。请在 Inspector 绑定 RoomTriggerZone 或给当前物体添加 Collider2D。", this);
        }

        if (roomGates == null || roomGates.Count == 0)
        {
            Debug.LogWarning("[RoomController] roomGates 为空，战斗开始时不会锁门。", this);
        }
        else
        {
            roomGates.RemoveAll(gate => gate == null);

            for (int i = roomGates.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (roomGates[i] == roomGates[j])
                    {
                        roomGates.RemoveAt(i);
                        break;
                    }
                }
            }

            for (int i = 0; i < roomGates.Count; i++)
            {
                if (roomGates[i].GetComponent<DoorController>() == null)
                {
                    Debug.LogWarning($"[RoomController] roomGates[{i}] ({roomGates[i].name}) 没有 DoorController，无法锁门。", roomGates[i]);
                }
            }

            DoorController[] childDoors = GetComponentsInChildren<DoorController>(true);
            for (int i = 0; i < childDoors.Length; i++)
            {
                GameObject childDoorObject = childDoors[i].gameObject;
                if (!roomGates.Contains(childDoorObject))
                {
                    Debug.LogWarning($"[RoomController] 检测到未纳入 roomGates 的门：{childDoorObject.name}。它不会被 TestRoom_01 的房间逻辑控制。", childDoorObject);
                }
            }
        }

        if (enemiesInRoom == null || enemiesInRoom.Count == 0)
        {
            Debug.LogWarning("[RoomController] enemiesInRoom 为空，房间会被立即判定为清空。", this);
        }
        else
        {
            enemiesInRoom.RemoveAll(enemy => enemy == null);
        }

        if (_treasureBox == null)
        {
            Debug.LogWarning("[RoomController] _treasureBox 未绑定，清怪后不会显示宝箱。", this);
        }
    }

    // 玩家进入房间触发区时开始战斗。
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (!isBattleStarted && !isRoomCleared)
        {
            StartRoomBattle();
        }
    }

    // 锁门、激活敌人并开启 canChase。
    private void StartRoomBattle()
    {
        isBattleStarted = true;

        foreach (GameObject gate in roomGates)
        {
            if (gate != null)
            {
                // Entering battle: make gates appear first, then lock.
                gate.SetActive(true);

                DoorController door = gate.GetComponent<DoorController>();
                if (door != null)
                {
                    door.SetLocked(true);
                }
                else
                {
                    gate.SetActive(true);
                }
            }
        }

        // Optional: wake all enemies and explicitly enable chase AI.
        foreach (GameObject enemy in enemiesInRoom)
        {
            if (enemy == null)
            {
                continue;
            }

            enemy.SetActive(true);

            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.canChase = true;
            }
            else
            {
                Debug.LogWarning($"[RoomController] 房间内的物体 {enemy.name} 没有挂载 EnemyAI 组件，已自动跳过。");
            }
        }

        nextCheckTime = Time.time + enemyCheckInterval;
    }

    // 轮询清怪状态；备用：玩家已在 bounds 内则开战。
    private void Update()
    {
        TryStartBattleByBoundsFallback();

        if (!isBattleStarted || isRoomCleared)
        {
            return;
        }

        if (Time.time < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.time + enemyCheckInterval;
        enemiesInRoom.RemoveAll(item => item == null);

        if (enemiesInRoom.Count == 0)
        {
            ClearRoom();
        }
    }

    // 清怪完成：解锁/隐藏门，显示宝箱。
    private void ClearRoom()
    {
        isRoomCleared = true;

        foreach (GameObject gate in roomGates)
        {
            if (gate != null)
            {
                DoorController door = gate.GetComponent<DoorController>();
                if (door != null)
                {
                    door.SetLocked(false);
                }

                // Keep old room-flow behavior: hide gates when room is cleared.
                // Entrance/Exit DungeonDoor 支持延迟隐藏，让清怪反馈更自然。
                if (IsDungeonDoorWithDelay(gate))
                {
                    StartCoroutine(FadeOutGateAfterDelay(gate, dungeonDoorHideDelay, dungeonDoorFadeDuration));
                }
                else
                {
                    gate.SetActive(false);
                }
            }
        }

        if (_treasureBox != null)
        {
            _treasureBox.SetActive(true);
        }

        Debug.Log("房间已清空，大门打开！");
    }

    private IEnumerator FadeOutGateAfterDelay(GameObject gate, float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        if (gate == null)
        {
            yield break;
        }

        SpriteRenderer[] renderers = gate.GetComponentsInChildren<SpriteRenderer>(true);
        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(1f, 0f, t);
            SetRenderersAlpha(renderers, alpha);
            yield return null;
        }

        SetRenderersAlpha(renderers, 0f);
        gate.SetActive(false);
        // 复位为不透明，避免未来再次激活时仍是 0 透明度。
        SetRenderersAlpha(renderers, 1f);
    }

    private static bool IsDungeonDoorWithDelay(GameObject gate)
    {
        if (gate == null)
        {
            return false;
        }

        string gateName = gate.name;
        return gateName.Contains("EntranceDungeonDoor") || gateName.Contains("ExitDungeonDoor");
    }

    private static void SetRenderersAlpha(SpriteRenderer[] renderers, float alpha)
    {
        if (renderers == null)
        {
            return;
        }

        float a = Mathf.Clamp01(alpha);
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer sr = renderers[i];
            if (sr == null)
            {
                continue;
            }

            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }

    // 查找 RoomTriggerZone 或自身 Collider2D。
    private void AutoBindTriggerZone()
    {
        if (roomTriggerZone != null)
        {
            return;
        }

        Transform triggerTransform = transform.Find("RoomTriggerZone");
        if (triggerTransform != null)
        {
            roomTriggerZone = triggerTransform.GetComponent<Collider2D>();
        }

        if (roomTriggerZone == null)
        {
            roomTriggerZone = GetComponent<Collider2D>();
        }
    }

    // 无 OnTrigger 时，玩家已在触发 bounds 内则开战。
    private void TryStartBattleByBoundsFallback()
    {
        if (isBattleStarted || isRoomCleared)
        {
            return;
        }

        AutoBindTriggerZone();
        if (roomTriggerZone == null)
        {
            return;
        }

        GameObject player = GameObject.FindWithTag(playerTag);
        if (player == null)
        {
            return;
        }

        if (roomTriggerZone.bounds.Contains(player.transform.position))
        {
            StartRoomBattle();
        }
    }
}
