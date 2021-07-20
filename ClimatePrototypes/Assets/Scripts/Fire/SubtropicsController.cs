using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using Math = System.Math;

using GlobalWorld = World;

public class SubtropicsController : RegionController {
	public static SubtropicsController Instance { get => instance as SubtropicsController; } // static instance

	public SubtropicsPlayer player;
	[HideInInspector] public Wind wind;
	[HideInInspector] public int difficulty = 2;
	[HideInInspector] public SubtropicsWorld world;
	public static SubtropicsWorld World { get => Instance.world; }

	void Start() {
		wind = GetComponentInChildren<Wind>();
		world = GetComponentInChildren<SubtropicsWorld>();
		if (base.GetSubtropicsTemp() < 20)
		{
			difficulty = 1;
		}
		else if (base.GetSubtropicsTemp() >= 20 && base.GetSubtropicsTemp() < 25)
		{
			difficulty = 2;
		}
		else if (base.GetSubtropicsTemp() >= 25 && base.GetSubtropicsTemp() < 30)
		{
			difficulty = 3;
		}
		else if (base.GetSubtropicsTemp() >= 30 && base.GetSubtropicsTemp() < 35)
		{
			difficulty = 4;
		}
		else {
			difficulty = 5;
		}
	}

	protected override void GameOver() {
		base.GameOver();
		double effect = GetFirePercentage();
		Debug.Log("Number of fires: " + effect);

		if (effect >= 5 && effect < 10)
		{
			base.ChangePublicOpinion(-5);
			EBM.F = EBM.F - 0.2;
		}
		else if (effect >= 10 && effect < 15)
		{
			base.ChangePublicOpinion(-10);
			EBM.F = EBM.F - 0.4;
		}
		else if (effect >= 15 && effect < 20)
		{
			base.ChangePublicOpinion(-15);
			EBM.F = EBM.F - 0.6;
		}
		else if (effect >= 20 && effect < 25)
		{
			base.ChangePublicOpinion(-20);
			EBM.F = EBM.F - 0.8;
		}
		else if (effect >= 25) {
			base.ChangePublicOpinion(-25);
			EBM.F = EBM.F - 1;
		}

		base.ChangeAverageTemp();
		base.Loading(false);
		//TriggerUpdate(() => GlobalWorld.co2.Update(region, delta: -effect)); // [-1, 0]
		// TODO: make the effect non linear
		// TriggerUpdate(() => GlobalWorld.co2.Update(region, delta: -Math.Min(1, Math.Log(effect)))); // [-1, 0]
	}

	public float GetFirePercentage() {
		var (fire, trees) = world.cellArray.Cast<GameObject>().Select(obj =>
			(obj.GetComponent<IdentityManager>().id == IdentityManager.Identity.Fire ? 1 : 0, obj.GetComponent<IdentityManager>().id == IdentityManager.Identity.Tree ? 1 : 0)
		).Aggregate((tup, obj) => (tup.Item1 + obj.Item1, tup.Item2 + obj.Item2));

		//return fire / (float) trees;
		return fire; 
	}
}
