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

    public void OnMovement(InputAction.CallbackContext input) {
        rawInput = new Vector2(input.ReadValue<Vector2>().x, input.ReadValue<Vector2>().y);
	}

    public void OnJump(InputAction.CallbackContext input) {
        if(input.performed) {
            controller.Jump();
        }

        if(input.canceled) {
            controller.ShortJump();
        }
    }
}
