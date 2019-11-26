using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Controller2D target;
	public float verticalOffset;
	public float lookAheadDetX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public float CameraControllerHeight;
	public Vector2 focusAresSize;

	public GameMaster gm;

	FocusArea focusArea;

	float nexTimeToSearch = 0;

	void Start()
	{
		target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Controller2D> ();
		focusArea = new FocusArea (target.GetComponent<CapsuleCollider2D>().bounds,focusAresSize);
	}

	void LateUpdate()
	{
		if(target != null)
		focusArea.Update (target.GetComponent<CapsuleCollider2D>().bounds);

		if (target == null) 
		{
			FindPlayer ();
		}


		transform.position = new Vector3 (focusArea.centre.x , focusArea.centre.y + CameraControllerHeight, -10) + Vector3.up * verticalOffset;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color (1, 0, 0, 0.5f);
		Gizmos.DrawCube (focusArea.centre, focusAresSize);
	}

	void FindPlayer()
	{
		nexTimeToSearch += Time.deltaTime;
		if (nexTimeToSearch >= gm.SpawnDelay) 
		{
			target = GameObject.FindGameObjectWithTag ("Player").GetComponent<Controller2D>();
		}
	}
		
	struct FocusArea
	{
		public Vector2 centre;
		public Vector2 velocity;
		float left,right;
		float top,bottom;

		public FocusArea(Bounds targetBounds,Vector2 size)
		{
			left = targetBounds.center.x - size.x/2;
			right = targetBounds.center.x + size.x/2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			velocity = Vector2.zero;
			centre = new Vector2((left + right)/2,(top + bottom)/2);
		}

		public void Update(Bounds targetBounds)
		{
			float shiftX = 0;
			if (targetBounds.min.x < left) {
				shiftX = targetBounds.min.x - left;
			} else if (targetBounds.max.x > right) {
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			} else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			centre = new Vector2((left + right)/2,(top + bottom)/2);
			velocity = new Vector2 (shiftX, shiftY);
		}
	}
}
