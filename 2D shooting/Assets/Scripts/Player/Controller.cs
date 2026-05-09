using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class controls player movement
/// </summary>
public class Controller : MonoBehaviour
{
    [Header("GameObject/Component References")]
    [Tooltip("The Rigidbody2D component to use in \"Astroids Mode\".")]
    public Rigidbody2D myRigidbody = null;

    [Header("Movement Variables")]
    [Tooltip("The speed at which the player will move.")]
    public float moveSpeed = 10.0f;
    [Tooltip("The speed at which the player rotates in asteroids movement mode")]
    public float rotationSpeed = 60f;

    [Header("Input Actions & Controls")]
    [Tooltip("The input action(s) that map to player movement")]
    public InputAction moveAction;
    [Tooltip("The input action(s) that map to where the controller looks")]
    public InputAction lookAction;

    /// <summary>
    /// Enum which stores different aiming modes
    /// </summary>
    public enum AimModes { AimTowardsMouse, AimForwards };

    [Tooltip("The aim mode in use by this player:\n" +
        "Aim Towards Mouse: Player rotates to face the mouse\n" +
        "Aim Forwards: Player aims the direction they face (doesn't face towards the mouse)")]
    public AimModes aimMode = AimModes.AimTowardsMouse;

    /// <summary>
    /// Enum to handle different movement modes for the player
    /// </summary>
    public enum MovementModes { MoveHorizontally, MoveVertically, FreeRoam, Astroids };

    [Tooltip("The movmeent mode used by this controller:\n" +
        "Move Horizontally: Player can only move left/right\n" +
        "Move Vertically: Player can only move up/down\n" +
        "FreeRoam: Player can move in any direction and can aim\n" +
        "Astroids: Player moves forward/back in the direction they are facing and rotates with horizontal input")]
    public MovementModes movementMode = MovementModes.FreeRoam;


    // Whether the player can aim with the mouse or not
    private bool canAimWithMouse
    {
        get
        {
            return aimMode == AimModes.AimTowardsMouse;
        }
    }

    // Whether the player's X coordinate is locked (Also assign in rigidbody)
    private bool lockXCoordinate
    {
        get
        {
            return movementMode == MovementModes.MoveVertically;
        }
    }
    // Whether the player's Y coordinate is locked (Also assign in rigidbody)
    public bool lockYCoordinate
    {
        get
        {
            return movementMode == MovementModes.MoveHorizontally;
        }
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once when the script starts before Update
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void Start()
    {
        if (moveAction.bindings.Count == 0 || lookAction.bindings.Count == 0)
        {
            Debug.LogWarning("An Input Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once per frame
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    void Update()
    {
        // Collect input and move the player accordingly
        HandleInput();
    }

    /// <summary>
    /// Description:
    /// Handles input and moves the player accordingly
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void HandleInput()
    {
        // Find the position that the player should look at
        Vector2 lookPosition = GetLookPosition();
        // Get movement input from the inputManager
        if (moveAction.bindings.Count == 0)
        {
            Debug.LogError("The Move Input Action does not have a binding set! It must have a binding set in order for movement to happen!");
        }
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float horizontalMovement = moveInput.x;
        float verticalMovement = moveInput.y;
        Vector3 movementVector = new Vector3(horizontalMovement, verticalMovement, 0);
        // Move the player
        MovePlayer(movementVector);
        LookAtPoint(lookPosition);
    }

    /// <summary>
    /// Description:
    /// Updates the position the player is looking at
    /// Inputs: 
    /// none
    /// Returns: 
    /// Vector2
    /// </summary>
    /// <returns>Vector2: The position the player should look at</returns>
    public Vector2 GetLookPosition()
    {
        Vector2 result = transform.up;
        if (aimMode != AimModes.AimForwards)
        {
            if (lookAction.bindings.Count == 0)
            {
                Debug.LogError("The Look Input Action does not have a binding set! It must have a binding set in order for the player to look around!");
            }
            result = lookAction.ReadValue<Vector2>();
        }
        else
        {
            result = transform.up;
        }
        return result;
    }

    /// <summary>
    /// Description:
    /// Moves the player
    /// Inputs: 
    /// Vector3 movement
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="movement">The direction to move the player</param>
    private void MovePlayer(Vector3 movement)
    {
        // Set the player's posiiton accordingly

        // Move according to astroids setting
        if (movementMode == MovementModes.Astroids)
        {

            // If no rigidbody is assigned, assign one
            if (myRigidbody == null)
            {
                myRigidbody = GetComponent<Rigidbody2D>();
            }

            // Move the player using physics
            Vector2 force = transform.up * movement.y * Time.deltaTime * moveSpeed;
            Debug.Log(force);
            myRigidbody.AddForce(force);

            // Rotate the player around the z axis
            Vector3 newRotationEulars = transform.rotation.eulerAngles;
            float zAxisRotation = transform.rotation.eulerAngles.z;
            float newZAxisRotation = zAxisRotation - rotationSpeed * movement.x * Time.deltaTime;
            newRotationEulars = new Vector3(newRotationEulars.x, newRotationEulars.y, newZAxisRotation);
            transform.rotation = Quaternion.Euler(newRotationEulars);

        }
        // Move according to the other settings
        else
        {
            // Don't move in the x if the settings stop us from doing so
            if (lockXCoordinate)
            {
                movement.x = 0;
            }
            // Don't move in the y if the settings stop us from doing so
            if (lockYCoordinate)
            {
                movement.y = 0;
            }
            // Move the player's transform
            transform.position = transform.position + (movement * Time.deltaTime * moveSpeed);
        }
    }

    /// <summary>
    /// Description: 
    /// Rotates the player to look at a point
    /// Inputs: 
    /// Vector3 point
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="point">The screen space position to look at</param>
    private void LookAtPoint(Vector3 point)
    {
        if (Time.timeScale > 0)
        {
            // Rotate the player to look at the mouse.
            Vector2 lookDirection = Camera.main.ScreenToWorldPoint(point) - transform.position;

            if (canAimWithMouse)
            {
                transform.up = lookDirection;
            }
            else
            {
                if (myRigidbody != null)
                {
                    myRigidbody.freezeRotation = true;
                }
            }
        }
    }
}
