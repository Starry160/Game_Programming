using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>玩家移动与瞄准：WASD/摇杆移动，鼠标/右摇杆瞄准。</summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Aim")]
    [SerializeField] private float _gamepadAimDeadZone = 0.15f;

    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private InputAction _moveAction;
    private InputAction _aimAction;

    private Vector2 _moveInput;
    private Vector2 _rawAimInput;
    private bool _aimControlIsPointer;

    /// <summary>
    /// 提供给外部系统读取的移动速度。
    /// </summary>
    public float MoveSpeed => _moveSpeed;

    /// <summary>
    /// 提供给外部系统读取的当前瞄准方向。
    /// </summary>
    public Vector2 AimDirection { get; private set; }

    // 初始化组件、刚体参数与输入 Action。
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        // 刚体参数强制由代码配置，避免场景手动设置不一致。
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 给 2D 俯视角像素角色一个通用默认碰撞体大小。
        _boxCollider2D.size = new Vector2(0.8f, 0.8f);

        CreateInputActions();
    }

    // 订阅并启用输入。
    private void OnEnable()
    {
        _moveAction.performed += OnMovePerformed;
        _moveAction.canceled += OnMoveCanceled;
        _aimAction.performed += OnAimPerformed;
        _aimAction.canceled += OnAimCanceled;

        _moveAction.Enable();
        _aimAction.Enable();
    }

    // 禁用输入并取消订阅。
    private void OnDisable()
    {
        _moveAction.Disable();
        _aimAction.Disable();

        _moveAction.performed -= OnMovePerformed;
        _moveAction.canceled -= OnMoveCanceled;
        _aimAction.performed -= OnAimPerformed;
        _aimAction.canceled -= OnAimCanceled;
    }

    // 释放 InputAction 资源。
    private void OnDestroy()
    {
        _moveAction?.Dispose();
        _aimAction?.Dispose();
    }

    // 物理帧：按输入设置刚体速度。
    private void FixedUpdate()
    {
        // 通过 Rigidbody2D.velocity 驱动物理移动，不直接操作 Transform。
        _rigidbody2D.velocity = _moveInput * _moveSpeed;
    }

    // 逻辑帧：更新瞄准方向与移动动画。
    private void Update()
    {
        UpdateAimDirection();
        // 角色左右朝向已交给 PlayerFacing 脚本基于鼠标位置处理，这里不再翻转。
        // UpdateFacingByAim();
        UpdateAnimation();
    }

    // 创建移动与瞄准的 Input System 绑定。
    private void CreateInputActions()
    {
        _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveAction.AddBinding("<Gamepad>/leftStick");

        // 鼠标位置与右摇杆共用一个 Action，按当前输入设备判断处理逻辑。
        _aimAction = new InputAction("Aim", InputActionType.PassThrough, expectedControlType: "Vector2");
        _aimAction.AddBinding("<Mouse>/position");
        _aimAction.AddBinding("<Gamepad>/rightStick");
    }

    // 移动输入开始/变化。
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // 移动输入松开。
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    // 瞄准输入更新，区分鼠标与手柄。
    private void OnAimPerformed(InputAction.CallbackContext context)
    {
        _rawAimInput = context.ReadValue<Vector2>();
        _aimControlIsPointer = context.control.device is Pointer;
    }

    // 瞄准输入取消。
    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        _rawAimInput = Vector2.zero;
    }

    // 将原始瞄准输入转为归一化世界方向。
    private void UpdateAimDirection()
    {
        if (_aimControlIsPointer)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                AimDirection = Vector2.zero;
                return;
            }

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(_rawAimInput);
            Vector2 aim = (Vector2)(mouseWorld - transform.position);
            AimDirection = aim.sqrMagnitude > 0.0001f ? aim.normalized : Vector2.zero;
            return;
        }

        if (_rawAimInput.sqrMagnitude < _gamepadAimDeadZone * _gamepadAimDeadZone)
        {
            AimDirection = Vector2.zero;
            return;
        }

        AimDirection = _rawAimInput.normalized;
    }

    // （已弃用）按移动方向翻转 Sprite，现由 PlayerFacing 处理。
    private void UpdateFacingByAim()
    {
        if (_spriteRenderer == null || Mathf.Approximately(_moveInput.x, 0f))
        {
            return;
        }

        _spriteRenderer.flipX = _moveInput.x < 0f;
    }

    // 根据是否在移动切换 Animator 的 isMoving。
    private void UpdateAnimation()
    {
        if (_animator == null)
        {
            return;
        }

        // 基于当前移动输入切换待机/移动动画状态。
        bool isMoving = _moveInput.sqrMagnitude > 0f;
        _animator.SetBool("isMoving", isMoving);
    }
}
