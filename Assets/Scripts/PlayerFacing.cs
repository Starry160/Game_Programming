using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFacing : MonoBehaviour
{
    private void Update()
    {
        // 攻击意图：鼠标左键被按下 / 按住。
        bool isAttacking = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (isAttacking)
        {
            FaceByMouse();
            return;
        }

        FaceByMovement();
    }

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

        // 2D 正交相机下，ScreenToWorldPoint 必须携带相对相机的 Z 深度；
        // 否则 Z=0 时结果会坍缩到相机自身位置，导致相机跟随期间左右判定翻转。
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseScreenWithDepth = new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            -mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenWithDepth);

        // 战斗状态：鼠标左右决定角色朝向，支持“边退边打”。
        if (mouseWorldPos.x > transform.position.x)
        {
            SetFacing(1f);
        }
        else if (mouseWorldPos.x < transform.position.x)
        {
            SetFacing(-1f);
        }
    }

    private void FaceByMovement()
    {
        // 跑图状态：直接读键盘 A/D 键的水平输入；未按键则保持当前朝向。
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

        // 兼容手柄：左摇杆 X 也参与水平朝向判定，带小死区防抖。
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

    private void SetFacing(float signX)
    {
        Vector3 scale = transform.localScale;
        scale.x = signX * Mathf.Abs(scale.x == 0f ? 1f : scale.x);
        transform.localScale = scale;
    }
}
