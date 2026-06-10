using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Starts room combat, locks doors, detects clears, and reveals rewards.</summary>
public class RoomController : MonoBehaviour
{
    public System.Action<RoomController> RoomCleared;
    public System.Action<RoomController> RoomBattleStarted;
    public bool IsRoomCleared => isRoomCleared;
    public bool IsBattleStarted => isBattleStarted;

    [Header("Room Data")]
    public List<GameObject> enemiesInRoom = new List<GameObject>();
    public List<GameObject> roomGates = new List<GameObject>();

    [Header("Check Settings")]
    [SerializeField] private float enemyCheckInterval = 0.2f;
    [SerializeField] private string playerTag = "Player";

    [Header("Treasure Reveal")]
    [Tooltip("Show the treasure chest after all room enemies are cleared.")]
    [SerializeField] private bool enableTreasureReveal = true;
    [SerializeField] private GameObject _treasureBox;

    [Header("Gate Hide Timing")]
    [Tooltip("Delay before hiding dungeon doors after the room is cleared.")]
    [SerializeField] private float dungeonDoorHideDelay = 0.8f;
    [Tooltip("Fade duration used when hiding dungeon doors after room clear.")]
    [SerializeField] private float dungeonDoorFadeDuration = 0.35f;

    [Header("Trigger Fallback")]
    [Tooltip("Trigger area used to detect that the player has entered this room.")]
    [SerializeField] private Collider2D roomTriggerZone;

    private bool isRoomCleared = false;
    private bool isBattleStarted = false;
    private float nextCheckTime = 0f;

    // Locates the room trigger so battle start checks work even if it was not assigned.
    private void Awake()
    {
        AutoBindTriggerZone();
    }

    // Keeps the reward chest hidden until this room has been cleared.
    private void Start()
    {
        if (enableTreasureReveal && _treasureBox != null)
        {
            _treasureBox.SetActive(false);
        }
    }

    [ContextMenu("Sync Room Gates From Children")]
    // Rebuilds the room gate list from child DoorController objects.
    private void SyncRoomGatesFromChildren()
    {
        DoorController[] childDoors = GetComponentsInChildren<DoorController>(true);
        roomGates.Clear();
        for (int i = 0; i < childDoors.Length; i++)
        {
            roomGates.Add(childDoors[i].gameObject);
        }

        Debug.Log($"[RoomController] Synchronized {roomGates.Count} doors into roomGates.", this);
    }

    // Keeps editor-time references and collider setup consistent.
    private void OnValidate()
    {
        AutoBindTriggerZone();

        // Editor validation catches missing room pieces before the scene is played.
        if (roomTriggerZone == null)
        {
            Debug.LogWarning("[RoomController] Required room reference is missing. Check the Inspector setup.", this);
        }

        if (roomGates == null || roomGates.Count == 0)
        {
            Debug.LogWarning("[RoomController] Required room reference is missing. Check the Inspector setup.", this);
        }
        else
        {
            roomGates.RemoveAll(gate => gate == null);

            // Remove duplicate gate references so a door is not locked or unlocked twice.
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
                    Debug.LogWarning($"[RoomController] roomGates[{i}] ({roomGates[i].name}) has no DoorController and cannot be locked.", roomGates[i]);
                }
            }

            DoorController[] childDoors = GetComponentsInChildren<DoorController>(true);
            for (int i = 0; i < childDoors.Length; i++)
            {
                GameObject childDoorObject = childDoors[i].gameObject;
                if (!roomGates.Contains(childDoorObject))
                {
                    Debug.LogWarning($"[RoomController] Door {childDoorObject.name} is not included in roomGates and will not be controlled by this room.", childDoorObject);
                }
            }
        }

        if (enemiesInRoom == null || enemiesInRoom.Count == 0)
        {
            Debug.LogWarning("[RoomController] Required room reference is missing. Check the Inspector setup.", this);
        }
        else
        {
            enemiesInRoom.RemoveAll(enemy => enemy == null);
        }

        if (enableTreasureReveal && _treasureBox == null)
        {
            Debug.LogWarning("[RoomController] Required room reference is missing. Check the Inspector setup.", this);
        }
    }

    // Starts the room battle when the player crosses the room trigger.
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

    // Locks gates, enables enemies, and activates any boss assigned to the room.
    private void StartRoomBattle()
    {
        isBattleStarted = true;
        RoomBattleStarted?.Invoke(this);

        // Battle start closes every configured gate so the player cannot leave mid-fight.
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

            // Normal enemies use EnemyAI, while the final boss has a separate activation path.
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.canChase = true;
            }

            FinalBossController finalBoss = enemy.GetComponent<FinalBossController>();
            if (finalBoss == null)
            {
                finalBoss = enemy.GetComponentInParent<FinalBossController>();
            }

            if (finalBoss != null)
            {
                finalBoss.ActivateBattle();
            }
        }

        nextCheckTime = Time.time + enemyCheckInterval;
    }

    // Periodically checks room enemies and clears the room once none remain.
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
        // Destroyed or inactive enemies are removed before deciding whether the room is clear.
        enemiesInRoom.RemoveAll(IsEnemyCleared);

        if (enemiesInRoom.Count == 0)
        {
            ClearRoom();
        }
    }

    // Unlocks gates, hides dungeon doors, and reveals the treasure chest after combat.
    private void ClearRoom()
    {
        isRoomCleared = true;

        // Clearing the room reverses the battle lockdown and then reveals the room reward.
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

        if (enableTreasureReveal && _treasureBox != null)
        {
            _treasureBox.SetActive(true);
        }

        RoomCleared?.Invoke(this);
        Debug.Log("Room cleared. Doors opened!");
    }

    // Waits briefly, fades a dungeon gate out, then disables it.
    private IEnumerator FadeOutGateAfterDelay(GameObject gate, float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        // If the door was destroyed during the delay, the fade can be skipped safely.
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
        SetRenderersAlpha(renderers, 1f);
    }

    // Returns whether this gate should use delayed fade-out behavior.
    private static bool IsDungeonDoorWithDelay(GameObject gate)
    {
        if (gate == null)
        {
            return false;
        }

        string gateName = gate.name;
        return gateName.Contains("EntranceDungeonDoor") || gateName.Contains("ExitDungeonDoor");
    }

    // Applies a shared alpha value to all renderers on a gate.
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

    // Returns whether an enemy entry should be removed from the active room list.
    private static bool IsEnemyCleared(GameObject enemyObject)
    {
        if (enemyObject == null)
        {
            return true;
        }

        FinalBossController finalBoss = enemyObject.GetComponent<FinalBossController>();
        if (finalBoss == null)
        {
            finalBoss = enemyObject.GetComponentInParent<FinalBossController>();
        }

        if (finalBoss != null)
        {
            return finalBoss.IsDefeated;
        }

        return false;
    }

    // Finds the room trigger collider when it was not assigned manually.
    private void AutoBindTriggerZone()
    {
        if (roomTriggerZone != null)
        {
            return;
        }

        // Prefer a named child trigger, then fall back to this object's Collider2D.
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

    // Starts combat if the player is already inside the room trigger bounds.
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

        // Bounds fallback covers cases where trigger callbacks were missed during scene setup.
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
