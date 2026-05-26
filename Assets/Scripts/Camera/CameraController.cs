using UnityEngine;

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

    private Transform _target;
    private float _lockedZ;
    private Vector3 _currentVelocity;

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
            _target = player.transform;
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
}
