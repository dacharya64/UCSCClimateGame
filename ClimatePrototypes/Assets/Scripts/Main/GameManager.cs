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
	double previousSubtropicsTempDifference;
	double previousArcticTempDifference;
	int timesSinceVisitedCity = 0;
	public int completedRegions = 0;
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
	public bool menuUp = true;

	public Sprite planet1;
	public Sprite planet2;
	public Sprite planet3;
	public Sprite planet4;

	public LineRenderer resultLine; 

	public bool inOverworld;

	public static bool gameOver = false;

	public static GameObject turnCounter;

	public override void Awake() {
		base.Awake();
		if (Instance.runModel && World.averageTemp == 0) {
			World.Init();
			// World.ranges = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<double, List<double>>>>(Resources.Load<TextAsset>("ipcc").text);
		}
	}

	public void Start() {
		inOverworld = true;
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
		previousSubtropicsTempDifference = 0; 
		previousArcticTempDifference = 0;
		InitStats();
		SetThermometerValue();
	}

	public void Update() {
		if (Application.platform != RuntimePlatform.WebGLPlayer) {
			if (Input.GetKeyDown("escape")) {
				QuitGame(0);
			}
		}
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
		turnCounter.SetActive(true);
		gameOver = false;
		EBM.F = 0;
		TitleScreen.isFirstTime = false;
		UIController.Instance.ChangeGameOverPromptState(false);
		SceneManager.LoadScene("TitleScreen");
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
			//for (int i = 0; i < 20; i++) { //FOR DEBUGGING ONLY
				if (!hasPlacedWorkers)
				{
					forcingIncrease = 0.6; // originally 0.6
				}
				EBM.F = EBM.F + forcingIncrease;
				World.ChangeAverageTemp();
			//}
		}

		if (to.name == "Overworld")
		{
			Canvas navBarCanvas = UIController.Instance.GetComponent<Canvas>();
			navBarCanvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() as Camera;
			inOverworld = true;
			Transform nodes = GameObject.FindWithTag("Nodes").GetComponent<Transform>();
			Transform nodesHot = GameObject.FindWithTag("NodesHot").GetComponent<Transform>();
			var tempDifference = World.averageTemp - World.globalStartingTemp;
			// Set the alternate art for regions if it is too hot
			if (tempDifference < 7)
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
			if (tempDifference > 3 && tempDifference < 5)
			{
				world.GetComponent<SpriteRenderer>().sprite = planet2;
			}
			else if (tempDifference >= 5 && tempDifference < 7)
			{
				world.GetComponent<SpriteRenderer>().sprite = planet3;
			}
			else if (tempDifference >= 7)
			{
				world.GetComponent<SpriteRenderer>().sprite = planet4;
			}
			else {
				world.GetComponent<SpriteRenderer>().sprite = planet1;
			}

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
		if (to.name != "Overworld") {
			inOverworld = false;
			FindCurrentRegion(to);
		}
			
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
		CheckGameOver();
		if (!gameOver) {
			isAnimating = true;
			float publicOpinion = (float)previousPublicOpinion * 100f;
			float econ = (float)previousEconomy * 100f;

			if ((float)World.averageTemp != thermometer.value)
			{
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
		}
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
				//if (name == "Overworld")
					//UIController.Instance.IncrementTurn();
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
		double currentSubtropicsTemp = World.temp[1];
		double subtropicsTempDifference = currentSubtropicsTemp - World.subtropicsStartingTemp;
		fireAlert = GameObject.FindGameObjectWithTag("FireAlert");
		if (previousSubtropicsTempDifference < -1 && subtropicsTempDifference >= -1)
		{
			SetFireAlertOn();
		}
		else if (previousSubtropicsTempDifference >= -1 && previousSubtropicsTempDifference < 1 && (subtropicsTempDifference >= 1 || subtropicsTempDifference < -1))
		{
			SetFireAlertOn();
		}
		else if (previousSubtropicsTempDifference >= 1 && previousSubtropicsTempDifference < 3 && (subtropicsTempDifference >= 3 || subtropicsTempDifference < 1))
		{
			SetFireAlertOn();
		}
		else if (previousSubtropicsTempDifference >= 3 && previousSubtropicsTempDifference < 5 && (subtropicsTempDifference >= 5 || subtropicsTempDifference < 3))
		{
			SetFireAlertOn();
		}
		else if (previousSubtropicsTempDifference >= 5 && subtropicsTempDifference < 5)
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
		previousSubtropicsTempDifference = subtropicsTempDifference;

		// Check if arctic temp has changed enough
		double currentArcticTemp = World.temp[2];
		double currentArcticTempDifference = currentArcticTemp - World.arcticStartingTemp;
		arcticAlert = GameObject.FindGameObjectWithTag("ArcticAlert");
		if (previousArcticTempDifference < -1 && currentArcticTempDifference >= -1)
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTempDifference >= -1 && previousArcticTempDifference < 3 && (currentArcticTempDifference >= 3 || currentArcticTempDifference < -1))
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTempDifference >= -3 && previousArcticTempDifference < 7 && (currentArcticTempDifference >= 7 || currentArcticTempDifference < -3))
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTempDifference >= 7 && previousArcticTempDifference < 11 && (currentArcticTempDifference >= 11 || currentArcticTempDifference < 7))
		{
			SetArcticAlertOn();
		}
		else if (previousArcticTempDifference >= 11 && currentArcticTempDifference < 11)
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
		previousArcticTempDifference = currentArcticTempDifference;

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
		if (completedRegions > 19) // CHANGE THIS FOR DEBUGGING, originally set to 19
		{
			turnCounter = GameObject.FindWithTag("TurnCounter");
			turnCounter.SetActive(false);
			gameOver = true;
			// show stats screen and let player restart
			UIController.Instance.ChangeGameOverPromptState(true);
			UIController.Instance.UpdateGameOverScreen();
			if (World.worldName != "")
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
			CalculateGraph();
			//visits = new Dictionary<World.Region, int> { { World.Region.Arctic, 0 }, { World.Region.Fire, 0 }, { World.Region.Forest, 0 }, { World.Region.City, 0 } };
		}
	}

	void CalculateGraph() {
		float counter = -0.65f;
        for (int i = 0; i < 12; i++)
        {
			double temp_difference = World.current_temp_list[i] - World.starting_temp_list[i];
            resultLine.SetPosition(i, new Vector3(counter, (((float)temp_difference * 0.23f)/2) - 1.95f, 0.0f));
			counter = counter + 0.4f;
			temp_difference = temp_difference + 1;
		}
    }
}
