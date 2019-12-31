using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Controller2D))]
public class Player : MonoBehaviour
{
    [Header("跳躍高度")]
    public float jumpHeight;
    [Header("到達高度所需的時間")]
    public float timeToJumpApex;

    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;

    //移動速度
    float moveSpeed = 10;

    //重力
    float gravity;

    //跳躍速度
    float jumpVelocity;

    //速度
    Vector2 velocity;

    //x平滑
    float velocityXSmoothing;

    Controller2D controller;

    public Vector2 animType;

    void Start()
    {
        controller = GetComponent<Controller2D>();

        //跳躍高度與時間關係的計算
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update()
    {
        if (controller.collisions.above || controller.collisions.below) 
        {
            velocity.y = 0;
        }

        //左右方向點及判斷
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //如果玩家上下空白而且是站在地上
        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below) 
        {
            velocity.y = jumpVelocity;
        }

        float targetVelocityX = input.x * moveSpeed;
        //Mathf.SmoothDamp(原點,目標,平滑數值,所需的時間)
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)? accelerationTimeGrounded: accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;

        animType = controller.Move(velocity * Time.deltaTime);
    }
}
