using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Creature : MonoBehaviour {

	public Animator Anim;
	public AnimatorStateInfo Animing;
	public AnimatorStateInfo Animing_;

	public Controller2D controller;

	public GameMaster gm;

	protected Vector3 velocity;

	protected float hp;
	public float HP
	{
		get{ return hp; }
		set{ if (value < 0)
				hp = 0;
			else
				hp = value;}
	}
	public string Name;

	public float moveSpeed; //移動速度

    public float jumpSpeed; //跳耀速度
    public bool b_jumpSpeed;

    public float accelerationTimeAirborne;//平滑
    public float accelerationTimeGrounded;//平滑

    public float jumpHeigh;         //跳躍高度
    public float timeToJumpApex; //到頂點需要的時間

    public float gravity;//重力
    public float velocityXSmoothing;//跳躍頂點

	public virtual void Begin()
	{
		if (gm == null) 
			gm = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();
		
		controller = GetComponent<Controller2D> ();
		gravity = -(2 * jumpHeigh) / Mathf.Pow (timeToJumpApex, 2);
		jumpSpeed = Mathf.Abs (gravity) * timeToJumpApex;
	}
		
	public void Moveing(float TargetVelocityX)
	{
		if(controller.collisions.above || controller.collisions.below)//射線hit時無重力
		{
            print(123);
			velocity.y = 0;
		}

		if (b_jumpSpeed && controller.collisions.below) 
		{
			velocity.y = jumpSpeed;
		}

		velocity.x = Mathf.SmoothDamp (velocity.x, TargetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);//平滑
		velocity.y += gravity * Time.deltaTime; //重力自己造
		controller.Move(velocity * Time.deltaTime);//ink利用這段移動
	}

	public void Damg(float damg)
	{
		HP -= damg;
		if(HP <= 0)
			Dead ();
	}

	public virtual void Dead()
	{
	}
}
