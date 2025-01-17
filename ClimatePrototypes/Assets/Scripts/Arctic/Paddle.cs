using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Paddle : MonoBehaviour {
	Rigidbody2D rb2D;
	[SerializeField] float paddleSpeed = 5f;
	public float paddleWidth;
	float horizontalInput = 0f;
	Vector2 screenBounds; // TODO: global variable class

	void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
		paddleWidth = GetComponent<SpriteRenderer>().bounds.size.x;
	}

	void Update() => horizontalInput = Input.GetAxis("Horizontal");
	void FixedUpdate() => Move(Mathf.Abs(horizontalInput) > 0.1f ? horizontalInput : 0);
	void Move(float input) => rb2D.velocity = Vector3.right * input * paddleSpeed;
	void LateUpdate() {
		paddleWidth = GetComponent<SpriteRenderer>().bounds.size.x;
		transform.position += (Mathf.Clamp(transform.position.x, -screenBounds.x + paddleWidth / 2, screenBounds.x - paddleWidth / 2) - transform.position.x) * Vector3.right;
	}

	/// <summary> handles short wave radiation </summary>
	void OnCollisionEnter2D(Collision2D other) {
		AudioManager.Instance.Play("SFX_Paddle_Bounce"); // TODO: audio global name class
		var ball = other.transform.GetComponent<RadiationBall>();
		if (ball.radiationType == SolarRadiationSpawner.Radiation.ShortWave) ball.Orient();
	}

	/// <summary> handles long wave radiation </summary>
	void OnTriggerEnter2D(Collider2D other) {
		AudioManager.Instance.Play("SFX_Paddle_Bounce"); // TODO: audio global name class
		if (other.transform.GetComponent<RadiationBall>().radiationType == SolarRadiationSpawner.Radiation.LongWave) {
			transform.localScale = new Vector3(Mathf.Max(.5f, transform.localScale.x * .95f), 1, 1);
			Destroy(other.gameObject);
		}
	}

	public void ShrinkPaddle(float amt) {
		transform.localScale = new Vector3(Mathf.Max(.5f, transform.localScale.x * amt), 1, 1);
	}
}
