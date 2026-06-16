using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the player's left-right facing direction. During attacks it faces the mouse target,
/// and during movement it faces the horizontal movement direction.
/// </summary>
public class PlayerFacing : MonoBehaviour
{
    // Updates facing from attack input first, then movement input when not attacking.
    private void Update()
    {
        bool isAttacking = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (isAttacking)
        {
            FaceByMouse();
            return;
        }

        FaceByMovement();
    }

    // Uses the mouse world position to flip the player during attacks.
    private void FaceByMouse()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseScreenWithDepth = new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            -mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenWithDepth);

        if (mouseWorldPos.x > transform.position.x)
        {
            SetFacing(1f);
        }
        else if (mouseWorldPos.x < transform.position.x)
        {
            SetFacing(-1f);
        }
    }

    // Uses keyboard or gamepad horizontal movement to flip the player while exploring.
    private void FaceByMovement()
    {
        float horizontal = ReadHorizontalInput();

        if (horizontal > 0f)
        {
            SetFacing(1f);
        }
        else if (horizontal < 0f)
        {
            SetFacing(-1f);
        }
    }

    // Combines keyboard and gamepad horizontal input with a small dead zone.
    private static float ReadHorizontalInput()
    {
        float horizontal = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed)
            {
                horizontal += 1f;
            }

            if (Keyboard.current.aKey.isPressed)
            {
                horizontal -= 1f;
            }
        }

        if (Mathf.Approximately(horizontal, 0f) && Gamepad.current != null)
        {
            float stickX = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.2f)
            {
                horizontal = stickX;
            }
        }

        return horizontal;
    }

    // Applies the facing direction by changing the sign of localScale.x.
    private void SetFacing(float signX)
    {
        Vector3 scale = transform.localScale;
        scale.x = signX * Mathf.Abs(scale.x == 0f ? 1f : scale.x);
        transform.localScale = scale;
    }
}
