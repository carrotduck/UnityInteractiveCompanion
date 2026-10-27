using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class UserTargetMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        Vector3 direction = Vector3.zero;

        if (keyboard.wKey.isPressed)
            direction.z += 1f;

        if (keyboard.sKey.isPressed)
            direction.z -= 1f;

        if (keyboard.aKey.isPressed)
            direction.x -= 1f;

        if (keyboard.dKey.isPressed)
            direction.x += 1f;

        // 归一化输入以保持斜向速度一致，碰撞阻挡交给角色控制器。
        controller.Move(
            direction.normalized * moveSpeed * Time.deltaTime);
    }
}
