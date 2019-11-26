using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class Controller2D : MonoBehaviour {
	
	public LayerMask collisionMask; //宣告階層選單

	const float skinWidth = 0.015f;//設置不能改變的寬度 
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float maxClimbAngle = 80;//可爬坡度
	float maxDescendAngle = 80;//最大下坡角度

	float horizontalRaySpacing;
	float verticalRaySpacing;

	CapsuleCollider2D collider;
	RaycastOrigins raycastOrigins; //宣告結構化 以方便使用

	public CollisionInfo collisions;

	public virtual void Awake()
	{
		collider = GetComponent<CapsuleCollider2D> ();
	}

	public virtual void Start()
	{
		CalculateRaySpacing ();
	}

	/// <summary>
	/// 移動
	/// </summary>
	/// <param name="velocity">Velocity.</param>
	public void Move(Vector3 velocity)
	{
		UpdateRaycastOrigins ();//只有移動時才會呼叫射線
		collisions.Reset();
		collisions.velocityOld = velocity;

		if (velocity.y < 0) 
		{
			DescendSlope (ref velocity);
		}
		if (velocity.x != 0) 
		{
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) 
		{
			VerticalCollisions (ref velocity);//可利用ref傳遞並改變參數
		}

		transform.Translate (velocity);
	}

	void HorizontalCollisions(ref Vector3 velocity)
	{
		float directionX = Mathf.Sign (velocity.x);//Sign符號
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;//Abs絕對值

		for (int i = 0; i < horizontalRayCount; i++) //利用迴圈打出多個射線
		{
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight; //以符號判斷1為正 -1為負
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);         //射線起始點
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);//判斷射線左右是否發生碰撞

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);//再利用符號在左右顯示出的射線

			if (hit) //碰撞時停止移動 與 遇是否可爬坡
			{
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);//利用射線計算角度

				if (i == 0 && slopeAngle <= maxClimbAngle)  //如果目前的坡度小於可爬坡度
				{
					if (collisions.descendingSlope) 
					{
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleO1d) //讓碰撞不因射線的緣故無法精準碰撞
					{
						distanceToSlopeStart = hit.distance - skinWidth; 
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope (ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) 
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					//修改在斜坡上撞到東西會狂抖的bug
					if (collisions.climbingSlope) 
					{
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
					}

					//------------------------------------判斷目前狀態
					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
					//-------------------------------------------------判斷目前撞到左邊或右邊
				}
			}
		}
	}

	void VerticalCollisions(ref Vector3 velocity)
	{
		float directionY = Mathf.Sign (velocity.y);//Sign符號
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;//Abs絕對值

		for (int i = 0; i < verticalRayCount; i++) //利用迴圈打出多個射線
		{
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft; 
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);//顯示出的射線

			if (hit) 
			{
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				//修改在斜坡上如果頭頂撞到東西會狂抖得bug
				if (collisions.climbingSlope) 
				{
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}

				//------------------------------------判斷目前狀態
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
				//-------------------------------------------------在Player_control中重力會因時間逐漸累積 因此如果現在踩在地上時就將人造重力規0 (判斷是否在空中)
			}
		}
		//解決爬坡時面對不同坡度會停頓的bug
		if (collisions.climbingSlope) 
		{
			float directionX = Mathf.Sign (velocity.x);
			rayLength = Mathf.Abs (velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit) 
			{
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) 
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	/// <summary>
	/// 在爬坡時讓爬坡的速度等於行走的速度
	/// </summary>
	/// <param name="velocity">Velocity.</param>
	/// <param name="slopeAngle">Slope angle.</param>
	void ClimbSlope(ref Vector3 velocity,float slopeAngle)
	{
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;//sin斜率(角度) 轉換成弧度

		if (velocity.y <= climbVelocityY) 
		{
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);//斜率問題
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	/// <summary>
	/// 可下坡度
	/// </summary>
	/// <param name="velocity">Velocity.</param>
	void DescendSlope(ref Vector3 velocity)
	{
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) 
		{
			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) 
			{
				if (Mathf.Sign (hit.normal.x) == directionX) 
				{
					if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x)) 
					{
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);//斜率問題
						velocity.y -= descendVelocityY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	/// <summary>
	/// 射線起始位置
	/// </summary>
	void UpdateRaycastOrigins()
	{
		Bounds bounds = collider.bounds;//設置邊界
		bounds.Expand(skinWidth * -2);  //將擴展的邊界設定在-2位置上

		//========================================================================設定為邊界上4個點
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
		//========================================================================
	}


	/// <summary>
	/// 射線結束位置
	/// </summary>
	void CalculateRaySpacing()
	{
		Bounds bounds = collider.bounds;//設置邊界
		bounds.Expand(skinWidth * -2);  //將擴展的邊界設定在-2位置上

		//======================================================================設定最小值
		horizontalRayCount = Mathf.Clamp(horizontalRayCount,2,int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount,2,int.MaxValue);
		//======================================================================

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	struct RaycastOrigins //將射線起始結構化
	{
		public Vector2 topLeft,topRight;      //左上,右上
		public Vector2 bottomLeft,bottomRight;//左下,右下
	}

	/// <summary>
	/// 判斷目前狀態
	/// 是否在空中
	/// 目前撞擊位子是否在左邊或在右邊
	/// </summary>
	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;

		/// <summary>
		/// 爬坡
		/// </summary>
		public bool climbingSlope;//上坡
		public bool descendingSlope;//下波
		public float slopeAngle,slopeAngleO1d;//斜角
		public Vector3 velocityOld;


		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleO1d = slopeAngle;
			slopeAngle = 0; //重置
		}
	}


}
