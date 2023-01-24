using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerInput : MonoBehaviour
{

    float playerSpeed = 6f;
    float playerGravity = -10f;
    Vector2 velocity;

    CharacterController2D controller;

    void Start()
    {
        controller = GetComponent<CharacterController2D>();
    }

    void Update()
    {
        Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        velocity.x = rawInput.x * playerSpeed;
        velocity.y += playerGravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
	}
}
