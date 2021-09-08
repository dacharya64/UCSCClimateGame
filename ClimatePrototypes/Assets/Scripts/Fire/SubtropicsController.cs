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
	public Text fireActivityText;
	public Text waterLevelText;
	public static double fireNumber = 0;
	[HideInInspector] public Wind wind;
	[HideInInspector] public int difficulty = 2;
	[HideInInspector] public SubtropicsWorld world;
	public static SubtropicsWorld World { get => Instance.world; }
	[SerializeField] public GameObject aboutPrompt;

	void Start() {
		UIController.Instance.timed = true;
		UIController.Instance.aboutPrompt = aboutPrompt;

		fireNumber = 0;
		wind = GetComponentInChildren<Wind>();
		world = GetComponentInChildren<SubtropicsWorld>();
		if (base.GetSubtropicsTemp() < 24)
		{
			difficulty = 1;
			fireActivityText.text = "Very Low";
			waterLevelText.text = "Very High";
		}
		else if (base.GetSubtropicsTemp() >= 24 && base.GetSubtropicsTemp() < 25)
		{
			difficulty = 2;
			fireActivityText.text = "Low";
			waterLevelText.text = "High";
		}
		else if (base.GetSubtropicsTemp() >= 25 && base.GetSubtropicsTemp() < 27)
		{
			difficulty = 3;
			fireActivityText.text = "Moderate";
			waterLevelText.text = "Moderate";
		}
		else if (base.GetSubtropicsTemp() >= 27 && base.GetSubtropicsTemp() < 29)
		{
			difficulty = 4;
			fireActivityText.text = "High";
			waterLevelText.text = "Low";
		}
		else {
			difficulty = 5;
			fireActivityText.text = "Very High";
			waterLevelText.text = "Very Low";
		}
	}

	protected override void GameOver() {
		double effect = GetFirePercentage();
		fireNumber = effect;
		base.GameOver();

		Debug.Log("Number of fires: " + effect);

		if (effect < 10)
		{
			base.ChangePublicOpinion(5);
			EBM.F = EBM.F - .02;
		}
		else if (effect >= 10 && effect < 20)
		{
			base.ChangePublicOpinion(-3);
			EBM.F = EBM.F - .04;
		}
		else if (effect >= 20 && effect < 35)
		{
			base.ChangePublicOpinion(-6);
			EBM.F = EBM.F - .06;
		}
		else if (effect >= 35 && effect < 50)
		{
			base.ChangePublicOpinion(-9);
			EBM.F = EBM.F - .08;
		}
		else if (effect >= 50) {
			base.ChangePublicOpinion(-12);
			EBM.F = EBM.F - .1;
		}
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

	public void OpenAbout()
	{
		//base.SetPause(1);
		aboutPrompt.SetActive(true);
	}

	public void CloseAbout()
	{
		aboutPrompt.SetActive(false);
		base.SetPause(0);
	}
}
