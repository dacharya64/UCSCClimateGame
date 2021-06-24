using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class Bill : MonoBehaviour {
	[SerializeField] Text title = default, body = default;
	[SerializeField] GameObject iconWrapper = default;
	[HideInInspector] public float speed;

	void Start() {
		foreach (Transform child in iconWrapper.transform)
			child.gameObject.SetActive(false);
	}

	public void SetBill(CityScript.BillData.BillHalf currentBill) {
		Print(currentBill.title, currentBill.body);
		ArrangeIcons(currentBill.effects);
	}

	void Print(string titleText, string bodyText) {
		StartCoroutine(UIController.Typewriter(title, titleText, speed));
		StartCoroutine(UIController.Typewriter(body, bodyText, speed));
	}

	/// <summary> Turn off icons that are not on bill </summary>
	void ArrangeIcons(Dictionary<string, float> effects) {
		List<RectTransform> showIcons = new List<RectTransform>();
		foreach (Transform child in iconWrapper.transform) {
			child.gameObject.SetActive(effects.ContainsKey(child.name));
/*			if (effects.ContainsKey(child.name)) {
				Debug.Log("Child name is " + child.name);
				float result = effects[child.name];
				Debug.Log("Result is: " + result);
			}*/

			//float result = effects[child.name];
			//Debug.Log("Result is: " + result);
			if (child.gameObject.activeSelf)
            {
				showIcons.Add(child as RectTransform);
			}
		}

		float size = showIcons[0].rect.width;
		int num = showIcons.Count;
		foreach (var (child, i) in showIcons.Enumerator())
		{
			child.localPosition = new Vector2(size * ((i - num / 2) + (num % 2 == 1 ? 0 : .5f)), child.localPosition.y);
			if (effects.ContainsKey(child.name))
			{
				Debug.Log("Child name is " + child.name);
				float result = effects[child.name];
				if (result > 0)
				{
					child.GetComponent<Image>().color = new Color32(0, 255, 0, 100);
				}
				else if (result < 0) {
					child.GetComponent<Image>().color = new Color32(255, 0, 0, 100);
				}
				Debug.Log("Result is: " + result);
			}
		}
	}
}
