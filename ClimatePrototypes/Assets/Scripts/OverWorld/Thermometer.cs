using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Thermometer : MonoBehaviour { 
    [SerializeField] Slider temperature = default;
    
    void Start() { }
    
    void Update()
    {
        UpdateSlider(temperature, (float) World.averageTemp);
    }

    void UpdateSlider(Slider slider, float value)
    {
        slider.value = value;
    }
}
