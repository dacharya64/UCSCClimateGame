using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Math = System.Math;

using UnityEngine;
using UnityEngine.UI;

public class ArcticController : RegionController {
	public static ArcticController Instance { get => instance as ArcticController; } // static instance

	[SerializeField] SeasonCycle cycle = default;
	public bool summer { get => cycle.isSummer; } // used for state changes in region
	/// <summary> present Buffers </summary>
	[HideInInspector] public Buffer[] buffers;
	[SerializeField] Transform ice = default;
	[HideInInspector] public Transform longWaveParent;
	[HideInInspector] public float tempInfluence;
	public float difficulty;
	[SerializeField] GameObject lowCloudSpawner;
	[SerializeField] GameObject highCloudSpawner;
	[SerializeField] GameObject paddle;
	[SerializeField] GameObject aboutPrompt;
	[SerializeField] Text seaIceLossText;

	void Start() {
		longWaveParent = new GameObject("Long Wave Ray").transform;
		// init temp influence, drives game difficulty
		tempInfluence = (float) (World.temp[2] - World.startingTemp[2]) / World.maxTempChange;
		//Debug.Log($"Arctic temp influence is: {tempInfluence}");
		Paddle paddleScript = paddle.GetComponent<Paddle>();

		if (base.GetArcticTemp() < -6)
		{
			difficulty = 1f;
			lowCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.8f);
			highCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.8f);
			seaIceLossText.text = "Very Low";
		}
		else if (base.GetArcticTemp() >= -6 && base.GetArcticTemp() < -1)
		{
			difficulty = 2f;
			lowCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.6f);
			highCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.6f);
			paddleScript.ShrinkPaddle(0.90f);
			seaIceLossText.text = "Low";
		}
		else if (base.GetArcticTemp() >= -1 && base.GetArcticTemp() < 4)
		{
			difficulty = 3f;
			lowCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.4f);
			highCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.4f);
			paddleScript.ShrinkPaddle(0.80f);
			seaIceLossText.text = "Moderate";
		}
		else if (base.GetArcticTemp() >= 4 && base.GetArcticTemp() < 9)
		{
			difficulty = 4f;
			lowCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.2f);
			highCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0.2f);
			paddleScript.ShrinkPaddle(0.70f);
			seaIceLossText.text = "High";
		}
		else
		{
			difficulty = 5f;
			lowCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0f);
			highCloudSpawner.GetComponent<CloudSpawner>().SetChanceOfDarkCloud(0f);
			paddleScript.ShrinkPaddle(0.60f);
			seaIceLossText.text = "Very High";
		}


		buffers = ice.GetComponentsInChildren<Buffer>();
		int totalHealth = buffers.Length * buffers[0].health;
		// dock health proportional to warming
		for (int i = 0; i < Math.Floor(difficulty - 1);) {
			var buff = buffers[Random.Range(0, buffers.Length)];
			if (buff.health > 0) {
				buff.health--;
				i++;
				buff.AssignSprite();
			}
		}
	}

	protected override void GameOver() {
		base.Loading(false);
		base.GameOver();
		Debug.Log($"Remaining {buffers.Select(b => b.health).Aggregate((sum, b) => b + sum)} ice of total {buffers.Length * 5} ice");
		// arctic does not affect model
		// TriggerUpdate(() => World.albedo.Update(World.Region.Arctic, World.Region.City, effect));
	}

	public void OpenAbout()
	{
		base.SetPause(1);
		aboutPrompt.SetActive(true);
	}

	public void CloseAbout()
	{
		aboutPrompt.SetActive(false);
		base.SetPause(0);
	}
}
