using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public void OnMovement(InputValue input) {
        rawInput = new Vector2(input.Get<Vector2>().x, input.Get<Vector2>().y);
	}

    public void OnJump(InputValue input) {
        controller.Jump();
    }
}
