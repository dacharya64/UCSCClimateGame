using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Logger : PathfindingAgent {
	[HideInInspector] public Vector3Int choppingTile;
	/*void Start() => OnReturn.AddListener(() => ForestController.Instance.damage += 20);*/
}

public static class LoggerActions {
	public static void Chop(Logger l) {
		ForestController.Instance.StartCoroutine(PlayChopSFX());
		l.anim.SetTrigger("Chopping");
		l.transform.localScale = new Vector3(-1, 1, 1); // flip sprite
		ForestController.Instance.StartCoroutine(ChopAndReturn(l));
	}

	public static IEnumerator ChopAndReturn(Logger l) {
		yield return ForestController.Instance.StartCoroutine(VolunteerActions.WaitAndReturn(l, 3));
		ForestGrid.map.SetTile(l.choppingTile, ForestGrid.stump);
		ForestGrid.RemoveTree(l.choppingTile);
	}

	public static IEnumerator PlayChopSFX() {
		AudioManager.Instance.Play("SFX_Tree_Chop");
		yield return new WaitForSeconds(0.8f);
		AudioManager.Instance.Play("SFX_Tree_Chop");
		yield return new WaitForSeconds(0.8f);
		AudioManager.Instance.Play("SFX_Tree_Chop");
		yield return new WaitForSeconds(0.8f);
		AudioManager.Instance.Play("SFX_Tree_Chop");
		yield return null;
	}
}
