using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.UI;

public class CityScript : RegionController { // TODO: maybe rename to CityController for consistency
	public static CityScript Instance { get => instance as CityScript; } // static instance
	public enum BillDifficulty { Easy, Med, Hard }
	public Text billNumberText;

	BillDifficulty currentDifficulty = BillDifficulty.Easy;
	Dictionary<BillDifficulty, List<BillData>> bills = new Dictionary<BillDifficulty, List<BillData>>();
	List<BillData> currentBillList => bills[currentDifficulty];

	//int totalBills = currentBillList.Count;
	// Find a random pair of bills out of all of the ones in the list
	int billNumber = 0;

	BillData currentBill => currentBillList[2]; // TODO change this back to billNumber
	int numberOfBills = 3; // Total number of bill choices to make
	int currentBillIndex = 0;

	[SerializeField] Text mainTitle = default;
	[SerializeField] Bill left = default, right = default;
	[SerializeField, Range(0.01f, 0.1f)] float speed = .1f;
	public GameObject returnPrompt;
	public GameObject aboutPrompt;
	public Text aboutText;
	public GameObject[] arrows;

	/// <summary> Namespace to hold bill data </summary>
	public struct BillData {
		public string name;
		public string tease;
		public BillHalf left, right;

		/// <summary> inner struct for each half to preserve namespace </summary>
		public struct BillHalf {
			public string title, body, tags;
			public Dictionary<string, float> effects;
			public BillHalf(string title, string body, string tags) => (this.title, this.body, this.effects, this.tags) = (title, body, null, tags);
		}

		public BillData(string name, string tease,  Dictionary<string, string> left = null, Dictionary<string, string> right = null) {
			this.name = name;
			this.tease = tease;
			this.left = new BillHalf(left["title"], left["body"], left["tags"]);
			this.right = new BillHalf(right["title"], right["body"], right["tags"]);
		}
	}

	void Start() {
		bills = LoadBills();
		currentDifficulty = BillDifficulty.Easy;
		(left.speed, right.speed) = (speed, speed);
		billNumber = Random.Range(0, currentBillList.Count - 1);
	}

	protected override void Init() { // called from parent
		introBlock.GetComponentInChildren<Button>(true)?.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
			mainTitle.transform.root.gameObject.SetActive(true);
			InitBill(currentBill);
		}));
	}

	public static Dictionary<BillDifficulty, List<BillData>> LoadBills() =>
		new string[] { "easy", "med", "hard" }.Map(level =>
			(level, JsonConvert.DeserializeObject<List<BillData>>(Resources.Load<TextAsset>($"bills_{level}").text)))
		.ToDictionary(x => (BillDifficulty) System.Enum.Parse(typeof(BillDifficulty), x.Item1, true), x => { return x.Item2; });

	static Dictionary<string, float> ParseTag(string tag) => tag.Split().ToDictionary(t => Regex.Match(t, @"[A-z]*(?=\+|-)").ToString(), t => float.Parse(Regex.Match(t, @"(?:\+|-).*").ToString()));

	void InitBill(BillData currentBill) {
		mainTitle.text = currentBill.name.ToString();
		aboutText.text = currentBill.tease.ToString();
		currentBill.left.effects = ParseTag(currentBill.left.tags);
		currentBill.right.effects = ParseTag(currentBill.right.tags);
		left.SetBill(currentBill.left);
		right.SetBill(currentBill.right);
	}

	public void GetNextBill() {
		// clear the existing arrows 
        arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject arrow in arrows)
        {
            arrow.SetActive(false);
        }
		// make both left and right opaque and hide "confirm" button 
		SetOpaque(GameObject.Find("Left"));
		SetOpaque(GameObject.Find("Right"));
		GameObject.Find("Confirm Button").SetActive(false);
		// remove the previous item from the list 
		currentBillList.RemoveAt(billNumber);
		// hide all the arrows again 

		// Now check to see if go to another bill, or go back to overworld
		if (currentBillIndex <= numberOfBills - 2)
		{
			billNumber = Random.Range(0, currentBillList.Count - 1); // pulls bills from those left in the list 
			currentBillIndex++;
			//Update the UI
			int billOutputValue = currentBillIndex + 1; 
			billNumberText.text = billOutputValue.ToString();
			// Go to the next pair of bills
			BillData currentBill = currentBillList[billNumber];
			InitBill(currentBill);
		} else 
		{
			// Show return prompt
			returnPrompt.SetActive(true); 
		}
	}

	public void SetTransparent(GameObject ui) => UIController.SetUIAlpha(ui, .7f);
	
	public void SetOpaque(GameObject ui) => UIController.SetUIAlpha(ui, 1f);

	public void OpenAbout() {
		aboutPrompt.SetActive(true);
	}

	public void CloseAbout() {
		aboutPrompt.SetActive(false);
	}
}
