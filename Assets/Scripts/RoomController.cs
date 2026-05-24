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
                gate.SetActive(true);

                DoorController door = gate.GetComponent<DoorController>();
                if (door != null)
                {
                    door.SetLocked(true);
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
                gate.SetActive(false);

                DoorController door = gate.GetComponent<DoorController>();
                if (door != null)
                {
                    door.SetLocked(false);
                }
            }
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
