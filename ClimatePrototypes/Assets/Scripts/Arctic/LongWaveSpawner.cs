using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LongWaveSpawner : MonoBehaviour { // TODO: rename to be consister with SolarRadiationSpawner
	[SerializeField] GameObject longWavePrefab = default;

	void Start() => StartCoroutine(EmitBall(Random.Range(1.4f, 2.6f)));

	IEnumerator EmitBall(float waitTime) { // TODO: update long wave radiation based on ratios
		yield return new WaitForSeconds(waitTime);
		for (int i = 0; i < (ArcticController.Instance.summer ? 2 : 2); i++)
			Instantiate(longWavePrefab, transform.position, Quaternion.identity, ArcticController.Instance.longWaveParent);
		if (ArcticController.Instance.summer)
		{
			StartCoroutine(EmitBall(Random.Range(1f, 2.2f)));
		}
		else {
			StartCoroutine(EmitBall(Random.Range(1.4f, 2.6f)));
		}
	}
}
