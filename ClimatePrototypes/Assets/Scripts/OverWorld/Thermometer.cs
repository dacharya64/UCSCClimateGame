using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Thermometer : MonoBehaviour { 
    [SerializeField] Slider temperature = default;
    float previousValue; 
    
    void Start() { }
    
    void Update()
    {
        //UpdateSlider(temperature, (float) World.averageTemp);
    }

    void UpdateSlider(Slider slider, float targetValue)
    {
        //slider.value = targetValue;
        Debug.Log("Previous value: " + previousValue);
        Debug.Log("Next value: " + targetValue);
        slider.value = Mathf.Lerp(previousValue, (float) targetValue, Time.deltaTime * 8f);
        previousValue = slider.value;
    }
}
