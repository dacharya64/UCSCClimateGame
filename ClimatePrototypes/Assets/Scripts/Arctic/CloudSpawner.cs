using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;

public class CloudSpawner : MonoBehaviour {
	public enum CloudType { High, Low }

	[SerializeField] CloudType type = CloudType.High;
	[SerializeField] GameObject cloudPrefab = default;
	[SerializeField] GameObject darkCloudPrefab = default;
	[SerializeField] bool canSpawn = true;
	[SerializeField] float cloudSpawnWaitSeconds = 8f;
	Transform cloudParent;
	public float chanceOfDarkCloud = 0.5f;
	public float darkCloudReflectivity = 0.75f; // if this is higher, clouds let more through
	bool left = true;

	void Start() {
		cloudParent = new GameObject($"{type.ToString()} Cloud").transform;
		if (canSpawn)
			StartCoroutine(SpawnCloud());
		if (Random.value >.5) {
			left = false;
			transform.position = Vector3.Scale(transform.position, new Vector3(-1, 1, 1));
		}
	}

	IEnumerator SpawnCloud() {
		yield return new WaitForSeconds(Random.Range(3f, cloudSpawnWaitSeconds) * (1 - .5f * ArcticController.Instance.tempInfluence) * (ArcticController.Instance.summer ? 1 : .8f));
		float rand = Random.value;
		if (rand <= chanceOfDarkCloud)
		{
			GameObject darkCloud = Instantiate(darkCloudPrefab, transform.position + transform.up * Random.Range(-0.5f, 1f), Quaternion.identity, cloudParent);
			darkCloud.GetComponent<Cloud>().flipped = !left;
			float rand2 = Random.value;
			if (rand2 <= darkCloudReflectivity)
			{
				darkCloud.GetComponent<BoxCollider2D>().enabled = false;
			}
		}
		else
		{
			Instantiate(cloudPrefab, transform.position + transform.up * Random.Range(-0.5f, 1f), Quaternion.identity, cloudParent).GetComponent<Cloud>().flipped = !left;
		}
		
		StartCoroutine(SpawnCloud());
	}
}
