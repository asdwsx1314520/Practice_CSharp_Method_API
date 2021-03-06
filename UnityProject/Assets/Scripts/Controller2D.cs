﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class Controller2D : MonoBehaviour
{
    [Header("角色身體")]
    public Transform body;

    [Header("角度")]
    float angle;
    /// <summary>
    /// 圖層矇板(判斷射線撞到的物體)
    /// </summary>
    public LayerMask collisionMask;

    /// <summary>
    /// 皮膚寬度
    /// </summary>
    const float skinWidth = 0.015f;

    //水平與垂直射線的數量
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    //水平與垂直射線之間的距離
    float horizontalRaySpacing;
    float verticalRaySpacing;

    new CapsuleCollider2D collider;
    RaycastOrigins raycastOrigins;

    public CollisionInfo collisions;

    void Start()
    {
        collider = GetComponent<CapsuleCollider2D>();
        CalculateRaySpacing();
    }

    /// <summary>
    /// 角色移動
    /// </summary>
    /// <param name="velocity">移動方向</param>
    public Vector2 Move(Vector2 velocity)
    {
        UpdateRaycastOrigins();
        collisions.Reset();

        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }

        if (velocity.y != 0)
        {
            //ref將變數回傳出來
            VerticalCollisions(ref velocity);
        }

        transform.Translate(velocity);

        if (collisions.left || collisions.right) return velocity;

        if (velocity.x > 0) 
        {
            body.rotation = Quaternion.Euler(new Vector2(0, 0));
        }
        else if(velocity.x < 0)
        {
            body.rotation = Quaternion.Euler(new Vector2(0, 180));
        }

        return velocity;
    }

    /// <summary>
    /// 水平碰撞
    /// </summary>
    /// <param name="velocity">要處理的數值</param>
    public void HorizontalCollisions(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
        for (int i = 0; i < horizontalRayCount; i++)
        {
            //玩家是否在移動
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;
                
                //當左邊撞擊到牆壁
                collisions.left = directionX == -1;
                //當右邊撞擊到牆壁時
                collisions.right = directionX == 1;

            }
        }
    }

    /// <summary>
    /// 垂直碰撞
    /// </summary>
    /// <param name="velocity">要處理的數值</param>
    public void VerticalCollisions(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++)
        {
            //玩家是否在下降
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                //當下方撞擊到牆壁時
                collisions.below = directionY == -1;
                //當上方撞擊到牆壁時
                collisions.above = directionY == 1;
            }
        }
    }

    /// <summary>
    /// 射線更新
    /// </summary>
    void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    /// <summary>
    /// 射線起始點與射線間距更新
    /// </summary>
    void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins
    {
        //各個角落
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    /// <summary>
    /// 碰撞訊息
    /// </summary>
    public struct CollisionInfo
    {
        //上下
        public bool above, below;
        //左右
        public bool left, right;

        /// <summary>
        /// 碰撞重置
        /// </summary>
        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
}
