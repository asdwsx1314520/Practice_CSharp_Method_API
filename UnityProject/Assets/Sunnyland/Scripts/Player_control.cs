using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class Player_control : Creature {
	
	#region 基本控制

	static int F_run_ = Animator.StringToHash ("Base Layer.F_run");
	static int F_squat_ = Animator.StringToHash ("Base Layer.F_squat");
	static int F_jump_ = Animator.StringToHash ("Base Layer.F_jump");
	static int F_reloading_ = Animator.StringToHash ("Base Layer 0.F_reloading");

	public GameObject herobody;//it take

	private bool turnback;
	private bool b_jump;
	public bool b_reloading;

	//Controller2D controller;

	#endregion


	void Awake ()
	{
		HP = 100;
		moveSpeed = 8.0f;
		accelerationTimeAirborne = 0.2f;
		accelerationTimeGrounded = 0.1f;
		jumpHeigh = 3.0f;
		timeToJumpApex = 0.8f;
		gravity = -1.0f;

		Begin ();	
	}

	void Update () 
	{
		float angle = gm.angle;

		if (hp >= 0) 
		{
			AnimaControler ();

			Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
			float TargetVelocityX = input.x * moveSpeed;

			Moveing (TargetVelocityX);

			if (TargetVelocityX != 0) 
			{
				Anim.SetBool ("F_run", true);
			}

			if (Input.GetKeyDown (KeyCode.W)) 
			{
				Anim.SetBool ("F_jump", true);
				b_jumpSpeed = true;
			}else{
				b_jumpSpeed = false;
			}
				
			if (Input.GetKey (KeyCode.R)) 
			{
				Reloading ();
			}
		}else
		{
			Dead ();
		}

		if(!b_reloading && hp >= 0)
			IK_target (angle);
	}


	/// <summary>
	/// 動畫控制
	/// </summary>
	void AnimaControler()
	{
		//==========================================================
		Animing = Anim.GetCurrentAnimatorStateInfo (0);
		Animing_ = Anim.GetCurrentAnimatorStateInfo (1);
		//==========================================================
		if (Animing.fullPathHash == F_jump_) {
			b_jump = true;
		} else {
			b_jump = false;
		}
		if (Animing_.fullPathHash == F_reloading_) {
			b_reloading = true;
		} else {
			b_reloading = false;
		}
		//--------------------------------------------------
		Anim.SetBool ("F_run", false);
		Anim.SetBool ("F_squat", false);
		Anim.SetBool ("F_reloading", false);
		//--------------------------------------------------

		if (b_jump) 
		{
			StartCoroutine (jumping());
		}

	}

	/// <summary>
	/// 腳色視角跟隨滑鼠
	/// </summary>
	void IK_target(float angle)
	{
		if (angle <= 90 && angle >= -95) 
			turnback = true;
		  else 
			turnback = false;

		if (turnback) 
		{
			herobody.transform.rotation = Quaternion.Euler (new Vector2 (0, 0));
		}
		if (!turnback) 
		{
			herobody.transform.rotation = Quaternion.Euler (new Vector2 (0, 180));
		}
	}

	IEnumerator jumping()
	{
		yield return new WaitForSeconds (0.025f);
		Anim.SetBool ("F_jump", false);
	}

	public override void Dead()
	{
		GameMaster.KillPlayer (this,1);
	}

	public void Reloading()
	{
		Anim.SetBool ("F_reloading", true);
	}

	public void OnTriggerEnter2D(Collider2D aaa)
	{
		if (aaa.gameObject.name == "dead") 
		{
			HP -= 110;
		}
	}
}
