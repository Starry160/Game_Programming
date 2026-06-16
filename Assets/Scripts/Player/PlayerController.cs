using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement and aiming input. It builds runtime input actions, moves the Rigidbody2D
/// during physics updates, tracks mouse or gamepad aim direction, and updates movement animation.
/// </summary>
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
    /// Current movement speed used by the Rigidbody2D controller.
    /// </summary>
    public float MoveSpeed => _moveSpeed;

    /// <summary>
    /// Normalized aim direction used by weapons and any aim-aware player logic.
    /// </summary>
    public Vector2 AimDirection { get; private set; }

    // Configures player physics and builds runtime input actions.
    private void Awake()
    {
        // Core movement uses Rigidbody2D physics, so lock rotation and disable gravity.
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        _boxCollider2D.size = new Vector2(0.8f, 0.8f);

        CreateInputActions();
    }

    // Enables movement and aim input callbacks while the player can act.
    private void OnEnable()
    {
        _moveAction.performed += OnMovePerformed;
        _moveAction.canceled += OnMoveCanceled;
        _aimAction.performed += OnAimPerformed;
        _aimAction.canceled += OnAimCanceled;

        _moveAction.Enable();
        _aimAction.Enable();
    }

    // Disables movement and aim input callbacks when player control is turned off.
    private void OnDisable()
    {
        _moveAction.Disable();
        _aimAction.Disable();

        _moveAction.performed -= OnMovePerformed;
        _moveAction.canceled -= OnMoveCanceled;
        _aimAction.performed -= OnAimPerformed;
        _aimAction.canceled -= OnAimCanceled;
    }

    // Disposes runtime-created input actions when the player object is destroyed.
    private void OnDestroy()
    {
        _moveAction?.Dispose();
        _aimAction?.Dispose();
    }

    // Moves the Rigidbody2D from the current input vector during the physics step.
    private void FixedUpdate()
    {
        _rigidbody2D.velocity = _moveInput * _moveSpeed;
    }

    // Converts aim input and updates movement animation every frame.
    private void Update()
    {
        UpdateAimDirection();
        // UpdateFacingByAim();
        UpdateAnimation();
    }

    // Creates movement and aim input actions for keyboard, mouse, and gamepad.
    private void CreateInputActions()
    {
        // WASD and left stick share the same move action for consistent control mapping.
        _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveAction.AddBinding("<Gamepad>/leftStick");

        _aimAction = new InputAction("Aim", InputActionType.PassThrough, expectedControlType: "Vector2");
        _aimAction.AddBinding("<Mouse>/position");
        _aimAction.AddBinding("<Gamepad>/rightStick");
    }

    // Stores the latest movement input vector.
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // Clears movement input when the control is released.
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    // Stores the latest aim input and updates aim direction.
    private void OnAimPerformed(InputAction.CallbackContext context)
    {
        _rawAimInput = context.ReadValue<Vector2>();
        _aimControlIsPointer = context.control.device is Pointer;
    }

    // Clears aim input when the control is released.
    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        _rawAimInput = Vector2.zero;
    }

    // Converts mouse or stick input into a normalized world-space aim direction.
    private void UpdateAimDirection()
    {
        if (_aimControlIsPointer)
        {
            // Mouse aim must be converted from screen space into the player's world plane.
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                AimDirection = Vector2.zero;
                return;
            }

            if (!IsFiniteVector(_rawAimInput))
            {
                AimDirection = Vector2.zero;
                return;
            }

            Vector3 screenPos = new Vector3(
                _rawAimInput.x,
                _rawAimInput.y,
                Mathf.Abs(mainCamera.transform.position.z - transform.position.z));
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(screenPos);
            Vector2 aim = (Vector2)(mouseWorld - transform.position);
            AimDirection = aim.sqrMagnitude > 0.0001f ? aim.normalized : Vector2.zero;
            return;
        }

        // Gamepad aim is already a direction vector, so only apply a dead zone.
        if (_rawAimInput.sqrMagnitude < _gamepadAimDeadZone * _gamepadAimDeadZone)
        {
            AimDirection = Vector2.zero;
            return;
        }

        AimDirection = _rawAimInput.normalized;
    }

    // Returns whether a vector contains only finite numeric values.
    private static bool IsFiniteVector(Vector2 value)
    {
        return !float.IsNaN(value.x) && !float.IsNaN(value.y) &&
               !float.IsInfinity(value.x) && !float.IsInfinity(value.y);
    }

    // Keeps aim-facing updates centralized in PlayerFacing.
    private void UpdateFacingByAim()
    {
        if (_spriteRenderer == null || Mathf.Approximately(_moveInput.x, 0f))
        {
            return;
        }

        _spriteRenderer.flipX = _moveInput.x < 0f;
    }

    // Sets the movement animation flag from current input.
    private void UpdateAnimation()
    {
        if (_animator == null)
        {
            return;
        }

        bool isMoving = _moveInput.sqrMagnitude > 0f;
        _animator.SetBool("isMoving", isMoving);
    }
}
