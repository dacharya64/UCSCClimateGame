using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Radiation = SolarRadiationSpawner.Radiation;

public class RadiationBall : MonoBehaviour {
	Rigidbody2D rb;
	[SerializeField] public Vector2 force = new Vector2(2, 5);
	Vector2 screenMin, screenMax; // TODO: global variable class
	public Radiation radiationType = Radiation.ShortWave;
	public int count = 1;

	void Start() {
		screenMin = Camera.main.ViewportToWorldPoint(Vector2.zero);
		screenMax = Camera.main.ViewportToWorldPoint(Vector2.one);
		rb = GetComponent<Rigidbody2D>();

		if (radiationType == Radiation.ShortWave) {
			rb.velocity = new Vector2(Random.Range(-force.x, force.x), -Random.Range(force.y * 0.8f, force.y));
		}
			
		else { // if longwave radiation
            if (count == 1)
            {
                force = new Vector2(0.5f, -5);
            }
            else
            {
                force = new Vector2(0.5f, 5);
            }
            rb.velocity = force;
		}
		Orient();
	}

	/// <summary> points ball in proper direction </summary>
	public void Orient() => transform.eulerAngles = Vector3.forward * (Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg + 90);

	void FixedUpdate() {
		if (transform.position.y < screenMin.y)
			ArcticController.Instance.damage += 10;
		// TODO: do this ↓ better with array + linq
		if (transform.position.x < screenMin.x || transform.position.x > screenMax.x || transform.position.y < screenMin.y || (transform.position.y > screenMax.y && rb.velocity.y > 0))
			Destroy(gameObject);
	}
}
