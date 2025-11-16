using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("输入动作引用")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference dashAction;
    public InputActionReference attackAction;

    // 输入状态
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool IsDashPressed { get; private set; }
    public bool IsAttackPressed { get; private set; }

    private void OnEnable()
    {
        moveAction.action.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        moveAction.action.canceled += ctx => MoveInput = Vector2.zero;

        jumpAction.action.performed += ctx => { IsJumpPressed = true; IsJumpHeld = true; };
        jumpAction.action.canceled += ctx => { IsJumpPressed = false; IsJumpHeld = false; };

        dashAction.action.performed += ctx => IsDashPressed = true;
        dashAction.action.canceled += ctx => IsDashPressed = false;

        attackAction.action.performed += ctx => IsAttackPressed = true;
        attackAction.action.canceled += ctx => IsAttackPressed = false;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= ctx => MoveInput = ctx.ReadValue<Vector2>();
        moveAction.action.canceled -= ctx => MoveInput = Vector2.zero;

        jumpAction.action.performed -= ctx => { IsJumpPressed = true; IsJumpHeld = true; };
        jumpAction.action.canceled -= ctx => { IsJumpPressed = false; IsJumpHeld = false; };

        dashAction.action.performed -= ctx => IsDashPressed = true;
        dashAction.action.canceled -= ctx => IsDashPressed = false;

        attackAction.action.performed -= ctx => IsAttackPressed = true;
        attackAction.action.canceled -= ctx => IsAttackPressed = false;
    }

    public void ResetOneShotInputs()
    {
        IsJumpPressed = false;
        IsDashPressed = false;
        IsAttackPressed = false;
    }
}