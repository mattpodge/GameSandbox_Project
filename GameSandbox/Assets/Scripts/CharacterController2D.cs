using System.Collections;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{

	Vector2 playerInput;

	// Collisions
	const float skinWidth = 0.015f;
	const float raySpacing = 0.25f;

	int horzRayCount;
	int vertRayCount;
	float horzRaySpacing;
	float vertRaySpacing;

	// Forces
	float gravity;

	// Movement
    float walkSpeed = 4f;
    float runSpeed = 6f;

    Vector2 velocity;
    float velocitySmoothing;

    float accTimeGrounded = 0.2f;
    float accTimeAirborne = 0.4f;

    // Jumping
    float maxJumpHeight = 2f;
    float minJumpHeight = 1f;
    float timeToJumpApex = 0.3f;

	float maxJumpVel;
	float minJumpVel;

    public LayerMask collisionMask;

    new BoxCollider2D collider;
    RayCastOrigins raycastOrigins;

	void Start() {
        collider = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();

		// Calculate forces
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVel = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVel = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	private void FixedUpdate() {
	}

	public void Move(Vector2 playerInput) {
		Vector2 movement = CalcVelocity(playerInput) * Time.deltaTime;
		UpdateRaycastOrigins();

		if(movement.x != 0) {
            HorzCollisions(ref movement);
        }

        if(movement.y != 0) {
            VertCollisions(ref movement);
		}

		transform.Translate(movement);
    }

    public void Jump() {
		velocity.y = maxJumpVel;
	}

	public void ShortJump() {
		Debug.Log("Short Jump");
	}

    Vector2 CalcVelocity(Vector2 playerInput) {
		float targetVelocity = playerInput.x * walkSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocitySmoothing, accTimeGrounded);
		velocity.y += gravity * Time.deltaTime;

        return new Vector2(velocity.x, velocity.y);
	}

	// Collision detection
	void HorzCollisions(ref Vector2 movement) {
        float dirX = Mathf.Sign(movement.x);
        float rayLength = Mathf.Abs(movement.x) + skinWidth;

		for(int i = 0; i < horzRayCount; i++) {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.btmLeft : raycastOrigins.btmRight;
            rayOrigin += Vector2.up * (horzRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin , Vector2.right * dirX, Color.yellow);

            if(hit) {
                movement.x = (hit.distance - skinWidth) * dirX;
                rayLength = hit.distance;
            }
		}

	}

    void VertCollisions(ref Vector2 movement) {
        float dirY = Mathf.Sign(movement.y);
        float rayLength = Mathf.Abs(movement.y) + skinWidth;

		for(int i = 0; i < vertRayCount; i++) {
            Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.btmLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (vertRaySpacing * i + movement.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin , Vector2.up * dirY, Color.red);

			if(hit) {
				movement.y = (hit.distance - skinWidth) * dirY;
				rayLength = hit.distance;
			}

		}
	}

	// Generate raycasts for collisions
	void UpdateRaycastOrigins() {
		Bounds bounds = collider.bounds;
		bounds.Expand(skinWidth * -2);
        raycastOrigins.btmLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.btmRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

	void CalculateRaySpacing() {
		Bounds bounds = collider.bounds;
		bounds.Expand(skinWidth * -2);
		horzRayCount = Mathf.RoundToInt(bounds.size.y / raySpacing);
		vertRayCount = Mathf.RoundToInt(bounds.size.x / raySpacing);

		horzRaySpacing = bounds.size.y / (horzRayCount - 1);
		vertRaySpacing = bounds.size.x / (vertRayCount - 1);
	}

	struct RayCastOrigins {
        public Vector2 btmLeft, btmRight;
        public Vector2 topLeft, topRight;
    }
}
