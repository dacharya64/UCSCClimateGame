using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
   public void adjustMusicVolume(float volume) {
        AudioManager.Instance.UpdateMusicVolume(volume);
    }

    public void adjustSFXVolume(float volume) {
        AudioManager.Instance.UpdateSFXVolume(volume);
    }

    public void resetToDefault() {
        AudioManager.Instance.UpdateMusicVolume(0.5f);
        AudioManager.Instance.UpdateSFXVolume(0.5f);
        // Change toggle positions
    }
}
