using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StatsPanel : MonoBehaviour {
	[SerializeField] Slider landUse = default, publicOpinion = default, emissions = default, economy = default;
	public float previousPublicOpinion;
	public float previousEmissions;
	public float previousEconomy;
	public float previousLandUse;

	void Start() {
		InitializeValues();
	}

	void InitializeValues() {
		SetSlider(landUse, (float) EBM.a0);
		SetSlider(publicOpinion, World.publicOpinion / 100, invertColors : true);
		SetSlider(emissions, (float) EBM.F); // used to be /14
		SetSlider(economy, World.money / 100);
	}

	public void CallUpdate() {
		previousLandUse = UpdateSlider(landUse, (float)EBM.a0, previousLandUse);
		previousPublicOpinion = UpdateSlider(publicOpinion, World.publicOpinion / 100, previousPublicOpinion, invertColors: true);
		previousEmissions = UpdateSlider(emissions, (float)EBM.F, previousEmissions); // used to be /14
		previousEconomy = UpdateSlider(economy, World.money / 100, previousEconomy);
	}

	float UpdateSlider(Slider slider, float value, float previousValue, bool invertColors = false) {
		slider.value = previousValue;
		slider.DOValue(value, 1.5f);
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
