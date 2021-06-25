using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class Bill : MonoBehaviour {
	[SerializeField] Text title = default, body = default;
	[SerializeField] GameObject iconWrapper = default;
	[HideInInspector] public float speed;
	private GameObject arrow1;
	private GameObject arrow2;

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
				string totalName = "";
				float result = effects[child.name];
				if (result > 0) // Change color to green 
				{
					//child.GetComponent<Image>().color = new Color32(0, 255, 0, 100);
					// find the up arrow and enable it 
					totalName = child.name + "ArrowUp1";
					arrow1 = child.transform.Find(totalName).gameObject;
					arrow1.SetActive(true);
					if (result > 1) {
						totalName = child.name + "ArrowUp2";
						arrow2 = child.transform.Find(totalName).gameObject;
						arrow2.SetActive(true);
					}

				}
				else if (result < 0) { // change color to red 
									   //child.GetComponent<Image>().color = new Color32(255, 0, 0, 100);
					totalName = child.name + "ArrowDown1";
					arrow1 = child.transform.Find(totalName).gameObject;
					arrow1.SetActive(true);
					if (result < -1)
					{
						totalName = child.name + "ArrowDown2";
						arrow2 = child.transform.Find(totalName).gameObject;
						arrow2.SetActive(true);
					}
				}
			}
		}
	}
}
