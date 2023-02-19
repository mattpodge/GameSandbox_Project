using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine.Windows;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerController : MonoBehaviour
{

	#region
	[Header("Forces")]
	float gravity;
	#endregion

	#region
	[Header("Movement")]
	[SerializeField]
	float walkSpeed = 8f;
	[SerializeField]
	float runSpeed = 16f;

	Vector2 velocity;
	float velocitySmoothing;

	[SerializeField]
	float accTimeGrounded = 0.1f;
	[SerializeField]
	float accTimeAirborne = 0.2f;
	#endregion

	#region
	[Header("Jumping")]
	[SerializeField]
	float maxJumpHeight = 3.5f;
	[SerializeField]
	float minJumpHeight = 1.25f;
	[SerializeField]
	float timeToJumpApex = 0.4f;
	[SerializeField]
	float fallMultiplier = 2f;

	float maxJumpVel;
	float minJumpVel;
	#endregion

	#region
	// Coyote Time
	[SerializeField]
	float coyoteTime = 0.1f;
	float coyoteTimeCounter;

	// Jump Buffer
	[SerializeField]
	float jumpBuffer = 0.05f;
	float jumpBufferCounter;
	#endregion

	#region
	[Header("Wall Jump")]
	[SerializeField]
	bool wallJumpEnabled = true;

	[SerializeField]
	float wallStickTime = 0.25f;
	float wallStickTimeCounter;

	bool wallSliding;
	int wallDirection;
	[SerializeField]
	float maxWallSlideVelocity = 3f;
	[SerializeField]
	Vector2 wallJumpClimb;
	[SerializeField]
	Vector2 wallJumpOff;
	[SerializeField]
	Vector2 wallJumpLeap;
	#endregion


	// Movement input
	Vector2 moveInput;

	// Ref character controller
    CharacterController2D controller;

    void Start()
    {
        controller = GetComponent<CharacterController2D>();

		// Calculate forces
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVel = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVel = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	void Update() {
		CalcVelocity();

		if(wallJumpEnabled) {
			WallSliding();
		}

		// Coyote time behaviour
		if(controller.collisions.below) {
			coyoteTimeCounter = coyoteTime;
		} else {
			coyoteTimeCounter -= Time.deltaTime;
		}

		// Jump buffer behaviour
		if(jumpBufferCounter > 0f) {
			jumpBufferCounter -= Time.deltaTime;
		}
		if(coyoteTimeCounter > 0f && jumpBufferCounter > 0f) {
			jumpBufferCounter = 0f;
			velocity.y = maxJumpVel;
		}

		// Move the player
		controller.Move(velocity * Time.deltaTime);

		if(controller.collisions.above || controller.collisions.below) {
			velocity.y = 0f;
		}
	}

	public void OnMovement(InputAction.CallbackContext context) {
        moveInput = new Vector2(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
	}

	public void OnRun(InputAction.CallbackContext context) {
		controller.state.isRunning = context.performed;
    }

    public void OnJump(InputAction.CallbackContext context) {
        if(context.performed) {

			if(wallSliding) {
				if(wallDirection == moveInput.x) {
					velocity.x = -wallDirection * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				} else if(moveInput.x == 0) {
					velocity.x = -wallDirection * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				} else {
					velocity.x = -wallDirection * wallJumpLeap.x;
					velocity.y = wallJumpLeap.y;
				}
			}

			if(coyoteTimeCounter > 0f) {
				velocity.y = maxJumpVel;
			}
			jumpBufferCounter = jumpBuffer;
		}
		if(context.canceled) {
			coyoteTimeCounter = 0f;
			if(velocity.y > minJumpVel) {
				velocity.y = minJumpVel;
			}
		}
	}

	void WallSliding() {
		wallDirection = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;
			if(velocity.y < -maxWallSlideVelocity) {
				velocity.y = -maxWallSlideVelocity;
			}

			// Give player time to leap when pushing away from the wall
			if(wallStickTimeCounter > 0) {
				velocitySmoothing = 0;
				velocity.x = 0;

				if(moveInput.x != wallDirection && moveInput.x != 0) {
					wallStickTimeCounter -= Time.deltaTime;
				} else {
					wallStickTimeCounter = wallStickTime;
				}
			} else {
				wallStickTimeCounter = wallStickTime;
			}
		}
	}

	void CalcVelocity() {
		float targetVelocity = moveInput.x * (controller.state.isRunning ? runSpeed : walkSpeed);

		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocitySmoothing, (controller.state.isGrounded ? accTimeGrounded : accTimeAirborne));
		velocity.y += gravity * (velocity.y <= -0.5f ? fallMultiplier : 1f) * Time.deltaTime;
	}


}
