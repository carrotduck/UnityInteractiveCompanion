using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class UserTargetMovement : MonoBehaviour
{
    public float moveSpeed = 2f;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
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

        // 通过角色控制器移动，让墙壁可以阻挡小球。
        controller.Move(
            direction.normalized * moveSpeed * Time.deltaTime);
    }
}