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

    new BoxCollider2D collider;
    RayCastOrigins raycastOrigins;
	public CollisionInfo collisions;
	public CharacterState state;

	void Awake() {
        collider = GetComponent<BoxCollider2D>();
	}

	void Start() {
		CalculateRaySpacing();
	}

	public void Move(Vector2 movement) {
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

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if(i == 0) {
					float distToMove = Mathf.Abs(movement.x);
					float distToClimb = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * distToMove;

					if(movement.y <= distToClimb) {
						movement.y = distToClimb;
						movement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * distToMove * Mathf.Sign(movement.x);
						collisions.below = true;
						state.isClimbingSlope = true;
					}
				}

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

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public void Reset() {
			above = below = false;
			left = right = false;
		}
	}

	// Keep track of states
	public struct CharacterState {
		public bool isGrounded;
		public bool isJumping;
		public bool isRunning;
		public bool isFalling;
		public bool isClimbingSlope;
		public bool isDescendingSlope;
	}
}
