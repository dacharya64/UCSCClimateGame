using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class WaterArrow : MonoBehaviour {
	Transform player;
	public Vector3 waterPosition;
	[SerializeField] float speed = 10f;

	void Start() => player = SubtropicsController.Instance.player.transform;

	void FixedUpdate() { // TODO: fix this rotation math
		// Get degrees between two vectors
		Vector3 targetDir = waterPosition - player.position;
		Vector3 currentDir = (player.position - transform.position);

		float crossP = Vector3.Cross(targetDir, currentDir).z;

		float angle = Vector3.Angle(currentDir, targetDir);
		float rotateSpeed = Mathf.Sign(crossP) * speed;

		if (Mathf.Abs(180 - angle) > 15) // damping
			transform.RotateAround(player.transform.position, Vector3.forward, rotateSpeed * Time.fixedDeltaTime);

		// point local right to parent object - red-axis-x
		transform.up = -(player.transform.position - transform.position);
	}
}
