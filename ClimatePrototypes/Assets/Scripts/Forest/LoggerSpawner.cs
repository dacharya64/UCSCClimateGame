﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LoggerSpawner : MonoBehaviour {
	[SerializeField] GameObject loggerPrefab = default;
	[SerializeField] float interval = 15;
	void Start() => StartCoroutine(SpawnLogger(interval));

	IEnumerator SpawnLogger(float delay) {
		yield return new WaitForSeconds(delay);
		if (ForestController.Instance.activeTrees.Count > 0) {
			var targetIndex = (int) (Random.value * ForestController.Instance.activeTrees.Count);
			var target = ForestController.Instance.activeTrees[targetIndex];
			ForestController.Instance.activeTrees.RemoveAt(targetIndex);
			Logger logger = MoveLoggerOnRoad(new Vector3Int(3, -2, -9), target);
			yield return new WaitForSeconds(7);
			SetLoggerTarget(logger, target, LoggerActions.Chop);
		}
		StartCoroutine(SpawnLogger(delay));
	}

	public Logger MoveLoggerOnRoad(Vector3Int pos, Vector3Int target) {
		var newLogger = ForestController.Instance.NewAgent(loggerPrefab, transform.position, (Vector3)pos) as Logger;
		return newLogger;
	}
	
	public void SetLoggerTarget(Logger newLogger, Vector3Int pos, UnityEngine.Events.UnityAction<Logger> onReached) {
		newLogger.AssignTarget(pos);
		newLogger.choppingTile = pos;

		newLogger.OnReached.AddListener((PathfindingAgent agent) => onReached.Invoke(agent as Logger));
		// newLogger.OnReturn.AddListener(() => { Debug.Log("logger returned"); });
	}
}
