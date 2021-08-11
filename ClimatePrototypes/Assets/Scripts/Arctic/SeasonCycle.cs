using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class SeasonCycle : MonoBehaviour {
	SpriteRenderer summerSR, winterSR;
	public bool isSummer = true;
	float dayDuration = 30f;
	System.Func<float, float, float> fadeIn, fadeOut;
	[SerializeField] float transitionTime = 1.5f;
	[SerializeField] Text SeasonText;

	void Start() {
		
		//isSummer = ArcticController.Instance.visits % 2 == 0;
		isSummer = true;
		var srs = GetComponentsInChildren<SpriteRenderer>();
		(summerSR, winterSR) = (srs[0], srs[1]);

		// init fade funcs
		fadeIn = (float step, float dur) => EaseMethods.CubicEaseOut(step, 0, 1, dur);
		fadeOut = (float step, float dur) => EaseMethods.CubicEaseIn(dur - step, 0, 1, dur);

		StartCoroutine(ChangeSeasons());
	}

	/// <summary> Controls seasonal cycle. </summary>
	IEnumerator ChangeSeasons(float duration = .5f) {
		for (float timer = duration; timer > dayDuration - transitionTime; timer -= Time.deltaTime) {
			yield return null;
			float step = timer - (dayDuration - transitionTime);
			summerSR.color = new Color(1, 1, 1, isSummer ? fadeOut(step, transitionTime) : fadeIn(step, transitionTime));
			winterSR.color = new Color(1, 1, 1, isSummer ? fadeIn(step, transitionTime) : fadeOut(step, transitionTime));
		}
		ArcticController.Instance.buffers.ToList().ForEach(b => b.AssignSprite());
		yield return new WaitForSeconds(duration);
		isSummer = !isSummer;
		if (isSummer)
		{
			SeasonText.text = "Summer";
		}
		else
		{
			SeasonText.text = "Winter";
		}
		StartCoroutine(ChangeSeasons(dayDuration));
	}
}
