using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager> { // TODO: ease transitions between music and sounds
	public enum AudioType { SFX, Music }

	public List<Sound> sounds; // maybe build on load and parse through all playables?
	AudioSource sfxSource1,	sfxSource2,	sfxSource3,	musicSource;

	public float totalMusicVolume;
	public float totalSFXVolume;

	public override void Awake() {
		base.Awake();
		// collect sounds here? maybe even read from sound resources
	}

	void Start() {
		totalMusicVolume = 0.5f;
		totalSFXVolume = 0.5f;
		while (GetComponents<AudioSource>().Length > 0)
			Destroy(GetComponent<AudioSource>());
		sfxSource1 = gameObject.AddComponent<AudioSource>();
		sfxSource2 = gameObject.AddComponent<AudioSource>();
		sfxSource3 = gameObject.AddComponent<AudioSource>();
		musicSource = gameObject.AddComponent<AudioSource>();
		musicSource.loop = true;
	}

	public void StopMusic() => musicSource.Stop();

	public void StopSFX() {
		sfxSource1.Stop();
		sfxSource2.Stop();
		sfxSource3.Stop();
	}

	public void Play(string sound) => Play(GetSound(sound));

	public void Play(Sound sound) {
		AudioSource channel = GetChannel(sound.type);
		sound.source = channel;
		channel.clip = sound.clip;
		if (sound.type == AudioType.Music)
		{
			channel.volume = sound.volume * totalMusicVolume;
		}
		else if (sound.type == AudioType.SFX)
		{
			channel.volume = sound.volume * totalSFXVolume;
		}
		else {
			channel.volume = sound.volume;
		}
		channel.pitch = sound.pitch;
		channel.Play();
		// Debug.Log($"played {sound} on channel {channel.name}");
	}

	AudioSource GetChannel(AudioType type) {
		if (type == AudioType.Music)
			return musicSource;
		else {
			if (sfxSource1.clip == null)
				return sfxSource1;
			if (sfxSource2.clip == null)
				return sfxSource2;
			return sfxSource3;
		}
	}

	public void UpdateMusicVolume(float vol) {
		totalMusicVolume = vol;
		musicSource.volume = vol;
	}

	public void UpdateSFXVolume(float vol)
	{
		totalSFXVolume = vol;
		sfxSource1.volume = vol;
		sfxSource2.volume = vol;
		sfxSource3.volume = vol;
	}

	public static Sound GetSound(string name) => Instance.sounds.Find(s => s.name == name);
	public void PlaySound(AudioClip clip) => Play(GetSound(clip.name));

}
