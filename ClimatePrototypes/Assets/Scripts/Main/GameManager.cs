using System.Collections;
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

	public RegionController currentRegion;
	Dictionary<World.Region, int> visits = new Dictionary<World.Region, int> { { World.Region.Arctic, 0 }, { World.Region.Fire, 0 }, { World.Region.Forest, 0 }, { World.Region.City, 0 } };

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
		SetSlider(thermometer, (float) World.averageTemp);
		previousTempValue = (float) World.averageTemp;
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
		statsPanel = stats.GetComponent(typeof(StatsPanel)) as StatsPanel;
		statsPanel.InitializeValues();
	}

	public static void Restart() {
		UIController.Instance.ChangeGameOverPromptState(false);
		SceneManager.LoadScene("TitleScreen");
		EBM.Reset();
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
		if (to.name == "Overworld")
		{
			statsPanel = stats.GetComponent(typeof(StatsPanel)) as StatsPanel;
			// Save the previous stats
			previousPublicOpinion = statsPanel.previousPublicOpinion;
			previousEmissions = statsPanel.previousEmissions;
			previousEconomy = statsPanel.previousEconomy;
			previousLandUse = statsPanel.previousLandUse;

			// turn on stats panel
			// check to see if something in the stats panel changed 

			float publicOpinion = (float)previousPublicOpinion * 100f;
			float econ = (float)previousEconomy * 100f;
			if (!Mathf.Approximately(publicOpinion, World.publicOpinion) || previousEmissions != (float)EBM.F || econ != World.money) {
				statsPanel.Toggle(true);
			}

			thermometer = GameObject.FindGameObjectWithTag("Slider").GetComponent<Slider>();
			AudioManager.Instance.Play("BGM_Menu"); // TODO: sound name variable class

			thermometer.value = previousTempValue;

			StartCoroutine(UpdateOverworldValues());

			
			/*// tween thermometer values
			thermometer.value = previousTempValue;
			thermometer.DOValue((float)World.averageTemp, 1.5f);
			previousTempValue = (float)World.averageTemp;*/

			// tween stats panel values 
			//statsPanel.CallUpdate();

			// check if turn on alert
			//CheckAlerts();

			// check if game over
			CheckGameOver();
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
		bool yield = false;
		if ((float) World.averageTemp != thermometer.value) {
			yield = true;
		}
		UpdateThermometerValue();
		if (yield) {
			yield return new WaitForSeconds(1.5f);
		}
		StartCoroutine(statsPanel.CallUpdate());
		yield return new WaitForSeconds(1.5f);
		CheckAlerts();
	}

	// tween thermometer values
	void UpdateThermometerValue() {
		thermometer.DOValue((float)World.averageTemp, 1.5f);
		previousTempValue = (float)World.averageTemp;
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
				AudioManager.Instance.StopMusic(); // could move earlier and play new music during newsscroll
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
			slider.value = 15.7881585877727f;
		}
		else
		{
			slider.value = targetValue;
		}
	}

	void CheckAlerts() {
		// Check if popular opinion has changed enough to influence the subtropics minigame
		tropicsAlert = GameObject.FindGameObjectWithTag("TropicsAlert");
		if (previousPublicOpinion < .20 && World.publicOpinion >= .20)
		{
			tropicsAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousPublicOpinion >= .20 && previousPublicOpinion < .40 && (World.publicOpinion / 100 < .20 || World.publicOpinion / 100 >= .40))
		{
			tropicsAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousPublicOpinion >= .40 && previousPublicOpinion < .60 && (World.publicOpinion / 100 < .40 || World.publicOpinion / 100 >= .60))
		{
			tropicsAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousPublicOpinion >= .60 && previousPublicOpinion <.80 && (World.publicOpinion / 100 < .60 || World.publicOpinion / 100 >= .80))
		{
			tropicsAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousPublicOpinion >= .80 && World.publicOpinion / 100 < .80)
		{
			tropicsAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else {
			tropicsAlert.GetComponent<SpriteRenderer>().enabled = false;
		}

		
		// Check if regional temp has gone up or down enough to change tropics minigame
		fireAlert = GameObject.FindGameObjectWithTag("FireAlert");
		double currentRegionalTemp = World.temp[1];
		if (previousRegionalTemp < 20 && currentRegionalTemp >= 20)
		{
			fireAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousRegionalTemp >= 20 && previousRegionalTemp < 25 && (currentRegionalTemp >= 25 || currentRegionalTemp < 20))
		{
			fireAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousRegionalTemp >= 25 && previousRegionalTemp < 30 && (currentRegionalTemp >= 30 || currentRegionalTemp < 25))
		{
			fireAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousRegionalTemp >= 30 && previousRegionalTemp < 35 && (currentRegionalTemp >= 35 || currentRegionalTemp < 30))
		{
			fireAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousRegionalTemp >= 35 && currentRegionalTemp < 35)
		{
			fireAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else {
			fireAlert.GetComponent<SpriteRenderer>().enabled = false;
		}
		previousRegionalTemp = currentRegionalTemp;

		// Check if arctic temp has changed enough
		arcticAlert = GameObject.FindGameObjectWithTag("ArcticAlert");
		double currentArcticTemp = World.temp[2];
		if (previousArcticTemp < -10 && currentRegionalTemp >= -10)
		{
			arcticAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousArcticTemp >= -10 && previousArcticTemp < -5 && (currentArcticTemp >= -5 || currentArcticTemp < -10))
		{
			arcticAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousArcticTemp >= -5 && previousArcticTemp < 0 && (currentArcticTemp >= 0 || currentArcticTemp < -5))
		{
			arcticAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousArcticTemp >= 0 && previousArcticTemp < 5 && (currentArcticTemp >= 5 || currentArcticTemp < 0))
		{
			arcticAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else if (previousArcticTemp >= 5 && currentArcticTemp < 5)
		{
			arcticAlert.GetComponent<SpriteRenderer>().enabled = true;
		}
		else
		{
			arcticAlert.GetComponent<SpriteRenderer>().enabled = false;
		}
		previousArcticTemp = currentArcticTemp;

		// Check if it's been too long since visited city 
		cityAlert = GameObject.FindGameObjectWithTag("CityAlert");
		if (timesSinceVisitedCity > 4)
		{
			cityAlert.GetComponent<SpriteRenderer>().enabled = true;
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
			completedRegions = 0; //reset # of completed regions
			World.turn = 1;
			timesSinceVisitedCity = 0;
			visits = new Dictionary<World.Region, int> { { World.Region.Arctic, 0 }, { World.Region.Fire, 0 }, { World.Region.Forest, 0 }, { World.Region.City, 0 } };
		}
	}
}
