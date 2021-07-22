using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LongWaveSpawner : MonoBehaviour { // TODO: rename to be consister with SolarRadiationSpawner
	float ballEmitWaitSeconds = 5.0f;
	[SerializeField] GameObject longWavePrefab = default;

	void Start() => StartCoroutine(EmitBall(1f));

	IEnumerator EmitBall(float waitTime) { // TODO: update long wave radiation based on ratios
		yield return new WaitForSeconds(waitTime);
		for (int i = 0; i < (ArcticController.Instance.summer ? 2 : 3); i++)
			Instantiate(longWavePrefab, transform.position, Quaternion.identity, ArcticController.Instance.longWaveParent);
		if (ArcticController.Instance.summer)
		{
			StartCoroutine(EmitBall(Random.Range(2, 5)));
		}
		else {
			StartCoroutine(EmitBall(Random.Range(4, 7)));
		}
	}
}
