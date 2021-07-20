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
	public float previousValue;
	[SerializeField] StatsPanel statsPanel;
	[SerializeField] GameObject stats;

	public RegionController currentRegion;
	Dictionary<World.Region, int> visits = new Dictionary<World.Region, int> { { World.Region.Arctic, 0 }, { World.Region.Fire, 0 }, { World.Region.Forest, 0 }, { World.Region.City, 0 } };

	public override void Awake() {
		base.Awake();
		if (Instance.runModel && World.averageTemp == 0) {
			World.Init();
			// World.ranges = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<double, List<double>>>>(Resources.Load<TextAsset>("ipcc").text);
		}
	}

	void Start() {
		FindCurrentRegion(SceneManager.GetActiveScene());
		SceneManager.activeSceneChanged += instance.InitScene;
		thermometer = GameObject.FindGameObjectWithTag("Slider").GetComponent<Slider>();
		thermometer.value = previousValue;
		SetSlider(thermometer, (float) World.averageTemp);
		previousValue = (float) World.averageTemp; 
	}

	public static void Restart() {
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
			statsPanel.Toggle(true);
			thermometer = GameObject.FindGameObjectWithTag("Slider").GetComponent<Slider>();
			AudioManager.Instance.Play("BGM_Menu"); // TODO: sound name variable class
			thermometer.value = previousValue;
			thermometer.DOValue((float) World.averageTemp, 1.5f);
			previousValue = (float) World.averageTemp; 
		}
		else
			AudioManager.Instance.Play("BGM_" + to.name); // TODO: sound name variable class
		if (to.name != "Overworld")
			FindCurrentRegion(to);
	}

	void FindCurrentRegion(Scene s) {
		foreach (GameObject o in s.GetRootGameObjects())
		{// RegionController child must be on root obj
			if (o.TryGetComponent<RegionController>(out currentRegion))
			{
				currentRegion.AssignRegion(s.name);
				currentRegion.Intro(visits[currentRegion.region]++);
				currentRegion.visits = visits[currentRegion.region];
				break;
			}
		}
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
		slider.value = targetValue;
		previousValue = targetValue;
	}
}
