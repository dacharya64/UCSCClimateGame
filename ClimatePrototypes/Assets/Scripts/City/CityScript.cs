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

	BillDifficulty currentDifficulty = BillDifficulty.Easy;
	Dictionary<BillDifficulty, List<BillData>> bills = new Dictionary<BillDifficulty, List<BillData>>();
	List<BillData> currentBillList => bills[currentDifficulty];

	//int totalBills = currentBillList.Count;
	// Find a random pair of bills out of all of the ones in the list
	int billNumber = 0;

	BillData currentBill => currentBillList[billNumber];
	int numberOfBills = 3; // Total number of bill choices to make
	int currentBillIndex = 0;

	[SerializeField] Text mainTitle = default;
	[SerializeField] Bill left = default, right = default;
	[SerializeField, Range(0.01f, 0.1f)] float speed = .1f;
	public GameObject returnPrompt;

	/// <summary> Namespace to hold bill data </summary>
	public struct BillData {
		public string name;
		public BillHalf left, right;

		/// <summary> inner struct for each half to preserve namespace </summary>
		public struct BillHalf {
			public string title, body, tags;
			public Dictionary<string, float> effects;
			public BillHalf(string title, string body, string tags) => (this.title, this.body, this.effects, this.tags) = (title, body, null, tags);
		}

		public BillData(string name, Dictionary<string, string> left = null, Dictionary<string, string> right = null) {
			this.name = name;
			this.left = new BillHalf(left["title"], left["body"], left["tags"]);
			this.right = new BillHalf(right["title"], right["body"], right["tags"]);
		}
	}

	void Start() {
		bills = LoadBills();
		currentDifficulty = BillDifficulty.Easy;
		(left.speed, right.speed) = (speed, speed);
		billNumber = Random.Range(0, 5);
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
		currentBill.left.effects = ParseTag(currentBill.left.tags);
		currentBill.right.effects = ParseTag(currentBill.right.tags);
		left.SetBill(currentBill.left);
		right.SetBill(currentBill.right);
	}

	public void GetNextBill() {
		// remove the previous item from the list 
		currentBillList.RemoveAt(billNumber);
		// Now check to see if go to another bill, or go back to overworld
		if (currentBillIndex <= numberOfBills - 2)
		{
			billNumber = Random.Range(0, currentBillList.Count);
			currentBillIndex++;
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
}
