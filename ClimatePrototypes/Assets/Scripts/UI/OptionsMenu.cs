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
}
