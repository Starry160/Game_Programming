using System.Collections.Generic;
using UnityEngine;

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

    [Header("Trigger Fallback")]
    [Tooltip("优先使用这个触发区做玩家进入检测（推荐绑定 RoomTriggerZone）。")]
    [SerializeField] private Collider2D roomTriggerZone;

    private bool isRoomCleared = false;
    private bool isBattleStarted = false;
    private float nextCheckTime = 0f;

    private void Awake()
    {
        AutoBindTriggerZone();
    }

    private void Start()
    {
        if (_treasureBox != null)
        {
            _treasureBox.SetActive(false);
        }
    }

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
                gate.SetActive(false);
            }
        }

        if (_treasureBox != null)
        {
            _treasureBox.SetActive(true);
        }

        Debug.Log("房间已清空，大门打开！");
    }

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
