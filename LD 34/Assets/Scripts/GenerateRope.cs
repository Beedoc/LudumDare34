using UnityEngine;
using System.Collections;

public class GenerateRope : MonoBehaviour {

	public GameObject player1;
	public GameObject player2;

	public KeyCode player1InputKeyboard;
	public KeyCode player1InputPad;
	public KeyCode player2InputKeyboard;
	public KeyCode player2InputPad;

	float energyMax = 100;
	float costForBlocking = 20f;
	float costForAttacking = 35f;
	float costForSpamming = 25f;
	float costForResting = 15f;

	bool player1Pressed;
	float player1PressedLastTime = -1000f;
	bool player1Released;
	float player1ReleasedLastTime = -1000f;
	public bool player1Attacking;
	public bool player1Spamming;
	public bool player1Blocking;
	public bool player1Resting;
	public bool player1isStunned;
	public float player1Energy;

	bool player2Pressed;
	float player2PressedLastTime = -1000f;
	bool player2Released;
	float player2ReleasedLastTime= -1000f;
	public bool player2Spamming;
	public bool player2Attacking;
	public bool player2Blocking;
	public bool player2Resting;
	public bool player2isStunned;
	public  float player2Energy;

	//coroutie mngt
	Coroutine P1Block;
	Coroutine P1Attack;
	Coroutine P1Rest;

	Coroutine P2Block;
	Coroutine P2Attack;
	Coroutine P2Rest;

	private float intervalleForSpamming = .5f;
	private float delayBeforeResting = 1f;
	private float delayBeforeBlocking = 1f;


	public GameObject ropePart;
	public int ropeSize = 200;

	private GameObject[] rope;

	private bool gameEnded = false;

	// Use this for initialization
	void Start () {

		rope = new GameObject[this.ropeSize];

		for (int i =0; i < this.ropeSize; i++)
		{
			this.rope[i] = Instantiate(ropePart, new Vector3(-8f + i * .15f,0,0), Quaternion.identity) as GameObject;

			if (i > 0)
				this.rope[i-1].GetComponent<HingeJoint2D>().connectedBody = this.rope[i].GetComponent<Rigidbody2D>();
		}

	/*	HingeJoint2D[] P1joints = this.player1.GetComponentsInChildren<HingeJoint2D> ();
		for (int i = 0; i < P1joints.Length; i++) {
			P1joints [i].connectedBody = this.rope [0].GetComponent<Rigidbody2D>();
		}

		HingeJoint2D[] P2joints = this.player2.GetComponentsInChildren<HingeJoint2D> ();
		for (int i = 0; i < P2joints.Length; i++) {
			P2joints [i].connectedBody = this.rope [this.rope.Length -1].GetComponent<Rigidbody2D>();
		}*/

		this.player1.GetComponent<HingeJoint2D> ().connectedBody = this.rope [0].GetComponent<Rigidbody2D> ();
		this.player2.GetComponent<HingeJoint2D> ().connectedBody = this.rope [this.rope.Length - 1].GetComponent<Rigidbody2D> ();

		Destroy (this.rope [this.rope.Length - 1].GetComponent<HingeJoint2D> ());

		//initPlayer;
		this.player1Resting = this.player2Resting = true;
		this.player1Energy = this.player2Energy = this.energyMax;
	}
	
	// Update is called once per frame
	void Update () {

		if (gameEnded) {
			if ((Input.GetKey (this.player1InputKeyboard) || Input.GetKey (this.player1InputPad)) && (Input.GetKey (this.player2InputKeyboard) || Input.GetKey (this.player2InputPad)))
				Application.LoadLevel (Application.loadedLevel);

			return;
		}

		#region energy from state
		if (player1Blocking)
			player1Energy = Mathf.Max(0, player1Energy -costForBlocking * Time.deltaTime);
		else if (player1Resting)
			player1Energy = Mathf.Min(energyMax, player1Energy  + costForResting* Time.deltaTime);
		else if (player1Spamming)
			player1Energy = Mathf.Max(0, player1Energy - costForSpamming* Time.deltaTime);

		if (player2Blocking)
			player2Energy = Mathf.Max(0, player2Energy -costForBlocking* Time.deltaTime);
		else if (player2Resting)
			player2Energy = Mathf.Min(energyMax, player2Energy  + costForResting* Time.deltaTime);
		else if (player2Spamming)
			player2Energy = Mathf.Max(0, player2Energy - costForSpamming* Time.deltaTime);

		if (player1Energy == 0)
		{
			Debug.Log("set false");
			player1.GetComponent<Animator>().SetBool("IsBLocking",false);
			Debug.Log(player1.GetComponent<Animator>().GetBool("IsBlocking"));
			player1Resting = true;
			player1isStunned = true;
			player1Blocking = false;
			player1Spamming = false;
		}

		if (player2Energy == 0)
		{
			player2.GetComponent<Animator>().SetBool("IsBLocking",false);
			player2Resting = true;
			player2isStunned = true;
			player2Blocking = false;
			player2Spamming = false;
		}

		if (player1isStunned && this.player1Energy == this.energyMax)
			player1isStunned = false;

		if (player2isStunned && this.player2Energy == this.energyMax)
			player2isStunned = false;


		#endregion

		#region inputs
		if ((Input.GetKeyDown(this.player1InputKeyboard) || Input.GetKeyDown(this.player1InputPad)) && !player1isStunned) {
			if (P1Rest !=null) StopCoroutine(P1Rest);
			if (P1Attack !=null) StopCoroutine(P1Attack);

			this.player1Pressed = true;
			this.player1Resting = false;
			this.player1PressedLastTime = Time.timeSinceLevelLoad;

			if (Time.timeSinceLevelLoad - this.player1ReleasedLastTime < this.intervalleForSpamming)
				this.player1Spamming = true;
			else
				P1Block = StartCoroutine("GoingToBlock", "Player1");

			this.player1ReleasedLastTime = - 1000f;
		}
		else if ((Input.GetKeyUp(this.player1InputKeyboard) || Input.GetKeyUp(this.player1InputPad)) && !player1isStunned) {
			this.player1Released = true;

			if (player1Blocking)
			{
				player1.GetComponent<Animator>().SetBool("IsBLocking",false);
				P1Rest = StartCoroutine("GoingToRest", "Player1");
				this.player1Blocking = false;
			}
			else
			{
				this.player1ReleasedLastTime = Time.timeSinceLevelLoad;
				this.player1PressedLastTime = -1000f;
				P1Attack = StartCoroutine("GoingToAttack", "Player1");
				StopCoroutine(P1Block);
			}

		}


		if ((Input.GetKeyDown(this.player2InputKeyboard) || Input.GetKeyDown(this.player2InputPad))  && !player2isStunned) {
			if (P2Rest !=null) StopCoroutine(P2Rest);
			if (P2Attack !=null) StopCoroutine(P2Attack);

			this.player2Pressed = true;
			this.player2Resting = false;
			this.player2PressedLastTime = Time.timeSinceLevelLoad;

			if (Time.timeSinceLevelLoad - this.player2ReleasedLastTime < this.intervalleForSpamming)
				this.player2Spamming = true;
			else
				P2Block = StartCoroutine("GoingToBlock", "Player2");

			this.player2ReleasedLastTime = - 1000f;
		}
		else if ((Input.GetKeyUp(this.player2InputKeyboard) || Input.GetKeyUp(this.player2InputPad)) && !player2isStunned) {
			this.player2Released = true;

			if (player2Blocking)
			{
				player2.GetComponent<Animator>().SetBool("IsBLocking",false);
				P2Rest = StartCoroutine("GoingToRest", "Player2");
				this.player2Blocking = false;
			}
			else
			{
				this.player2ReleasedLastTime = Time.timeSinceLevelLoad;
				this.player2PressedLastTime = -1000f;
				P2Attack = StartCoroutine("GoingToAttack", "Player2");
				StopCoroutine(P2Block);
			}
		}
		#endregion
	}

	void FixedUpdate()
	{
		//Temp 
		//this.player1Attacking = false;
		if (gameEnded)
			return;


		if (this.player1Attacking) {

			if (this.player2Blocking)
			{
				this.rope[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(-250,0),ForceMode2D.Impulse);
			}
			else
			{
				this.rope[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(-500,0),ForceMode2D.Impulse);
			}

			this.player1Attacking = false;
		}
		else if (this.player1Spamming)
		{
			if (this.player2Blocking)
			{
				this.rope[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(-15,0),ForceMode2D.Impulse);
			}
			else
			{
				this.rope[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(-50,0),ForceMode2D.Impulse);
			}
		}

		if (this.player2Attacking) {
			
			if (this.player1Blocking)
			{
				this.rope[this.rope.Length-1].GetComponent<Rigidbody2D>().AddForce(new Vector2(250,0),ForceMode2D.Impulse);
			}
			else
			{
				this.rope[this.rope.Length-1].GetComponent<Rigidbody2D>().AddForce(new Vector2(500,0),ForceMode2D.Impulse);
			}
			
			this.player2Attacking = false;
		}
		else if (this.player2Spamming)
		{
			if (this.player1Blocking)
			{
				this.rope[this.rope.Length-1].GetComponent<Rigidbody2D>().AddForce(new Vector2(15,0),ForceMode2D.Impulse);
			}
			else
			{
				this.rope[this.rope.Length-1].GetComponent<Rigidbody2D>().AddForce(new Vector2(50,0),ForceMode2D.Impulse);
			}
		}

	}

	void LateUpdate()
	{
		if (gameEnded)
			return;

		//check victory
		if (this.player1.transform.position.x > 0) {
			//victory player 2
			gameEnded = true;
		} else if (this.player2.transform.position.x < 0) {
			//victory player 1
			gameEnded = true;
		}

	}

	IEnumerator GoingToBlock(string player)
	{
		yield return new WaitForSeconds (this.delayBeforeBlocking);
		if (player == "Player1") {
			Debug.Log (player1.GetComponent<Animator> ().GetBool ("IsBlocking"));
			player1.GetComponent<Animator> ().SetBool ("IsBlocking", true);
			Debug.Log ("afer: " + player1.GetComponent<Animator> ().GetBool ("IsBlocking"));
			this.player1Blocking = true;
			this.player1Resting = false;
			this.player1Spamming = false;
		} else if (player == "Player2") {
			Debug.Log (player2.GetComponent<Animator> ().GetBool ("IsBlocking"));
			player2.GetComponent<Animator>().SetBool("IsBLocking",true);
			Debug.Log ("afer: " + player2.GetComponent<Animator> ().GetBool ("IsBlocking"));
			this.player2Blocking = true;
			this.player2Resting = false;
			this.player2Spamming = false;
		}
	}

	IEnumerator GoingToRest(string player)
	{
		yield return new WaitForSeconds (this.delayBeforeResting);
		if (player == "Player1") {
			this.player1Blocking = false;
			this.player1Resting = true;
			this.player1Spamming = false;
		} else if (player == "Player2") {
			this.player2Blocking = false;
			this.player2Resting = true;
			this.player2Spamming = false;
		}
	}

	IEnumerator GoingToAttack(string player)
	{
		yield return new WaitForSeconds (this.intervalleForSpamming);
		if (player == "Player1") {
			if (this.player1Energy >= this.costForAttacking) {
				this.player1Energy = Mathf.Max (0, this.player1Energy - this.costForAttacking);
				this.player1Attacking = true;
			}
		} else if (player == "Player2") {
			if (this.player2Energy >= this.costForAttacking) {
				this.player2Energy = Mathf.Max (0, this.player2Energy - this.costForAttacking);
				this.player2Attacking = true;
			}
		}
	}
}
