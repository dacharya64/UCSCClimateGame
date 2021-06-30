using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.UI;

public class CityScript : RegionController
{ // TODO: maybe rename to CityController for consistency
    public static CityScript Instance { get => instance as CityScript; } // static instance
    public enum BillDifficulty { Easy, Med, Hard }
    public Text billNumberText;
    public Text totalBillNumberText;
    public GameObject counter;
    private bool firstVisit;

    BillDifficulty currentDifficulty = BillDifficulty.Easy;
    Dictionary<BillDifficulty, List<BillData>> bills = new Dictionary<BillDifficulty, List<BillData>>();
    List<BillData> currentBillList => bills[currentDifficulty];

    int billNumber = 0;
    BillData currentBill => currentBillList[billNumber];
    int numberOfBills = 3; // Total number of bill choices to make
    int currentBillIndex = 0;
    

    [SerializeField] Text mainTitle = default;
    [SerializeField] Bill left = default, right = default;
    [SerializeField, Range(0.01f, 0.1f)] float speed = .1f;
    public GameObject aboutPrompt;
    public Text aboutText;
    public GameObject[] arrows;
    public List<int> usedBills = new List<int>();
    public string selectedBillHalf;

    /// <summary> Namespace to hold bill data </summary>
    public struct BillData
    {
        public string name;
        public string tease;
        public BillHalf left, right;

        /// <summary> inner struct for each half to preserve namespace </summary>
        public struct BillHalf
        {
            public string title, body, tags;
            public Dictionary<string, float> effects;
            public BillHalf(string title, string body, string tags) => (this.title, this.body, this.effects, this.tags) = (title, body, null, tags);
        }

        public BillData(string name, string tease, Dictionary<string, string> left = null, Dictionary<string, string> right = null)
        {
            this.name = name;
            this.tease = tease;
            this.left = new BillHalf(left["title"], left["body"], left["tags"]);
            this.right = new BillHalf(right["title"], right["body"], right["tags"]);
        }
    }

    void Start()
    {
        bills = LoadBills();
        currentDifficulty = BillDifficulty.Easy;
        (left.speed, right.speed) = (speed, speed);
        if (!firstVisit)
        {
            // remove from the current bill list all of the indices that have been used 
            for (int i = 0; i < GameManager.Instance.billIndices.Count; i++)
            {
                Debug.Log(GameManager.Instance.billIndices[i]);
                usedBills.Add(GameManager.Instance.billIndices[i]);
                currentBillList.RemoveAt(GameManager.Instance.billIndices[i]);
            }
        }
        billNumber = Random.Range(0, currentBillList.Count);
    }

    protected override void Init()
    { // called from parent
        firstVisit = false;
        counter.SetActive(true);
        mainTitle.transform.root.gameObject.SetActive(true);
        InitBill(currentBill);
    }

    protected override void IntroInit()
    { // called from parent
        firstVisit = true;
        totalBillNumberText.text = "/1";
        counter.SetActive(true);
        mainTitle.transform.root.gameObject.SetActive(true);
        InitBill(currentBill);
    }

    public static Dictionary<BillDifficulty, List<BillData>> LoadBills() =>
        new string[] { "easy", "med", "hard" }.Map(level =>
            (level, JsonConvert.DeserializeObject<List<BillData>>(Resources.Load<TextAsset>($"bills_{level}").text)))
        .ToDictionary(x => (BillDifficulty)System.Enum.Parse(typeof(BillDifficulty), x.Item1, true), x => { return x.Item2; });

    static Dictionary<string, float> ParseTag(string tag) => tag.Split().ToDictionary(t => Regex.Match(t, @"[A-z]*(?=\+|-)").ToString(), t => float.Parse(Regex.Match(t, @"(?:\+|-).*").ToString()));

    void InitBill(BillData currentBill)
    {
        mainTitle.text = currentBill.name.ToString();
        aboutText.text = currentBill.tease.ToString();
        currentBill.left.effects = ParseTag(currentBill.left.tags);
        currentBill.right.effects = ParseTag(currentBill.right.tags);
        left.SetBill(currentBill.left);
        right.SetBill(currentBill.right);
    }

    public void GetNextBill()
    {
        //first, do the effects of the chosen bill
        EnactBillEffects();

        // clear the existing arrows 
        arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject arrow in arrows)
        {
            arrow.SetActive(false);
        }
        // make both left and right opaque and hide "confirm" button 
        SetOpaque(GameObject.Find("Left"));
        SetOpaque(GameObject.Find("Right"));
        selectedBillHalf = "none";
        GameObject.Find("Confirm Button").SetActive(false);
        // remove the previous item from the list 
        usedBills.Add(billNumber);
        currentBillList.RemoveAt(billNumber);

        // hide all the arrows again 

        // Now check to see if go to another bill, or go back to overworld -- true if counter at certain amount and not first visit 
        if (currentBillIndex <= numberOfBills - 2 && !firstVisit)
        {
            billNumber = Random.Range(0, currentBillList.Count); // pulls bills from those left in the list 
            currentBillIndex++;
            //Update the UI
            int billOutputValue = currentBillIndex + 1;
            billNumberText.text = billOutputValue.ToString();
            // Go to the next pair of bills
            BillData currentBill = currentBillList[billNumber];
            InitBill(currentBill);
        }
        else
        {
            // Show return prompt
            GameManager.Instance.billIndices = usedBills;
            base.GameOver();

        }

        void EnactBillEffects() {
            Debug.Log("Selected bill is: " + selectedBillHalf);
        }
    }

    public void SetTransparent(GameObject ui) => UIController.SetUIAlpha(ui, .7f);

    public void SetOpaque(GameObject ui) => UIController.SetUIAlpha(ui, 1f);

    public void OpenAbout()
    {
        aboutPrompt.SetActive(true);
    }

    public void CloseAbout()
    {
        aboutPrompt.SetActive(false);
    }

    public void PlayScribbleSound() {
        AudioManager.Instance.Play("SFX_Scribble");
    }

    public void SetSelectedBillHalf(string clickedBillHalf) {
        selectedBillHalf = clickedBillHalf;
        //Debug.Log("selected " + selectedBillHalf);
    }
}

