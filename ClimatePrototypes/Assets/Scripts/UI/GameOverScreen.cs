using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] Text FinalTemperatureText;
    [SerializeField] Text FinalTropicsTempText;
    [SerializeField] Text FinalSubtropicsTempText;
    [SerializeField] Text FinalArcticTempText;
    [SerializeField] Text FinalPublicOpinionText;
    [SerializeField] Text FinalEmissionsText;
    [SerializeField] Text FinalEconomyText;

    // Start is called before the first frame update
    void Start()
    {
        FinalTemperatureText.text = World.averageTemp.ToString("F3");
        FinalTropicsTempText.text = World.temp[0].ToString("F3");
        FinalSubtropicsTempText.text = World.temp[1].ToString("F3");
        FinalArcticTempText.text = World.temp[2].ToString("F3");
        FinalPublicOpinionText.text = World.publicOpinion.ToString();
        FinalEmissionsText.text = ((float)EBM.F).ToString("F3");
        FinalEconomyText.text = World.money.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        

    }
}
