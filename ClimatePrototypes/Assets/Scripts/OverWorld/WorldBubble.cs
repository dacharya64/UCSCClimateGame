using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class WorldBubble : MonoBehaviour {
	float size, minSize = .001f;
	public Color colour;
	GameObject bubble;
	bool active = false;
	Vector3 startPos;
	public Dictionary<string, SpriteRenderer> icons = new Dictionary<string, SpriteRenderer>();
	CircleCollider2D col;
	public Canvas canvas;
	public AnimationCurve animationCurve;
	public float fadingSpeed = 5f;
	private CanvasGroup canvasGroup;
	public GameObject cityPrompt;

	public enum Direction { FadeIn, FadeOut };

	void Awake() {
		col = GetComponent<CircleCollider2D>();
		bubble = transform.GetChild(0).gameObject;
		size = bubble.transform.localScale.x;
		startPos = bubble.transform.localPosition;
		bubble.transform.localScale = Vector3.one * minSize;
		bubble.transform.Find("Base").GetComponent<SpriteRenderer>().color = colour; // TODO: never use Find
		foreach (SpriteRenderer icon in transform.Find("Icons").GetComponentsInChildren<SpriteRenderer>()) { // TODO: never use Find
			icons.Add(icon.name.Replace("Icon", string.Empty), icon);
			icon.gameObject.SetActive(false);
		}
	}

	void Start() {
		if (canvas == null) canvas = GetComponent<Canvas>();
		canvasGroup = canvas.GetComponent<CanvasGroup>();
		if (canvasGroup == null) Debug.LogError("Please assign a canvas group to the canvas!");

		if (animationCurve.length == 0)
		{
			animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
	}

	void OnMouseEnter() {
		if (!active)
			StartCoroutine(Bubble(entering: true, dur: .25f));
	}

	void OnMouseOver() {
		if (!active)
			StartCoroutine(Bubble(entering: true, dur: .25f));
		if (Input.GetButtonDown("Fire1")) {
			// Check if the node is the city node and if there are still bills available
			if (this.gameObject.name == "CityNode" && GameManager.Instance.billIndices.Count >= 13) // Change this value if # of bills goes up
			{
				UIController.Instance.ChangeCityPromptState(true);
			}
			else {
				foreach (var node in transform.parent.GetComponentsInChildren<WorldBubble>())
					foreach (var kvp in node.icons)
						kvp.Value.gameObject.SetActive(false);
				StartCoroutine(EnterRegion(new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0)));
			}
		}
	}

	void OnMouseExit() {
		if (active)
			StartCoroutine(Bubble(entering: false, dur: .25f));
	}

	/// <summary> expands and collapses bubble </summary>
	IEnumerator Bubble(bool entering, float dur, Vector2 pos = default(Vector2)) {
		active = entering;
		if (entering)
			bubble.SetActive(true);
		for (var (start, step) = (Time.time, 0f); step < dur; step = Time.time - start) {
			yield return null;
			float scale = entering ?
				EaseMethods.CubicEaseOut(step / dur, minSize, size, size) :
				EaseMethods.CubicEaseIn(1 - step / dur, minSize, size, size);
			bubble.transform.localScale = scale * Vector3.one;
			col.radius = scale / size * 7 + 3;
			// bubble.transform.localPosition = entering ? Vector3.Lerp(transform.position, startPos, step / dur) : Vector3.Lerp(startPos, transform.position, step / dur);
		}
		if (!entering)
			bubble.SetActive(false);
	}

	IEnumerator EnterRegion(Vector3 bubblePos, float time = .5f) {
		StartCoroutine(FadeCanvas(canvasGroup, Direction.FadeOut, 0.05f));
		//StartCoroutine(UIController.SlideNav(UIController.Instance.navbar.transform, up : true));
		Vector3 camStartPos = Camera.main.transform.position;
		for (var (start, step) = (Time.time, 0f); step < time; step = Time.time - start) {
			yield return null;
			Camera.main.transform.position = Vector3.Lerp(camStartPos, bubblePos, step / time);
			Camera.main.orthographicSize = 5 * (1 - step / time); // slow
			Camera.main.GetComponent<OverworldController>().fadeMat.SetFloat("_Alpha", step / time); // slow
		}
		GameManager.Transition(name.Replace("Node", string.Empty));
		// Shader.SetGlobalFloat("_Alpha", 1);
	}

	public IEnumerator FadeCanvas(CanvasGroup canvasGroup, Direction direction, float duration)
	{
		// keep track of when the fading started, when it should finish, and how long it has been running
		var startTime = Time.time;
		var endTime = Time.time + duration;
		var elapsedTime = 0f;

		// set the canvas to the start alpha – this ensures that the canvas is ‘reset’ if you fade it multiple times
		if (direction == Direction.FadeIn) canvasGroup.alpha = animationCurve.Evaluate(0f);
		else canvasGroup.alpha = animationCurve.Evaluate(1f);

		// loop repeatedly until the previously calculated end time
		while (Time.time <= endTime)
		{
			elapsedTime = Time.time - startTime; // update the elapsed time
			var percentage = 1 / (duration / elapsedTime); // calculate how far along the timeline we are
			if ((direction == Direction.FadeOut)) // if we are fading out
			{
				canvasGroup.alpha = animationCurve.Evaluate(1f - percentage);
			}
			else // if we are fading in/up
			{
				canvasGroup.alpha = animationCurve.Evaluate(percentage);
			}

			yield return new WaitForEndOfFrame(); // wait for the next frame before continuing the loop
		}

		// force the alpha to the end alpha before finishing – this is here to mitigate any rounding errors, e.g. leaving the alpha at 0.01 instead of 0
		if (direction == Direction.FadeIn) canvasGroup.alpha = animationCurve.Evaluate(1f);
		else canvasGroup.alpha = animationCurve.Evaluate(0f);
	}
}
