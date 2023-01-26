using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerInput : MonoBehaviour
{

    Vector2 rawInput;
    CharacterController2D controller;

    void Start()
    {
        controller = GetComponent<CharacterController2D>();
    }

    void FixedUpdate()
    {
        controller.Move(rawInput);
	}

    public void OnMovement(InputAction.CallbackContext context) {
        rawInput = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
	}

    public void OnRun(InputAction.CallbackContext context) {
        controller.Running(context.performed);
    }

    public void OnJump(InputAction.CallbackContext context) {
        if(context.performed) {
            controller.Jump();
        }

        if(context.canceled) {
            controller.JumpRelease();
        }
    }
}
