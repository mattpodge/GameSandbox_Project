using System.Collections;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{

    const float skinWidth = 0.015f;

    const float raySpacing = 0.25f;
    int horzRayCount;
    int vertRayCount;
    float horzRaySpacing;
    float vertRaySpacing;

    public LayerMask collisionMask;

    new BoxCollider2D collider;
    RayCastOrigins raycastOrigins;

	void Start() {
        collider = GetComponent<BoxCollider2D>();
		CalculateRaySpacing();
	}

    public void Move(Vector2 movement) {
        UpdateRaycastOrigins();

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
