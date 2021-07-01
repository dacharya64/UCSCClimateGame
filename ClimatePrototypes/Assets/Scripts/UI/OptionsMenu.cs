using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour { 
    public float previousMusicVolume;
    public float previousSFXVolume;
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

    public void savePreviousVolumes() {
        previousMusicVolume = AudioManager.Instance.totalMusicVolume;
        previousSFXVolume = AudioManager.Instance.totalSFXVolume;
    }

    public void resetToPreviousVolumes() {
        musicSlider.value = previousMusicVolume;
        sfxSlider.value = previousSFXVolume;
        AudioManager.Instance.UpdateMusicVolume(previousMusicVolume);
        AudioManager.Instance.UpdateSFXVolume(previousSFXVolume);
    }
}
