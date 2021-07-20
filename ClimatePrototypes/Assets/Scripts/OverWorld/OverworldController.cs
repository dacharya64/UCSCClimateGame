using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class OverworldController : MonoBehaviour {
	[SerializeField] GameObject worldWrapper = default;
	[SerializeField] Transform moon = default;
	Transform world;
	[SerializeField] SpriteRenderer bg = default;
	[HideInInspector] public Material fadeMat;
	public AnimationCurve animationCurve;
	public float fadingSpeed = 5f;
	[SerializeField] Canvas canvas;
	private CanvasGroup canvasGroup;
	

	public enum Direction { FadeIn, FadeOut };

	void Awake() {
		if (canvas == null) canvas = GetComponent<Canvas>();
		canvasGroup = canvas.GetComponent<CanvasGroup>();
		if (canvasGroup == null) Debug.LogError("Please assign a canvas group to the canvas!");

	}

	void Start() {
		fadeMat = new Material(Shader.Find("Screen/Fade"));
		world = worldWrapper.transform.GetChild(0);
		if (canvas == null) canvas = GetComponent<Canvas>();
		canvasGroup = canvas.GetComponent<CanvasGroup>();
		if (canvasGroup == null) Debug.LogError("Please assign a canvas group to the canvas!");

		if (animationCurve.length == 0)
		{
			animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}

		double targetValue = World.averageTemp;
		Debug.Log("target value is: " + targetValue);
		//thermometer.value = Mathf.Lerp(thermometer.value, (float) targetValue, Time.deltaTime * 8f);
		
		StartCoroutine(RotateMoon());
	}

	IEnumerator RotateMoon() {
		float moonDist = (world.position - moon.position).magnitude;
		float alpha = Vector2.Angle(Vector2.right, (Vector2)(moon.position - world.position)) * Mathf.Deg2Rad;
		SpriteRenderer moonSprite = moon.GetComponent<SpriteRenderer>();
		UnityEngine.SceneManagement.Scene overworldScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

		while (overworldScene.isLoaded) {
			yield return null;
			float step = Time.time / 2;
			float x = moonDist * Mathf.Cos(step) * Mathf.Cos(alpha) - moonDist / 2 * Mathf.Sin(step) * Mathf.Sin(alpha);
			float y = moonDist * Mathf.Cos(step) * Mathf.Sin(alpha) + moonDist / 2 * Mathf.Sin(step) * Mathf.Cos(alpha);

			moon.transform.position = new Vector2(x, y) + (Vector2)world.position;
			moon.transform.eulerAngles = Vector3.forward * Mathf.Sin(step) * Mathf.Rad2Deg;
			moonSprite.sortingOrder = Mathf.Sin(step) > 0 ? 0 : 2;
		}
	}

	/// <summary> used to load world in from title screen </summary>
	public void SendToBottom() {
		Camera.main.transform.position = Vector3.forward * -10;
		bg.transform.position = new Vector3(bg.transform.position.x, -Camera.main.ViewportToWorldPoint(Vector2.zero).y - bg.bounds.extents.y, bg.transform.position.z); // TODO: global variable class
		Camera.main.transform.position = new Vector3(0, bg.bounds.min.y - Camera.main.ViewportToWorldPoint(Vector2.zero).y, -10); // TODO: global variable class
	}

	public void ClearWorld() {
		foreach (SpriteRenderer sr in worldWrapper.GetComponentsInChildren<SpriteRenderer>())
			sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
	}

	public IEnumerator EnterWorld(float time = 1) {
		StartCoroutine(FadeCanvas(canvasGroup, Direction.FadeIn, fadingSpeed));
		ClearWorld();
		SpriteRenderer[] sprites = worldWrapper.GetComponentsInChildren<SpriteRenderer>();
		for (var (start, step) = (Time.time, 0f); step < time; step = Time.time - start) {
			yield return null;
			foreach (var sr in sprites)
				sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, step);
		}
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest) {
		Graphics.Blit(src, dest, fadeMat);
	}

	public void HideThermometer() {
		canvasGroup.alpha = 0;
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

	void UpdateSlider(Slider slider, float targetValue)
	{
		//slider.value = targetValue;
		slider.value = Mathf.Lerp(slider.value, (float)targetValue, Time.deltaTime * 24f);
	}
}
