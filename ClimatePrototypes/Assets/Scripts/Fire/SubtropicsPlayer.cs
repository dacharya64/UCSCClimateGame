using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class SubtropicsPlayer : MonoBehaviour {
	[SerializeField] float speed = 10;
	[SerializeField] float cameraSpeed = 20;
	[SerializeField] Text leftWaterUI = default;
	[SerializeField] Transform leftWaterBarUI = default;
	[SerializeField] GameObject line = default;
	[SerializeField] Slider waterTracker = default;
	LineRenderer newLine;
	public Material heliLine;

	Animator bladeAnimator;
	TrailRenderer waterTR;
	SpriteRenderer playerRenderer;

	public Vector3? target;
	int water, maxWater = 50;
	bool filling = false, slow = false;
	float lastUsedWater = 0;

	Rigidbody2D body;

	float playerSpeed = 2; //speed player moves
	float turnSpeed = 130; // speed player turns

	void Start() {
		body = GetComponent<Rigidbody2D>();
		bladeAnimator = GetComponentInChildren<Animator>();
		waterTR = GetComponentInChildren<TrailRenderer>();
		playerRenderer = GetComponent<SpriteRenderer>();
		waterTR.enabled = false;

		// draw line
		newLine = line.GetComponent<LineRenderer>();
		//newLine.material = new Material(Shader.Find("Sprites/Default"));

		//newLine.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat"); // TODO: apply your own material
		newLine.material = heliLine;
		newLine.material.SetTextureScale("_MainTex", new Vector2(0.1f, 1.0f));
		newLine.widthMultiplier = 0.2f;

		bladeAnimator.SetBool("isMoving", true);
		water = maxWater;
	}

	void Update() {
		/*float charPosX = transform.position.x;
		float charPosZ = transform.position.z;
		float cameraOffset = 18.0f;

		Camera.main.transform.position = new Vector3(charPosX, cameraOffset, charPosZ);*/
		float step = cameraSpeed * Time.deltaTime;
		//Camera.main.transform.position = Vector2.MoveTowards(Camera.main.transform.position, transform.position, step); // move camera
		Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, new Vector3(transform.position.x, transform.position.y, -10), step);
		//cameraFollow();

		UpdateSlider(waterTracker, water);

		lastUsedWater += Time.deltaTime; // TODO: do this timer logic better, maybe with coroutine

		if (lastUsedWater >= 1f) // turn off renderer if not used water in 1 sec
			waterTR.enabled = false;

		leftWaterUI.text = water.ToString();
		int height = water * 10;
		(leftWaterBarUI as RectTransform).sizeDelta = new Vector2(120, height); // set width is 120. water [0,50], height [0,500]
		(leftWaterBarUI as RectTransform).localPosition = new Vector3(30, -(500 - height) / 2, 0);
		// TODO: convert to slider

		MoveForward(); // Player Movement
		TurnRightAndLeft();//Player Turning
		//Path();
		Extinguish();
	}


	void MoveForward()
	{

		if (Input.GetKey("up") || Input.GetKey(KeyCode.W))//Press up arrow key to move forward on the Y AXIS
		{
			transform.Translate(0, playerSpeed * Time.deltaTime, 0);
		}


		if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
		{
			transform.Translate(0, -playerSpeed * Time.deltaTime, 0);
		}

	}

	void TurnRightAndLeft()
	{

		if (Input.GetKey("right") || Input.GetKey(KeyCode.D)) //Right arrow key to turn right
		{
			transform.Rotate(-Vector3.forward * turnSpeed * Time.deltaTime);
		}

		if (Input.GetKey("left") || Input.GetKey(KeyCode.A)) //Left arrow key to turn left
		{
			transform.Rotate(Vector3.forward * turnSpeed * Time.deltaTime);
		}

	}

	void Path() {
		if (target != null) {
			float step = speed * Time.deltaTime;
			// Are we currently moving towards a region?
			transform.position = Vector3.MoveTowards(transform.position, target.Value, step); // move player
			//Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, Camera.main.transform.position + target.Value - transform.position, step); // move camera
			// TODO: nest camera under player

			Vector3 vectorToTarget = target.Value - transform.position;
			float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90f;
			Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
			transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * speed);

			if (transform.position == target)
				target = null;
			DrawPlayerPath();
		}
	}

	void cameraFollow()
	{
		Debug.Log("Camera following...");
		float charPosX = transform.position.x;
		float charPosZ = transform.position.z;
		float cameraOffset = 18.0f;

		Camera.main.transform.position = new Vector3(charPosX, cameraOffset, charPosZ);
	}

	void Extinguish() {
		// check what cell player is on top of 
		var playerCell = SubtropicsController.World.GetCell(gameObject.transform.position).GetComponent<IdentityManager>();

		//SubtropicsController.World.MutateCell(playerCell, IdentityManager.Identity.Green);
		//playerCell.moisture = IdentityManager.Moisture.Moist;

		// kill all immediate neighbors fire, radius buffer
		foreach (var neighbor in SubtropicsController.World.GetRadius(playerCell.transform.position)) {
			if (neighbor.id == IdentityManager.Identity.Fire && water > 0)
			{
				// check nature of the cell
				if (neighbor.fireVariance == 1)
				{ // if tree
					neighbor.GetComponent<TreeID>().burnt = true;
					neighbor.id = IdentityManager.Identity.Tree;
				}
				else
					neighbor.id = IdentityManager.Identity.Green;
				neighbor.moisture = IdentityManager.Moisture.Moist;
				water = water - 1; // use 1 water per cell
				lastUsedWater = 0; // reset timer
				waterTR.enabled = true;
				AudioManager.Instance.Play("SFX_Smoulder");
				neighbor.GetComponent<AudioSource>().enabled = false;
			}
			else if (neighbor.id == IdentityManager.Identity.Water && water < maxWater) {
				filling = true;
				water += 1;
				AudioManager.Instance.Play("SFX_Refill");
				StartCoroutine(FillWater());
			}
		}

		/*if (!filling && water < maxWater) {
			filling = true;
			water += 1;
			StartCoroutine(FillWater());
		}*/
	}

	IEnumerator FillWater() { // TODO: redo with Invoke and anonymous function instead of coroutine
		yield return new WaitForSeconds(0.1f);
		filling = false;
	}

	void DrawPlayerPath() {
		if (target != null) {
			newLine.positionCount = 2;
			newLine.SetPositions(new Vector3[] { transform.position, target.Value });
		} else
			newLine.positionCount = 0;
	}

	/// <summary> Pulse effect when colliding with cloud </summary>
	/// <param name="other"></param>
	void OnTriggerEnter2D(Collider2D other) {
		if (other.transform.TryGetComponent(out SubtropicsCloud cloud)) {
			//Camera.maib.GetComponent<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
			StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(0.40f, .10f));
			if (!slow)
				StartCoroutine(SlowDown(3f)); // if hit cloud, slows down to 1/4 speed for 3 sec
		}

		if (other.GetComponent("MountainID") != null)
		{
			//Camera.maib.GetComponent<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
			Debug.Log("Hitting mountain");
		}
	}

	IEnumerator SlowDown(float duration, float factor = 4) {
		float slowSpeed = speed / factor;
		slow = true; // todo: maybe do this without a class variable
		for (float elapsed = 0.0f; elapsed < duration; elapsed += Time.deltaTime) {
			speed = slowSpeed;
			yield return null;
		}
		// speed = Mathf.Lerp(slowSpeed,factor * slowSpeed, 0.25f); 
		slow = false;
		speed = factor * slowSpeed;
	}

	void UpdateSlider(Slider slider, float value)
	{
		slider.value = value;
	}
}
