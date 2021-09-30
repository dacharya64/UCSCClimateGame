using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

public class Factory : MonoBehaviour {
	BoxCollider2D col;
	int step = 0;
	int damageMultiplier = 1;
	public static int protesters = 0;
	Dictionary<Transform, bool> subtargets = new Dictionary<Transform, bool>();
	int awaitSubTargetReached = 0;
	public static int counter = 0;
	int workersAtFactory = 0;

	void Start() {
		counter = 0;
		col = GetComponent<BoxCollider2D>();
		foreach (Transform child in transform)
			subtargets.Add(child, false);
	}

	void FixedUpdate() {
	//	if (step++ % 10 == 0)
			//ForestController.Instance.damage += damageMultiplier * ((1 - protesters / ForestController.Instance.numActive) * 2 / 3f + 1 / 3f);
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent(out Volunteer v)) {
			// Debug.Log(other.gameObject);
			v.OnReached.Invoke(v);
			//v.Stop();
		}
	}

	void OnMouseDown() {
		if (ForestController.Instance.hasSelected)
		{
			workersAtFactory++;
			if (workersAtFactory == 1)
			{
				ForestController.forcingIncrease -= 0.05;
			}
			else if (workersAtFactory == 2)
			{
				ForestController.forcingIncrease -= 0.075;
			}
			else if (workersAtFactory == 3)
			{
				ForestController.forcingIncrease -= 0.1;
			}
			else if (workersAtFactory == 4)
			{
				ForestController.forcingIncrease -= 0.125;
			}
			else
			{ //workersAtFactory = 5
				ForestController.forcingIncrease -= 0.15;
			}
			

			var selectedTarget = subtargets.ElementAt(counter).Key;
			counter++;
			//var selectedTarget = subtargets.Where(kvp => !kvp.Value).OrderBy(kvp => kvp.Key.position.y).ElementAt(0).Key;
			subtargets[selectedTarget] = true;
			ForestController.Instance.SetVolunteerTarget(selectedTarget.position, VolunteerActions.Protest);
		}
	}
}
