using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StatsPanel : MonoBehaviour {
	[SerializeField] Slider landUse = default, publicOpinion = default, emissions = default, economy = default;
	public float previousPublicOpinion;
	public float previousEmissions = 2;
	public float previousEconomy;
	public float previousLandUse;

	public void InitializeValues() {
		SetSlider(landUse, (float) EBM.a0);
		SetSlider(publicOpinion, World.publicOpinion / 100, invertColors : true);
		SetSlider(emissions, (float) EBM.F); // used to be /14
		previousEmissions = (float) EBM.F;
		SetSlider(economy, World.money / 100);
	}

	public IEnumerator CallUpdate() {
		float newPublicOpinion = (float) previousPublicOpinion * 100f;
		float newEcon = (float) previousEconomy * 100f;
		if (!Mathf.Approximately(newPublicOpinion, World.publicOpinion))
		{
			previousPublicOpinion = UpdateSlider(publicOpinion, World.publicOpinion / 100, previousPublicOpinion, invertColors: true);
			yield return new WaitForSeconds(1.5f);
		}
		
		if (previousEmissions != (float)EBM.F)
		{
			previousEmissions = UpdateSlider(emissions, (float)EBM.F, previousEmissions); // used to be /14
			yield return new WaitForSeconds(1.5f);
		}
		
		if (!Mathf.Approximately(newEcon, World.money))
		{
			previousEconomy = UpdateSlider(economy, World.money / 100, previousEconomy);
			yield return new WaitForSeconds(1.5f);
		}
		yield return new WaitForSeconds(1f);
	}

	float UpdateSlider(Slider slider, float value, float previousValue, bool invertColors = false) {
		Transform sliderTransform = slider.GetComponent<Transform>();
		float delay = 1f;
		if (previousValue > value)
		{
			AudioManager.Instance.Play("SFX_SliderDown");
		}
		else if (previousValue < value)
		{
			AudioManager.Instance.Play("SFX_SliderUp");
		}

		slider.value = previousValue;
		Color originalColor = slider.fillRect.GetComponentInChildren<Image>(true).color;
		Color newColor = new Color(originalColor.r - 0.4f, originalColor.g - 0.4f, originalColor.b - 0.4f, 1f);
		DOTween.Sequence()
			.Append(slider.fillRect.GetComponentInChildren<Image>(true).DOColor(newColor, delay))
			.Join(sliderTransform.DOScale(new Vector3(sliderTransform.localScale.x + 0.2f, sliderTransform.localScale.y + 0.2f, sliderTransform.localScale.z), delay))
			.Append(slider.DOValue(value, 1.8f))
			.Append(slider.fillRect.GetComponentInChildren<Image>(true).DOColor(originalColor, delay))
			.Join(sliderTransform.DOScale(new Vector3(sliderTransform.localScale.x, sliderTransform.localScale.y, sliderTransform.localScale.z), delay));
		slider.fillRect.GetComponentInChildren<Image>(true).color = invertColors ? Color.Lerp(Color.red, Color.green, value) : Color.Lerp(Color.green, Color.red, value);
		return value;
	}

	void SetSlider(Slider slider, float value, bool invertColors = false)
	{
		slider.value = value;
		slider.fillRect.GetComponentInChildren<Image>(true).color = invertColors ? Color.Lerp(Color.red, Color.green, value) : Color.Lerp(Color.green, Color.red, value);
	}

	public void Toggle() => gameObject.SetActive(!gameObject.activeSelf);
	public void Toggle(bool status) => gameObject.SetActive(status);
}
