using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

[RequireComponent (typeof(CapsuleCollider2D))]
public class CharacterController2D : MonoBehaviour
{

	#region
	[Header("Collision Detection")]
	const float skinWidth = 0.015f;
	const float raySpacing = 0.125f;

	int horzRayCount;
	int vertRayCount;
	float horzRaySpacing;
	float vertRaySpacing;

	public LayerMask collisionMask;
	#endregion

	[SerializeField]
	float maxSlopeAngle = 60.0f;

    new CapsuleCollider2D collider;
    RayCastOrigins raycastOrigins;
	public CollisionInfo collisions;
	public CharacterState state;

	void Awake() {
        collider = GetComponent<CapsuleCollider2D>();
	}

	void Start() {
		CalculateRaySpacing();
		state.dirFacing = 1;
	}

	public void Move(Vector2 movement) {
		UpdateRaycastOrigins();
		collisions.Reset();
		state.Reset();

		if(movement.y < 0) {
			DescendSlope(ref movement);
		}

		if(movement.x != 0) {
			state.dirFacing = (int)Mathf.Sign(movement.x);
		}

        HorzCollisions(ref movement);

        if(movement.y != 0) {
            VertCollisions(ref movement);
		}


		transform.Translate(movement);
    }

	// Detect objects left/right of our character
	void HorzCollisions(ref Vector2 movement) {
        float dirX = state.dirFacing;
        float rayLength = Mathf.Abs(movement.x) + skinWidth;

		if(Mathf.Abs(movement.x) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		for(int i = 0; i < horzRayCount; i++) {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.btmLeft : raycastOrigins.btmRight;
            rayOrigin += Vector2.up * (horzRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin , Vector2.right * dirX * rayLength, Color.yellow);

            if(hit) {

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				// If bottom right/left ray hits a slope...
				if(i == 0 && slopeAngle <= maxSlopeAngle) {
					ClimbSlope(ref movement, slopeAngle);
				}

				// When we're on flat ground
				if(!state.isClimbingSlope || slopeAngle > maxSlopeAngle) {
					movement.x = (hit.distance - skinWidth) * dirX;
					rayLength = hit.distance;

					// Collisions in front of us while climbing
					if(state.isClimbingSlope) {
						movement.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(movement.x);
					}

					collisions.left = dirX == -1;
					collisions.right = dirX == 1;
				}

            }
		}

	}

	// Detect objects above/below our character
    void VertCollisions(ref Vector2 movement) {
        float dirY = Mathf.Sign(movement.y);
        float rayLength = Mathf.Abs(movement.y) + skinWidth;

		// Capsule
		/*Vector2 pointA = (dirY == -1) ? raycastOrigins.btmLeft : raycastOrigins.topLeft;
		Vector2 pointB = (dirY == -1) ? raycastOrigins.btmRight : raycastOrigins.topRight;
		float radius = Vector2.Distance(pointA, pointB) / 2f;
		Vector3 centrePoint = (pointA + pointB) / 2f;
		Quaternion centreDir = Quaternion.LookRotation((pointB - pointA).normalized, Vector3.up * dirY);
		Debug.DrawRay(centrePoint, Vector2.up * dirY * 0.025f, Color.magenta);*/

		for(int i = 0; i < vertRayCount; i++) {
			// Draw rays in a straight line
            Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.btmLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (vertRaySpacing * i + movement.x);

			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, collisionMask);
			Debug.DrawRay(rayOrigin , Vector2.up * dirY * rayLength, Color.red);

			// Draw rays in a semi circle
			/*float angle = Mathf.PI * (i + 1) / (vertRayCount + 1);
			float xPos = Mathf.Sin(angle) * radius;
			float yPos = Mathf.Cos(angle) * radius;
			Vector3 pointPos = new Vector3(0, xPos, yPos);
			pointPos = centreDir * pointPos;

			RaycastHit2D hit = Physics2D.Raycast(centrePoint + pointPos, pointPos, rayLength, collisionMask);
			Debug.DrawRay(centrePoint + pointPos, pointPos * rayLength, Color.magenta);*/


			if(hit) {
				movement.y = (hit.distance - skinWidth) * dirY;
				rayLength = hit.distance;

				// Collisions above us while climbing
				if(state.isClimbingSlope) {
					movement.x = movement.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(movement.x);
				}

				collisions.below = dirY == -1;
				collisions.above = dirY == 1;
			}

		}
	}

	// Climbing slope logic
	void ClimbSlope(ref Vector2 movement, float slopeAngle) {
		float distToMove = Mathf.Abs(movement.x);
		float distToClimb = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * distToMove;

		if(movement.y <= distToClimb) {
			movement.y = distToClimb;
			movement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * distToMove * Mathf.Sign(movement.x);

			collisions.below = true;
			collisions.slopeAngle = slopeAngle;
			state.isClimbingSlope = true;
		}
	}

	// Descending slope logic
	void DescendSlope(ref Vector2 movement) {
		float dirX = Mathf.Sign(movement.x);
		Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.btmRight : raycastOrigins.btmLeft;
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if(hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

			if(slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
				// If the normal is facing the same direction as our destination
				if(Mathf.Sign(hit.normal.x) == dirX) {
					// Check to see if we're close enough to the slope
					if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(movement.x)) {
						float distToMove = Mathf.Abs(movement.x);
						float distToDescend = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * distToMove;
						movement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * distToMove * Mathf.Sign(movement.x);
						movement.y -= distToDescend;

						collisions.below = true;
						collisions.slopeAngle = slopeAngle;
						state.isDescendingSlope = true;
					}
				}
			}
		}
	}

	// Generate raycasts for collisions
	void UpdateRaycastOrigins() {
		Bounds bounds = collider.bounds;
		bounds.Expand(skinWidth * -2);

		// Box
		raycastOrigins.btmLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.btmRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

		// Capsule
		/*float capsuleRadius = bounds.size.x / 2;
        raycastOrigins.btmLeft = new Vector2(bounds.min.x, bounds.min.y + capsuleRadius);
        raycastOrigins.btmRight = new Vector2(bounds.max.x, bounds.min.y + capsuleRadius);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y - capsuleRadius);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y - capsuleRadius);*/
    }

	void CalculateRaySpacing() {
		Bounds bounds = collider.bounds;
		bounds.Expand(skinWidth * -2);

		// Box
		horzRayCount = Mathf.RoundToInt(bounds.size.y / raySpacing);
		vertRayCount = Mathf.RoundToInt(bounds.size.x / raySpacing);
		horzRaySpacing = bounds.size.y / (horzRayCount - 1);
		vertRaySpacing = bounds.size.x / (vertRayCount - 1);

		// Capsule
		/*float capsuleRadius = bounds.size.x / 2;
		horzRayCount = Mathf.RoundToInt((bounds.size.y - (capsuleRadius * 2)) / raySpacing);
		vertRayCount = Mathf.RoundToInt((Mathf.PI * capsuleRadius) / raySpacing);
		horzRaySpacing = (bounds.size.y - (capsuleRadius * 2)) / (horzRayCount - 1);
		vertRaySpacing = (Mathf.PI * capsuleRadius) / (vertRayCount - 1);*/
	}

	struct RayCastOrigins {
        public Vector2 btmLeft, btmRight;
        public Vector2 topLeft, topRight;
    }

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public float slopeAngle, slopeAnglePrev;

		public void Reset() {
			above = below = false;
			left = right = false;

			slopeAnglePrev = slopeAngle;
			slopeAngle = 0;
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

		public int dirFacing;

		public void Reset() {
			isClimbingSlope = false;
			isDescendingSlope = false;
		}
	}
}
