using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController> {
	public Text worldNameText;
	[SerializeField] Text moneyText = default,
	turnText = default;
	[SerializeField] Button backButton = default,
	exitButton = default;
	[SerializeField] GameObject returnPrompt = default;
	[SerializeField] GameObject loadingPrompt = default;
	[SerializeField] GameObject infoBox = default;
	[SerializeField] GameObject cityPrompt = default;
	[SerializeField] GameObject outOfMoneyPrompt = default;
	[SerializeField] GameObject gameOverPrompt;
	public GameObject navbar;
	Dictionary<GameObject, bool> uiActiveStatus = new Dictionary<GameObject, bool>();
	public World.Region previousRegion;
	public bool firstTimeVisiting;
	public GameObject aboutPrompt;
	public bool timed = false;

	/// <summary> Flips active status of UI element and stores it </summary>
	public void Toggle(GameObject obj) {
		if (!uiActiveStatus.ContainsKey(obj))
			uiActiveStatus.Add(obj, !obj.activeSelf);
		else
			uiActiveStatus[obj] = !uiActiveStatus[obj];
		obj.SetActive(uiActiveStatus[obj]);
	}

	void OnEnable() {
		worldNameText.text = World.worldName;
		turnText.text = $"Year {World.turn}";
	}

	void Update() {
		//moneyText.text = $"{World.money:F2}"; // technically we don't use money anymore?
	}

	public void IncrementTurn() => turnText.text = $"Year {++World.turn}";

	public void ToggleBackButton(bool on) {
		backButton.gameObject.SetActive(on);
		//exitButton.gameObject.SetActive(!on);
	}

	// methods that start with UI are non-static methods for Unity Editor buttons
	public void UIQuitGame(int status) => GameManager.Instance.QuitGame(status);

	public void UIOpenSettings(int status) => GameManager.Instance.OpenSettings(status);

	public void UITransition(string level) {
		returnPrompt.SetActive(false);
		GameManager.Transition(level);
		/*if (previousRegion == World.Region.Fire && firstTimeVisiting)
		{
			ChangeInfoBoxState(true);
		}
		firstTimeVisiting = false;*/
		
	}

	/* Controls the text for the game over prompts */

	public void SetCityPrompt(bool status) {
		returnPrompt.GetComponentInChildren<Text>().text = "You have selected all of the bills!";
		returnPrompt.SetActive(status);
	}

	public void SetArcticPrompt(bool status)
	{
		returnPrompt.GetComponentInChildren<Text>().text = "Another year has passed.";
		returnPrompt.SetActive(status);
	}

	public void SetForestPrompt(bool status, double result) {
		string emissionsResult;
		if (result < 0.06)
		{
			emissionsResult = "Small";
		}
		else if (result < 0.12)
		{
			emissionsResult = "Moderate";
		}
		else
		{
			emissionsResult = "Large";
		}
		returnPrompt.GetComponentInChildren<Text>().text = "You have placed all of the volunteers! \n\nPrediced emissions increase: " + emissionsResult;
		returnPrompt.SetActive(status);
	}

	public void SetFirePrompt(bool status, double fires)
	{
		string publicOpinionResult;
		if (fires < 10)
		{
			publicOpinionResult = "Increases";
		}
		else if (fires < 20)
		{
			publicOpinionResult = "Decreases";
		}
		else {
			publicOpinionResult = "Greatly Decreases";
		}
		returnPrompt.GetComponentInChildren<Text>().text = "You have fought the fires!\n\nFires remaining: " + fires + "\n\nPublic opinion: " + publicOpinionResult;
		returnPrompt.SetActive(status);
	}

	public void SetPrompt(bool status)
	{
		returnPrompt.GetComponentInChildren<Text>().text = "You have completed this region!";
		returnPrompt.SetActive(status);
	}

		public void SetLoadingPrompt(bool status)
	{
		loadingPrompt.SetActive(status);
	}

	static IEnumerator WaitForRealSeconds(float seconds) {
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < seconds)
			yield return null;
	}

	public static IEnumerator Typewriter(TMPro.TextMeshProUGUI print, string text, string header, float delay = .05f) { //given text to print, text ref, and print speed, does typewriter effect
		if (text.Length == 0)
			yield break;
		if (print.text == "Title") {
			print.text = header;
			print.transform.position += print.preferredWidth * Vector3.right;
		}
		print.text = "";
		for (int i = 0; i < text.Length; i++) {
			print.text += text[i];
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) // mouse or space to skip
				print.text = text.Substring(0, (i = text.Length - 2));
			yield return WaitForRealSeconds(delay);
		}
	}
	public static IEnumerator Typewriter(Text print, string text, string header, float delay = .05f) { //given text to print, text ref, and print speed, does typewriter effect
		if (print.text == "Title") {
			print.text = text;
			print.transform.position += print.preferredWidth / 2 * Vector3.right;
		}
		print.text = "";
		for (int i = 0; i < text.Length; i++) {
			print.text += text[i];
			//if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
			//	print.text = text.Substring(0, (i = text.Length));
            yield return WaitForRealSeconds(delay);
		}

	}

	public static IEnumerator TypewriterClickToAdvance(Text print, string text, float delay = .05f)
	{ //given text to print, text ref, and print speed, does typewriter effect
		if (print.text == "Title")
		{
			print.text = text;
			print.transform.position += print.preferredWidth / 2 * Vector3.right;
		}
		print.text = "";
		for (int i = 0; i < text.Length; i++)
		{
			print.text += text[i];
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) {
				print.text = text.Substring(0, (i = text.Length));
				AudioManager.Instance.StopSFX();
			}
				

			yield return WaitForRealSeconds(delay);
		}
	}

	public static IEnumerator ClickToAdvance(Text text, string[] words, GameObject button = null) { 
		Text header = GameObject.FindWithTag("Title").GetComponent<Text>();
		header.text = words[0];
		var clickPrompt = text.GetComponentOnlyInChildren<Text>()?.gameObject;
		yield return instance.StartCoroutine(Typewriter(text, words[1], words[0]));

		if (words.Length <= 2)
		{
			clickPrompt.SetActive(false);
			if (button)
				button.SetActive(true);
		}
		else {
			clickPrompt.SetActive(true);
		}
		for (int i = 2; i < words.Length; i++) {
			yield return new WaitForMouseDown();
			clickPrompt.SetActive(false);
			yield return instance.StartCoroutine(Typewriter(text, words[i], words[0]));
			clickPrompt.SetActive(true);
			if (i == words.Length - 1) {
				clickPrompt.SetActive(false);
				if (button)
					button.SetActive(true);
			}
		}
	}

	public static IEnumerator SlideNav(Transform nav, bool up = false, float time = .5f) {
		float height = (nav.transform as RectTransform).rect.height;
		nav.transform.position = nav.transform.position + Vector3.up * height * (up ? 0 : 1);
		float startingHeight = nav.transform.position.y;

		for (var(start, step) = (Time.time, 0f); step < time; step = Time.time - start) {
			yield return null;
			nav.transform.position = new Vector3(nav.transform.position.x, startingHeight - step / time * height * (up ? -1 : 1), nav.transform.position.z);
		}
	}

	public static void SetUIAlpha(GameObject ui, float a) {
		foreach (var child in ui.GetComponentsInChildren<Graphic>())
			child.color = new Color(child.color.r, child.color.g, child.color.b, a);
	}

	public void ChangeInfoBoxState(bool state) {
		infoBox.SetActive(state);
	}

	public void ChangeOutOfMoneyPrompt(bool state)
	{
		outOfMoneyPrompt.SetActive(state);
	}
	public void ChangeCityPromptState(bool state)
	{
		cityPrompt.SetActive(state);
	}

	public void ChangeGameOverPromptState(bool state)
	{
		//gameOverPrompt = GameObject.FindGameObjectWithTag("GameOverPrompt");
		gameOverPrompt.SetActive(state);
	}

	public void RestartGame() {
		GameManager.Restart();
	}

	public void OpenAboutBox() {
        if (aboutPrompt != null)
        {
			aboutPrompt.SetActive(true);
			if (timed) {
				Time.timeScale = 0;
			}
        }
    }
}
