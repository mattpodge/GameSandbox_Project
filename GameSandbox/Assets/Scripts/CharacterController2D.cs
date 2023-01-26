using System.Collections;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{

	#region
	[Header("Collision Detection")]
	const float skinWidth = 0.015f;
	const float raySpacing = 0.25f;

	int horzRayCount;
	int vertRayCount;
	float horzRaySpacing;
	float vertRaySpacing;

	public LayerMask collisionMask;
	#endregion

	#region
	[Header("Player Input")]
	Vector2 playerInput;
	bool playerRunning;
	#endregion

	#region
	[Header("Forces")]
	float gravity;
	#endregion

	#region
	[Header("Movement")]
	[SerializeField]
	float walkSpeed = 4f;
	[SerializeField]
	float runSpeed = 6f;

    Vector2 velocity;
    float velocitySmoothing;

	[SerializeField]
	float accTimeGrounded = 0.2f;
	[SerializeField]
	float accTimeAirborne = 0.4f;
	#endregion

	#region
	[Header("Jumping")]
	[SerializeField]
	float maxJumpHeight = 2f;
	[SerializeField]
	float minJumpHeight = 1f;
	[SerializeField]
	float timeToJumpApex = 0.3f;

	float maxJumpVel;
	float minJumpVel;


	// Coyote Time
	[SerializeField]
	float coyoteTime = 0.2f;
	float coyoteTimeCounter;

	// Jump Buffer
	[SerializeField]
	float jumpBuffer = 0.2f;
	float jumpBufferCounter;
	#endregion


    new BoxCollider2D collider;
    RayCastOrigins raycastOrigins;
	CollisionInfo collisions;

	void Start() {
        collider = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();

		// Calculate forces
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVel = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVel = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	private void FixedUpdate() {
		Debug.Log(collisions.below);

		if(collisions.below) {
			coyoteTimeCounter = coyoteTime;
		} else {
			coyoteTimeCounter -= Time.deltaTime;
		}

		if(jumpBufferCounter > 0f) {
			jumpBufferCounter -= Time.deltaTime;
		}

		if(coyoteTimeCounter > 0f && jumpBufferCounter > 0f) {
			Jump();
			jumpBufferCounter = 0f;
		}
	}

	public void Move(Vector2 playerInput) {
		Vector2 movement = CalcVelocity(playerInput) * Time.deltaTime;
		UpdateRaycastOrigins();
		collisions.Reset();

		if(movement.x != 0) {
            HorzCollisions(ref movement);
        }

        if(movement.y != 0) {
            VertCollisions(ref movement);
		}

		transform.Translate(movement);
    }

	public void Running(bool running) {
		playerRunning = running;
	}

    public void Jump() {
		if(coyoteTimeCounter > 0f) {
			velocity.y = maxJumpVel;
		}
		jumpBufferCounter = jumpBuffer;
	}

	public void JumpRelease() {
		if(velocity.y > minJumpVel) {
			velocity.y = minJumpVel;
		}
		coyoteTimeCounter = 0f;
	}

    Vector2 CalcVelocity(Vector2 playerInput) {
		float targetVelocity = playerInput.x * (playerRunning ? runSpeed : walkSpeed);
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocitySmoothing, (collisions.below ? accTimeGrounded : accTimeAirborne));
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

				collisions.left = dirX == -1;
				collisions.right = dirX == 1;
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

				collisions.below = dirY == -1;
				collisions.above = dirY == 1;
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

	struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public void Reset() {
			above = below = false;
			left = right = false;
		}
	}
}
