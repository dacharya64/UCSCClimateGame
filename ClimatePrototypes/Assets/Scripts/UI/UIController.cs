﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public GameObject settingsGroup;
	public InfoController infoGroup;
	public GameObject notificationGroup;
	public Text moneyText;
	public Text turnText;

	bool settingsOn = false;
	bool infoOn = false;
	bool notificationsOn = false;

	// Start is called before the first frame update
	void Start() {
		turnText.text = $"Turn {World.turn}";
	}

	// Update is called once per frame
	void Update() {
		moneyText.text = $"Money: ${World.money:0,0}";
	}

	public void IncrementTurn() {
		World.turn++;
		turnText.text = $"Turn {World.turn}";
	}

	public void ToggleSettings() {
		settingsOn = !settingsOn;

		if (settingsGroup)
			settingsGroup.SetActive(settingsOn);
	}

	public void ToggleInfo() {
		infoOn = !infoOn;

		if (infoGroup) {
			// infoGroup.gameObject.SetActive(infoOn);
			infoGroup.bRenderOnNextFrame = true;
		}
	}

	public void ToggleNotifications() {
		notificationsOn = !notificationsOn;

		if (notificationGroup)
			notificationGroup.SetActive(notificationsOn);
	}

	public void Exit() => GameManager.QuitGame();

	public void ChangeLevel(string level) => GameManager.Transition(level);

}
