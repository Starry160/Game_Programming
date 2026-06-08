using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>2D 相机平滑跟随玩家，可外部切换目标。</summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Follow")]
    [Tooltip("平滑跟随时间，数值越小跟随越紧，越大缓冲越柔。")]
    [SerializeField] private float _smoothTime = 0.15f;

    [Tooltip("相机最大跟随速度，0 或负值表示不限制。")]
    [SerializeField] private float _maxSpeed = -1f;

    [Tooltip("相机与目标的本地偏移（XY 平面）。")]
    [SerializeField] private Vector2 _offset = Vector2.zero;

    [Header("Intro Cinematic")]
    [Tooltip("进入场景时播放 Boss 开场镜头。")]
    [SerializeField] private bool _enableIntroCinematic = true;
    [Tooltip("仅在该场景名生效，避免影响其它关卡。")]
    [SerializeField] private string _introSceneName = "Final Boss";
    [Tooltip("Boss 对象名。")]
    [SerializeField] private string _bossObjectName = "FinalBoss";
    [SerializeField] private float _holdOnPlayerDuration = 0.8f;
    [SerializeField] private float _moveToBossDuration = 1.2f;
    [SerializeField] private float _holdOnBossDuration = 1.5f;
    [SerializeField] private float _moveBackToPlayerDuration = 1.0f;
    [SerializeField] private float _cinematicSmoothTime = 0.32f;
    [Tooltip("开场期间临时禁用玩家移动/攻击。")]
    [SerializeField] private bool _disablePlayerControlDuringIntro = true;

    private Transform _target;
    private float _lockedZ;
    private Vector3 _currentVelocity;
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    private PlayerFacing _playerFacing;

    // 缓存初始 Z，防止 2D 正交相机深度漂移。
    private void Awake()
    {
        // 锁定初始 Z 轴深度，避免跟随导致 2D 画面丢失。
        _lockedZ = transform.position.z;
    }

    // 查找场景中的玩家作为跟随目标。
    private void Start()
    {
        // 纯代码定位跟随目标，无需 Inspector 手动拖拽。
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            _playerController = player;
            _playerAttack = player.GetComponent<PlayerAttack>();
            _playerFacing = player.GetComponent<PlayerFacing>();
            _target = player.transform;
            TryPlayIntroCinematic(player.transform);
        }
        else
        {
            Debug.LogWarning("[CameraController] 未在场景中找到 PlayerController，相机将保持静止。");
        }
    }

    // 每帧平滑插值到目标位置（LateUpdate 减少抖动）。
    private void LateUpdate()
    {
        // 在 LateUpdate 中处理跟随，避免与玩家的 Update/FixedUpdate 移动产生抖动。
        if (_target == null)
        {
            return;
        }

        Vector3 desired = new Vector3(
            _target.position.x + _offset.x,
            _target.position.y + _offset.y,
            _lockedZ);

        float maxSpeed = _maxSpeed > 0f ? _maxSpeed : Mathf.Infinity;
        Vector3 smoothed = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref _currentVelocity,
            _smoothTime,
            maxSpeed);

        // 再次强制锁定 Z，双保险避免误修改。
        smoothed.z = _lockedZ;
        transform.position = smoothed;
    }

    /// <summary>
    /// 允许外部系统（例如关卡切换、Boss 剧情）手动指定跟随目标。
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    private void TryPlayIntroCinematic(Transform playerTransform)
    {
        if (!_enableIntroCinematic)
        {
            return;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!string.Equals(activeSceneName, _introSceneName, System.StringComparison.Ordinal))
        {
            return;
        }

        GameObject bossObject = GameObject.Find(_bossObjectName);
        if (bossObject == null)
        {
            return;
        }

        StartCoroutine(IntroCinematicRoutine(playerTransform, bossObject.transform));
    }

    private IEnumerator IntroCinematicRoutine(Transform playerTransform, Transform bossTransform)
    {
        if (_disablePlayerControlDuringIntro)
        {
            SetPlayerControlEnabled(false);
        }

        float originalSmoothTime = _smoothTime;
        _smoothTime = Mathf.Max(0.01f, _cinematicSmoothTime);

        // Hold on player first to build tension before camera move.
        yield return new WaitForSeconds(Mathf.Max(0f, _holdOnPlayerDuration));

        SetTarget(bossTransform);
        yield return new WaitForSeconds(Mathf.Max(0.05f, _moveToBossDuration));
        yield return new WaitForSeconds(Mathf.Max(0f, _holdOnBossDuration));

        SetTarget(playerTransform);
        yield return new WaitForSeconds(Mathf.Max(0.05f, _moveBackToPlayerDuration));

        _smoothTime = originalSmoothTime;
        if (_disablePlayerControlDuringIntro)
        {
            SetPlayerControlEnabled(true);
        }
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (_playerController != null)
        {
            _playerController.enabled = enabled;
            Rigidbody2D rb = _playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }

        if (_playerAttack != null)
        {
            _playerAttack.enabled = enabled;
        }

        if (_playerFacing != null)
        {
            _playerFacing.enabled = enabled;
        }
    }
}
