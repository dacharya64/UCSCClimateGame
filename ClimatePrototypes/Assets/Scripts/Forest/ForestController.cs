using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Pathfinding;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ForestController : RegionController {
	public static ForestController Instance { get => instance as ForestController; } // static instance

	[SerializeField] GameObject volunteerPrefab = default, uiPanel = default;
	[SerializeField] GameObject volunteer2Prefab;
	[SerializeField] GameObject volunteer3Prefab;
	[HideInInspector] public VolunteerUI selected;
	public int numActive;
	[HideInInspector] public bool overUI = false;
	public bool hasSelected { get => selected != null && !overUI; }
	public int volunteersPlaced;
	public int maxVolunteers;
	// for showing the results of changing the model
	public double forcingIncrease = 0.6;

	public GameObject aboutPrompt;
	public Text aboutText;
	public Text titleText;

	[HideInInspector] public Transform agentParent, utility;
	public List<VolunteerTask> volunteers = new List<VolunteerTask>();
	public List<GameObject> volunteerAgents = new List<GameObject>();
	public List<Vector3Int> activeTiles { get => volunteers.Where(v => v.activeTile != null).Select(v => v.activeTile.Value).ToList(); }
	public List<Vector3Int> activeTrees = new List<Vector3Int>();

	[SerializeField] Slider emissionsTracker = default;

	public Image totalArrow1;
	public Image totalArrow2;
	public Image totalArrow3;
	public Image totalArrow4;
	public Image totalArrow5;
	public Image totalArrow6;

	public Sprite arrowOutline;
	public Sprite arrowFull;

	public static double effect;

	public GameObject tutorial1;
	public GameObject tutorial2;

	public void UIHover(bool over) => overUI = over;

	public float inputTimer = 0;
	public bool counting = false;

	void Start() {
		// Tell the UI controller where the about prompt is
		UIController.Instance.timed = false;
		UIController.Instance.aboutPrompt = aboutPrompt;

		forcingIncrease = 0.6;
		agentParent = new GameObject("Agent Parent").transform;
		agentParent.parent = transform;
		utility = new GameObject("Utility").transform;
		utility.parent = transform;
		volunteersPlaced = 0;
		// Set active and max number of workers based on popular opinion
		if (World.publicOpinion < 20)
		{
			numActive = 1;
			maxVolunteers = 1;
		}
		else if (World.publicOpinion >= 20 && World.publicOpinion < 40)
		{
			numActive = 2;
			maxVolunteers = 2;
		}
		else if (World.publicOpinion >= 40 && World.publicOpinion < 60)
		{
			numActive = 3;
			maxVolunteers = 3;
		}
		else if (World.publicOpinion >= 60 && World.publicOpinion < 80)
		{
			numActive = 4;
			maxVolunteers = 4;
		}
		else {
			numActive = 5;
			maxVolunteers = 5;
		}
		uiPanel.GetComponentsInChildren<VolunteerUI>().Skip(numActive).ToList().ForEach(v => v.Deactivate());
	}

	public void IntroInit() {
		Init();
	}

	public void Init() {
		counting = true;
	}

	protected override void Update() {
		// add to a timer to determine when to open the tutorial
		if (counting)
			inputTimer += Time.deltaTime;

		// if player does nothing, show the tutorial
		if (inputTimer >= 10f)
		{
			inputTimer = 0;
			tutorial1.SetActive(true);
		}

		base.Update();
		
		if (forcingIncrease < 0.6)
		{
			totalArrow6.GetComponent<Image>().sprite = arrowOutline;
		}
		else {
			totalArrow6.GetComponent<Image>().sprite = arrowFull;
		}
		if (forcingIncrease < 0.5)
		{
			totalArrow5.GetComponent<Image>().sprite = arrowOutline;
		}
		else {
			totalArrow5.GetComponent<Image>().sprite = arrowFull;
		}
		if (forcingIncrease < 0.4)
		{
			totalArrow4.GetComponent<Image>().sprite = arrowOutline;
		}
		else {
			totalArrow4.GetComponent<Image>().sprite = arrowFull;
		}
		if (forcingIncrease < 0.3)
		{
			totalArrow3.GetComponent<Image>().sprite = arrowOutline;
		} else {
			totalArrow3.GetComponent<Image>().sprite = arrowFull;
		}
		if (forcingIncrease < 0.2)
		{
			totalArrow2.GetComponent<Image>().sprite = arrowOutline;
		}
		else {
			totalArrow2.GetComponent<Image>().sprite = arrowFull;
		}
		if (forcingIncrease < 0.1)
		{
			totalArrow1.GetComponent<Image>().sprite = arrowOutline;
		}
		else {
			totalArrow1.GetComponent<Image>().sprite = arrowFull;
		}
	}

	protected override void GameOver() {
		base.GameOver();
		StopAllCoroutines();
		//TriggerUpdate(() => World.co2.Update(region, delta: effect));
		GameManager.Instance.forcingIncrease = forcingIncrease;
		//TriggerUpdate(() => World.co2.Update(region, delta : effect * 1.18)); // [-1.18, 1.18]
		GameManager.Instance.hasPlacedWorkers = true;
	}

	/// <summary> Generates new volunteer / logger on click </summary>
	public PathfindingAgent NewAgent(GameObject prefab, Vector3 pos, Vector3 target) {
		var newAgent = GameObject.Instantiate(prefab, pos, Quaternion.identity, agentParent).GetComponent<PathfindingAgent>();
		newAgent.transform.position = new Vector3(newAgent.transform.position.x, newAgent.transform.position.y, 0);
		newAgent.gameObject.SetActive(true);
		newAgent.AssignTarget(target);
		return newAgent;
	}

	/// <summary> Creates volunteer and applies path target </summary>
	public void SetVolunteerTarget(Vector3 pos, UnityAction<Volunteer> onReached) {
		var newVolunteer =  new Volunteer();
		var value = Random.value;
		if (value < 0.33)
		{
			newVolunteer = NewAgent(volunteer3Prefab, Camera.main.ScreenToWorldPoint(selected.transform.position), pos) as Volunteer;
		}
		else if (value >= 0.33 && Random.value < 0.66)
		{
			newVolunteer = NewAgent(volunteer2Prefab, Camera.main.ScreenToWorldPoint(selected.transform.position), pos) as Volunteer;
		}
		else {
			newVolunteer = NewAgent(volunteerPrefab, Camera.main.ScreenToWorldPoint(selected.transform.position), pos) as Volunteer;
		}
		volunteerAgents.Add(newVolunteer.gameObject);
		if (Random.value > 0.5)
		{
			AudioManager.Instance.Play("SFX_Citizen_Acknowledgement");
		}
		else {
			AudioManager.Instance.Play("SFX_Citizen_Ok");
		}
			
		newVolunteer.ID = volunteers.Count;
		newVolunteer.name += $" {newVolunteer.ID}";

		volunteers.Add(new VolunteerTask(newVolunteer, selected, onReached, pos : pos));
		// selected.gameObject.SetActive(false);
		selected.AssignBubble(onReached);
		selected = null;

		newVolunteer.OnReached.AddListener((PathfindingAgent agent) => {
			onReached.Invoke(agent as Volunteer);
		});
		newVolunteer.OnReturn.AddListener(() => {
			volunteers[newVolunteer.ID]?.UI.Reset();
			volunteers.RemoveAt(newVolunteer.ID);
		});
	}

	/// <summary> Vector3Int overload (for ForestGrid) </summary>
	public void SetVolunteerTarget(Vector3Int pos, UnityAction<Volunteer> onReached) {
		SetVolunteerTarget((Vector3) pos, onReached);
		volunteers[volunteers.Count - 1].activeTile = pos;
	}

	public void EndGame()
	{
			GameOver();
	}

    public void OpenAbout()
	{
		aboutPrompt.SetActive(true);
	}

	public void CloseAbout()
	{
		aboutPrompt.SetActive(false);
	}

	public void SetAboutText(string area) {
		if (area == "factory")
		{
			aboutText.text = "This is the factory. Volunteers here will protest, which will lower the rate at which emissions increases over time.";
		}
		else if (area == "forest") {
			aboutText.text = "This is the area being deforested. Volunteers placed here will help plant trees. This will reduce emissions slightly.";
		} else {
			aboutText.text = "This is the carbon sequestration plant, which captures and stores carbon dioxide. Volunteers placed here will reduce emissions significantly, but this costs money.";
		}
	}

	public void SetTitleText(string area) {
		if (area == "factory")
		{
			titleText.text = "Protest Emissions";
		}
		else if (area == "forest")
		{
			titleText.text = "Plant Trees";
		}
		else
		{
			titleText.text = "Capture Carbon";
		}
	}

	public void ResetAllWorkers() {
		if (volunteers.Count > 0) {
			for (int i = 0; i < volunteers.Count; i++)
			{
				volunteers[i]?.UI.Reset();
			}
			for (int i = 0; i < volunteerAgents.Count; i++) {
				Destroy(volunteerAgents[i]);
			}
			volunteers.Clear();
			volunteerAgents.Clear();
			volunteersPlaced = 0;
			Factory.counter = 0;
			Station.counter = 0;
			
			// reset values changed by placing workers
			forcingIncrease = 0.6;
		}
	}
}

// [System.Serializable]
public class VolunteerTask {
	public Volunteer volunteer;
	public VolunteerUI UI;
	public Vector3Int? activeTile;
	public Vector3 target;
	public UnityAction<Volunteer> action;
	public VolunteerTask(Volunteer v, VolunteerUI vUI, UnityAction<Volunteer> vAction, Vector3Int? tile = null, Vector3 pos = default) => (volunteer, UI, action, activeTile, target) = (v, vUI, vAction, tile, pos);
}

