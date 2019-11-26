using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour 
{
	public static GameMaster gm;

	public Player_control player;//玩家
	public GameObject Prefad_Player;
	public Transform[] Player_Born;
	public float SpawnDelay = 8;
	public float MonsterDelay;
	public int MonstersContorler = 5; //怪物數量控制
	public int MonsterNow;

	public float angle;

	public bool Will;
	public bool Lose;
	public Text GameTime;
	public float Minutes, Seconds;
	public float WillTime;

	public GameObject CPanim;
	public bool b_Start;
	public Sprite I_Lose;
	public Sprite I_Will;

	public ParticleSystem Lightning;

	public void Begim()
	{
		CPanim.SetActive (true);
	}

	IEnumerator GameBegim()
	{
		yield return new WaitForSeconds (1);
		Begim ();
	}

	public void FindPlayer()
	{
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player_control>();
	}

	void Awake () 
	{		
		Cursor.visible = false; //隱藏滑鼠

		if (gm == null) 
			gm = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();
		
		if (player == null)
			FindPlayer();

	}

	/// <summary>
	/// 玩家位置&&gm = 玩家位置
	/// </summary>
	void PlayerHere()
	{
		if (player != null)
			this.transform.position = player.transform.position;
		else {
			this.transform.position = new Vector3 (0, 0, 0);
		}
	}

	/// <summary>
	/// 主角重生點
	/// </summary>
	/// <returns>The player.</returns>
	public IEnumerator RespawnPlayer()
	{
		yield return new WaitForSeconds (SpawnDelay);
		int I = Random.Range (0, 1);
		if(player == null)
			Instantiate (Prefad_Player, Player_Born[I].transform.position, Player_Born[I].transform.rotation);
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player_control>();
	}

	public static void KillPlayer(Player_control player ,float Deadtime)
	{
		Destroy (player.gameObject , Deadtime);
		gm.StartCoroutine (gm.RespawnPlayer ());
	}

	/// <summary>
	/// 時間 以後可加到Mathf裡面
	/// </summary>
	public void Clock()
	{
		Minutes = Mathf.Clamp (Minutes, 0, float.MaxValue);
		Seconds = Mathf.Clamp (Seconds, 0, 60);

		if (!Lose) 
		{
			if (WillTime != 3) 
			{
				if (Seconds > 0)
				{ //可改成Swich 也可用迴圈
					Seconds -= 1 * Time.deltaTime;
				}else if (WillTime != 3) 
				{
					Minutes--;
					WillTime++;
					Seconds = 59;
				}
			} else 
			{
				Seconds = 0;
				Will = true;
			}
		}
		GameTime.text = string.Format ("{0}:{1:00}", Minutes, Seconds);
	}


	/// <summary>
	/// 輸贏判斷
	/// </summary>
	public void Judgment()
	{
		if(Will)
		{
			CPanim.SetActive (true);
			CPanim.GetComponent<SpriteRenderer> ().sprite = I_Will;
		}
		if (Lose) 
		{
			CPanim.SetActive (true);
			CPanim.GetComponent<SpriteRenderer> ().sprite = I_Lose;
		}	
	}

	void Update()
	{
		if (b_Start) 
		{
			gm.PlayerHere ();

		}
	}
}
