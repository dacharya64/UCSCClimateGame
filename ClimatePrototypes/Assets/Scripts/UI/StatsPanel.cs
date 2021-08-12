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

	public void InitializeValues() {
		SetSlider(landUse, (float) EBM.a0);
		SetSlider(publicOpinion, World.publicOpinion / 100, invertColors : true);
		SetSlider(emissions, (float) EBM.F); // used to be /14
		SetSlider(economy, World.money / 100);
	}

	public IEnumerator CallUpdate() {
		bool yield = false;
		if (previousLandUse != (float)EBM.a0)
		{
			yield = true;
		}
		previousLandUse = UpdateSlider(landUse, (float)EBM.a0, previousLandUse);
		if (yield)
		{
			yield return new WaitForSeconds(1.5f);
		}
		yield = false;
		
		if (World.publicOpinion / 100 != previousPublicOpinion)
		{
			yield = true;
		}
		previousPublicOpinion = UpdateSlider(publicOpinion, World.publicOpinion / 100, previousPublicOpinion, invertColors: true);
		if (yield)
		{
			yield return new WaitForSeconds(1.5f);
		}
		yield = false;

		if (previousEmissions != (float)EBM.F)
		{
			yield = true;
		}
		previousEmissions = UpdateSlider(emissions, (float)EBM.F, previousEmissions); // used to be /14
		if (yield)
		{
			yield return new WaitForSeconds(1.5f);
		}
		yield = false;

		if (previousEconomy != World.money / 100)
		{
			yield = true;
		}
		previousEconomy = UpdateSlider(economy, World.money / 100, previousEconomy);
		if (yield)
		{
			yield return new WaitForSeconds(1.5f);
		}
		yield return null;
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
