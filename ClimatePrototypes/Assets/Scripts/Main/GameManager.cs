﻿using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : Singleton<GameManager> {
	[SerializeField] GameObject loadingScreen = default, quitPrompt = default, optionsPrompt = default;
	[HideInInspector] public bool runningModel = false;
	public bool runModel = true;
	public List<int> billIndices = new List<int>();
	public float previousMusicVolume;
	public float previousSFXVolume;
	[SerializeField] Slider thermometer;
	public float previousTempValue;
	[SerializeField] StatsPanel statsPanel;
	[SerializeField] GameObject stats;
	float previousPublicOpinion;
	float previousEmissions;
	float previousEconomy;
	float previousLandUse;
	[SerializeField] GameObject tropicsAlert;
	GameObject fireAlert;
	GameObject arcticAlert;
	GameObject cityAlert;
	double previousRegionalTemp;
	double previousArcticTemp;
	int timesSinceVisitedCity = 0;
	int completedRegions = 0;
	GameObject GameOverPrompt;
	[SerializeField] Image thermometerFill;
	Image thermometerBottom;
	public bool isAnimating = false;
	public double forcingIncrease;
	public Text worldNameText;
	public Text worldNameText2;
	public bool hasPlacedWorkers = false;
	public RegionController currentRegion;
	public Dictionary<World.Region, int> visits = new Dictionary<World.Region, int> { { World.Region.Arctic, 0 }, { World.Region.Fire, 0 }, { World.Region.Forest, 0 }, { World.Region.City, 0 } };
	public bool arcticIsAddressed = true;
	public bool tropicsIsAddressed = true;
	public bool subtropicsIsAddressed = true;
	public bool hasShownFirePopup = false;
	public bool menuUp = false;

	public Sprite planet1;
	public Sprite planet2;
	public Sprite planet3;
	public Sprite planet4;

	public override void Awake() {
		base.Awake();
		if (Instance.runModel && World.averageTemp == 0) {
			World.Init();
			// World.ranges = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<double, List<double>>>>(Resources.Load<TextAsset>("ipcc").text);
		}
	}

	public void Start() {
		FindCurrentRegion(SceneManager.GetActiveScene());
		SceneManager.activeSceneChanged += instance.InitScene;
		thermometer = GameObject.FindGameObjectWithTag("Slider").GetComponent<Slider>();
		thermometerFill = GameObject.FindGameObjectWithTag("ThermometerFill").GetComponent<Image>();
		thermometerBottom = GameObject.FindGameObjectWithTag("ThermometerBottom").GetComponent<Image>();
		tropicsAlert = GameObject.FindGameObjectWithTag("TropicsAlert");
		tropicsAlert.GetComponent<SpriteRenderer>().enabled = false;
		fireAlert = GameObject.FindGameObjectWithTag("FireAlert");
		fireAlert.GetComponent<SpriteRenderer>().enabled = false;
		arcticAlert = GameObject.FindGameObjectWithTag("ArcticAlert");
		arcticAlert.GetComponent<SpriteRenderer>().enabled = false;
		cityAlert = GameObject.FindGameObjectWithTag("CityAlert");
		cityAlert.GetComponent<SpriteRenderer>().enabled = false;
		previousRegionalTemp = World.temp[1];
		previousArcticTemp = World.temp[2];
		InitStats();
		SetThermometerValue();
	}

	public void TurnOffMenu() {
		menuUp = false;
		Time.timeScale = 1;
	}

	public void InitStats() {
		statsPanel = stats.GetComponent(typeof(StatsPanel)) as StatsPanel;
		statsPanel.InitializeValues();
	}

	public void SetThermometerValue() {
		SetSlider(thermometer, (float)World.averageTemp);
		previousTempValue = (float)World.averageTemp;
	}

	public static void Restart() {
		EBM.F = 2;
		TitleScreen.isFirstTime = false;
		UIController.Instance.ChangeGameOverPromptState(false);
		SceneManager.LoadScene("TitleScreen");
		//TitleScreen.Instance.TurnOffLoadingText();
		//EBM.Reset();
		//World.Calc();
	}

	public void QuitGame(int exitStatus = 0) {
		switch (exitStatus) {
			case 0:
				instance.quitPrompt.SetActive(true);
				break;
			case 1:
				instance.quitPrompt.SetActive(false);
				break;
			case 2:
				Application.Quit();
				break;
		}
	}

	public void OpenSettings(int settingsStatus = 0)
	{
		switch (settingsStatus)
		{
			case 0:
				SavePreviousVolumes();
				instance.optionsPrompt.SetActive(true);
				break;
			case 1:
				instance.optionsPrompt.SetActive(false);
				break;
		}
	}

	public void SavePreviousVolumes() {
		previousMusicVolume = AudioManager.Instance.totalMusicVolume;
		previousSFXVolume = AudioManager.Instance.totalSFXVolume;
	}

	void InitScene(Scene from, Scene to) {
		instance.loadingScreen.SetActive(false);
		UIController.Instance.ToggleBackButton(to.name != "Overworld");

		if (to.name == "Overworld" && from.name != "TitleScreen") {
			//first, increase emissions according to worker placement in tropics
			// If they have never visited the tropics 
			// Unless you are at title screen 
			if (!hasPlacedWorkers)
			{
				forcingIncrease = EBM.F * 0.1;
			}
			EBM.F = EBM.F + forcingIncrease;
			World.ChangeAverageTemp();
		}

		if (to.name == "Overworld")
		{
			Transform nodes = GameObject.FindWithTag("Nodes").GetComponent<Transform>();
			Transform nodesHot = GameObject.FindWithTag("NodesHot").GetComponent<Transform>();
			// Set the alternate art for regions if it is too hot
			if (World.averageTemp < 20)
			{
				foreach (Transform node in nodes)
				{
					node.gameObject.SetActive(true);
				}

				foreach (Transform node in nodesHot)
				{
					node.gameObject.SetActive(false);
				}
			}
			else
			{
				foreach (Transform node in nodes)
				{
					node.gameObject.SetActive(false);
				}

				foreach (Transform node in nodesHot)
				{
					node.gameObject.SetActive(true);
				}
			}

			
			//add alternate art for planet if world is too hot
			GameObject world = GameObject.Find("world");
			var temp = World.averageTemp;
			temp = 21;
			if (temp > 20 && temp < 22)
			{
				world.GetComponent<SpriteRenderer>().sprite = planet2;
			}
			else if (temp >= 22 && temp < 24)
			{
				world.GetComponent<SpriteRenderer>().sprite = planet3;
			}
			else if (temp >= 24)
			{
				world.GetComponent<SpriteRenderer>().sprite = planet4;
			}
			else {
				world.GetComponent<SpriteRenderer>().sprite = planet1;
			}


			/*if (visits[World.Region.Fire] == 1 && !hasShownFirePopup) {
				UIController.Instance.ChangeInfoBoxState(true);
				hasShownFirePopup = true;
			}*/
			statsPanel = stats.GetComponent(typeof(StatsPanel)) as StatsPanel;
			// Save the previous stats
			previousPublicOpinion = statsPanel.previousPublicOpinion;
			previousEmissions = statsPanel.previousEmissions;
			previousEconomy = statsPanel.previousEconomy;
			previousLandUse = statsPanel.previousLandUse;

			var currentMoney = previousEconomy * 100;
			if (World.money <= 0f && World.money < currentMoney)
			{
				UIController.Instance.ChangeOutOfMoneyPrompt(true);
			} 

			// turn on stats panel
			// check to see if something in the stats panel changed 
			thermometer = GameObject.FindGameObjectWithTag("Slider").GetComponent<Slider>();
			thermometerFill = GameObject.FindGameObjectWithTag("ThermometerFill").GetComponent<Image>();
			thermometerBottom = GameObject.FindGameObjectWithTag("ThermometerBottom").GetComponent<Image>();
			AudioManager.Instance.Play("BGM_Menu"); // TODO: sound name variable class

			thermometer.value = previousTempValue;

			if (from.name != "TitleScreen" && from.name != "Overworld") {
				StartCoroutine(UpdateOverworldValues());
			}
			
		}
		else if (to.name != "TitleScreen") {
			AudioManager.Instance.Play("BGM_" + to.name); // TODO: sound name variable class
		}
		if (to.name != "Overworld")
			FindCurrentRegion(to);
	}

	void FindCurrentRegion(Scene s) {
		foreach (GameObject o in s.GetRootGameObjects())
		{// RegionController child must be on root obj
			if (o.TryGetComponent<RegionController>(out currentRegion))
			{
				if (s.name != "City") {
					timesSinceVisitedCity++;
				} else {
					timesSinceVisitedCity = 0;
				}
				currentRegion.AssignRegion(s.name);
				currentRegion.Intro(visits[currentRegion.region]++);
				currentRegion.visits = visits[currentRegion.region];
				break;
			}
		}
	}

	IEnumerator UpdateOverworldValues()
	{
		isAnimating = true;
		float publicOpinion = (float)previousPublicOpinion * 100f;
		float econ = (float)previousEconomy * 100f;

		if ((float) World.averageTemp != thermometer.value) {
			UpdateThermometerValue();
			yield return new WaitForSeconds(1.5f);
		}
		if (!Mathf.Approximately(publicOpinion, World.publicOpinion) || previousEmissions != (float)EBM.F || econ != World.money)
		{
			statsPanel.Toggle(true);
		}
		yield return StartCoroutine(statsPanel.CallUpdate());
		statsPanel.Toggle(false);
		CheckAlerts();
		isAnimating = false;
		CheckGameOver();
	}

	// tween thermometer values
	void UpdateThermometerValue() {
		Transform thermometerTransform = thermometer.GetComponent<Transform>();
		float delay = 0.8f;
		if (previousTempValue > (float)World.averageTemp) {
			AudioManager.Instance.Play("SFX_SliderDown");
		} else if (previousTempValue < (float)World.averageTemp) {
			AudioManager.Instance.Play("SFX_SliderUp");
		}
		DOTween.Sequence()
			.Append(thermometerFill.DOColor(new Color32(173, 173, 173, 255), delay))
			.Join(thermometerBottom.DOColor(new Color32(173, 173, 173, 255), delay))
			.Join(thermometerTransform.DOScale(new Vector3(thermometerTransform.localScale.x + 0.5f, thermometerTransform.localScale.y + 0.5f, thermometerTransform.localScale.z), delay))
			.Append(thermometer.DOValue((float)World.averageTemp, 1.5f))
			.Append(thermometerFill.DOColor(Color.white, delay))
			.Join(thermometerBottom.DOColor(Color.white, delay))
			.Join(thermometerTransform.DOScale(new Vector3(thermometerTransform.localScale.x, thermometerTransform.localScale.y, thermometerTransform.localScale.z), delay));
		previousTempValue = (float) World.averageTemp;
	}

	public static void Transition(string scene) => instance.StartCoroutine(LoadScene(scene));

	static IEnumerator LoadScene(string name) {
		Cursor.visible = true;
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
		asyncLoad.allowSceneActivation = false;
		float start = Time.realtimeSinceStartup;

		Instance.loadingScreen.SetActive(true); // TODO: news scroll during loading screen

		while (!asyncLoad.isDone || GameManager.Instance.runningModel)
		{
			yield return null;

			if (asyncLoad.progress >= .9f && Time.realtimeSinceStartup - start > 1 && !GameManager.Instance.runningModel)
			{ // awaits both model and scene load
				Time.timeScale = 1;
				asyncLoad.allowSceneActivation = true;
				//AudioManager.Instance.StopMusic(); // could move earlier and play new music during newsscroll
				if (name == "Overworld")
					UIController.Instance.IncrementTurn();
				UIController.Instance.SetPrompt(false);
				Cursor.visible = true;
				yield break;
			}
		}
	}

	void SetSlider(Slider slider, float targetValue)
	{
		if (targetValue == 0)
		{
			slider.value = 17.07f;
		}
		else
		{
			slider.value = targetValue;
		}
	}

	void Bounce(Transform alert) {
		DOTween.Sequence()
			.Append(alert.DOMoveY(alert.position.y + 0.2f, 0.3f))
			.Append(alert.DOMoveY(alert.position.y - 0.2f, 0.3f));
	}

	void SetTropicsAlertOn() {
		tropicsAlert = GameObject.FindGameObjectWithTag("TropicsAlert");
		Transform tropicsTransform = tropicsAlert.GetComponent<Transform>();

		tropicsAlert.GetComponent<SpriteRenderer>().enabled = true;
		hasPlacedWorkers = false;
		tropicsIsAddressed = false;
		Bounce(tropicsTransform);
	}

	void SetFireAlertOn() {
		fireAlert = GameObject.FindGameObjectWithTag("FireAlert");
		Transform fireTransform = fireAlert.GetComponent<Transform>();

		fireAlert.GetComponent<SpriteRenderer>().enabled = true;
		Bounce(fireTransform);
		subtropicsIsAddressed = false;
	}

	void SetArcticAlertOn() {
		arcticAlert = GameObject.FindGameObjectWithTag("ArcticAlert");
		Transform arcticTransform = arcticAlert.GetComponent<Transform>();

		arcticAlert.GetComponent<SpriteRenderer>().enabled = true;
		Bounce(arcticTransform);
		arcticIsAddressed = false;
	}

	void CheckAlerts() {
		// Check if popular opinion has changed enough to influence the subtropics minigame
		tropicsAlert = GameObject.FindGameObjectWithTag("TropicsAlert");

		if (previousPublicOpinion < .20 && World.publicOpinion / 100 >= .20)
		{
			SetTropicsAlertOn();
		}
		else if (previousPublicOpinion >= .20 && previousPublicOpinion < .40 && (World.publicOpinion / 100 < .20 || World.publicOpinion / 100 >= .40))
		{
			SetTropicsAlertOn();
		}
		else if (previousPublicOpinion >= .40 && previousPublicOpinion < .60 && (World.publicOpinion / 100 < .40 || World.publicOpinion / 100 >= .60))
		{
			SetTropicsAlertOn();
		}
		else if (previousPublicOpinion >= .60 && previousPublicOpinion < .80 && (World.publicOpinion / 100 < .60 || World.publicOpinion / 100 >= .80))
		{
			SetTropicsAlertOn();
		}
		else if (previousPublicOpinion >= .80 && World.publicOpinion / 100 < .80)
		{
			SetTropicsAlertOn();
		}
		else if (!tropicsIsAddressed) {
			SetTropicsAlertOn();
		}
		else
		{
			tropicsAlert.GetComponent<SpriteRenderer>().enabled = false;
		}
		
		// Check if regional temp has gone up or down enough to change fire minigame
		double currentRegionalTemp = World.temp[1];
		fireAlert = GameObject.FindGameObjectWithTag("FireAlert");
		if (previousRegionalTemp < 24 && currentRegionalTemp >= 24)
		{
			SetFireAlertOn();
		}
		else if (previousRegionalTemp >= 24 && previousRegionalTemp < 25 && (currentRegionalTemp >= 25 || currentRegionalTemp < 24))
		{
			SetFireAlertOn();
		}
		else if (previousRegionalTemp >= 25 && previousRegionalTemp < 27 && (currentRegionalTemp >= 27 || currentRegionalTemp < 25))
		{
			SetFireAlertOn();
		}
		else if (previousRegionalTemp >= 27 && previousRegionalTemp < 29 && (currentRegionalTemp >= 29 || currentRegionalTemp < 27))
		{
			SetFireAlertOn();
		}
		else if (previousRegionalTemp >= 29 && currentRegionalTemp < 29)
		{
			SetFireAlertOn();
		}
		else if (!subtropicsIsAddressed)
		{
			SetFireAlertOn();
		}
		else {
			fireAlert.GetComponent<SpriteRenderer>().enabled = false;
		}
		previousRegionalTemp = currentRegionalTemp;

		// Check if arctic temp has changed enough
		double currentArcticTemp = World.temp[2];
		arcticAlert = GameObject.FindGameObjectWithTag("ArcticAlert");
		if (previousArcticTemp < -6 && currentArcticTemp >= -6)
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTemp >= -6 && previousArcticTemp < -1 && (currentArcticTemp >= -1 || currentArcticTemp < -6))
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTemp >= -1 && previousArcticTemp < 4 && (currentArcticTemp >= 4 || currentArcticTemp < -1))
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTemp >= 4 && previousArcticTemp < 9 && (currentArcticTemp >= 9 || currentArcticTemp < 4))
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTemp >= 9 && currentArcticTemp < 9)
		{
			SetArcticAlertOn();
		}
		else if (!arcticIsAddressed)
		{
			SetArcticAlertOn();
		}
		else
		{
			arcticAlert.GetComponent<SpriteRenderer>().enabled = false;
		}
		previousArcticTemp = currentArcticTemp;

		// Check if it's been too long since visited city 
		cityAlert = GameObject.FindGameObjectWithTag("CityAlert");
		Transform cityTransform = cityAlert.GetComponent<Transform>();
		if (timesSinceVisitedCity > 4 && billIndices.Count < 13)
		{
			cityAlert.GetComponent<SpriteRenderer>().enabled = true;
			Bounce(cityTransform);
		}
		else {
			cityAlert.GetComponent<SpriteRenderer>().enabled = false;
		}
	}

	public void IncrementCompletedRegions() {
		completedRegions++;
	}

	void CheckGameOver() {
		if (completedRegions > 19)
		{
			// show stats screen and let player restart
			UIController.Instance.ChangeGameOverPromptState(true);
			UIController.Instance.UpdateGameOverScreen();
			if (worldNameText.text != "")
			{
				worldNameText.text = World.worldName;
				worldNameText2.text = World.worldName;
			}
			else {
				worldNameText.text = "Your Planet";
				worldNameText2.text = "Your Planet";
			}
			
			completedRegions = 0; //reset # of completed regions
			World.turn = 1;
			timesSinceVisitedCity = 0;
			billIndices = new List<int>();
			EBM.F = 2;
			World.money = 70f; 
			World.publicOpinion = 70f;
			//visits = new Dictionary<World.Region, int> { { World.Region.Arctic, 0 }, { World.Region.Fire, 0 }, { World.Region.Forest, 0 }, { World.Region.City, 0 } };
		}
	}
}
