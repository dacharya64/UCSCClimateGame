using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour { 
    public Slider sfxSlider;
    public Slider musicSlider;

   public void adjustMusicVolume(float volume) {
        AudioManager.Instance.UpdateMusicVolume(volume);
    }

    public void adjustSFXVolume(float volume) {
        AudioManager.Instance.UpdateSFXVolume(volume);
    }

    public void resetToDefault() {
        AudioManager.Instance.UpdateMusicVolume(0.5f);
        AudioManager.Instance.UpdateSFXVolume(0.5f);
    }

    public void resetToPreviousVolumes() {
        musicSlider.value = GameManager.Instance.previousMusicVolume;
        sfxSlider.value = GameManager.Instance.previousSFXVolume;
        AudioManager.Instance.UpdateMusicVolume(GameManager.Instance.previousMusicVolume);
        AudioManager.Instance.UpdateSFXVolume(GameManager.Instance.previousSFXVolume);
    }
}
