﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

public class ForestGrid : MonoBehaviour {
	public static Tilemap map;
	[SerializeField] TileBase[] _trees = default;
	public static TileBase[] trees;
	public static TileBase stump { get => trees[0]; }
	public static TileBase dead { get => trees[1]; }
	public static TileBase empty { get => trees[2]; }
	public static TileBase sprout { get => trees[3]; }
	Vector3Int hoverCell;
	[SerializeField] TileBase hoverTile = default;
	public static List<ForestTree> currentTrees = new List<ForestTree>();
	public static float growthTime = 20;
	// public List<ForestTree> treeMonitor;

	void Awake() => trees = _trees;

	void Start() {
		map = GetComponentInChildren<Tilemap>();

		for (var (i, max) = (0, (int) map.size.x * map.size.y / 2 * 1); i < max; i++) {
			var randomPos = (Vector3) (map.cellBounds.max - map.cellBounds.min);
			randomPos.Scale(new Vector3(Random.value, Random.value, Random.value));
			var randomPosInt = Vector3Int.FloorToInt(randomPos) + map.cellBounds.min;
			if (currentTrees.Any(tree => tree.pos == randomPosInt))
				continue;
			currentTrees.Add(new ForestTree(randomPosInt, _trees[Random.Range(2, 6)]));
		}
	}

	void Update() {
		if (ForestController.Instance.hasSelected) {
			Vector3Int newHover = map.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			if (map.cellBounds.Contains(newHover)) {
				if (newHover != hoverCell) {
					ClearHover(hoverCell);
					map.SetTile(new Vector3Int(newHover.x, newHover.y, 1), hoverTile);
					map.SetColor(new Vector3Int(newHover.x, newHover.y, 1), new Color(1, 1, 1, .3f));
				}
			} else
				ClearHover(hoverCell);
			hoverCell = newHover;

			if (Input.GetMouseButtonDown(0) && map.cellBounds.Contains(hoverCell) && !ForestController.Instance.activeTiles.Contains(hoverCell)) {
				if (map.GetTile(hoverCell) == empty)
				{
					World.publicOpinion = World.publicOpinion + 5;
					ForestController.forcingIncrease -= 0.06;
					ForestController.Instance.SetVolunteerTarget(hoverCell, VolunteerActions.Plant);
				}
				else if (map.GetTile(hoverCell) == stump || map.GetTile(hoverCell) == dead)
				{
					World.publicOpinion = World.publicOpinion + 5;
					ForestController.forcingIncrease -= 0.06;
					ForestController.Instance.SetVolunteerTarget(hoverCell, VolunteerActions.Clear);
				}
				ClearHover(hoverCell);
			}
		}
	}

	public static void ClearHover(Vector3Int cell) {
		map.SetTile(new Vector3Int(cell.x, cell.y, 1), null);
		map.SetColor(new Vector3Int(cell.x, cell.y, 1), Color.white);
	}

	public static void RemoveTree(Vector3Int pos) {
		var remove = ForestGrid.currentTrees.Find(tree => tree.pos == pos);
		ForestGrid.currentTrees.Remove(remove);
		// map.SetTile(remove.pos, empty);
	}
}

[System.Serializable]
public class ForestTree {
	[SerializeField] TileBase _tile;
	TileBase tile {
		get => _tile;
		set {
			_tile = value;
			ForestGrid.map.SetTile(pos, _tile);
			index = ForestGrid.trees.ToList().IndexOf(_tile);
		}
	}
	// TileBase tile { get=>ForestGrid.map.GetTile(pos); set=>ForestGrid.map.SetTile(pos, value);} 
	public Vector3Int pos;
	[SerializeField] int index = -1;
	public bool alive { get => index > 1; }
	Coroutine sequestration;

	public ForestTree(Vector3Int pos, TileBase tile = null) {
		this.tile = tile ?? ForestGrid.empty;
		// Debug.Log(this.tile);
		this.pos = pos;
		ForestController.Instance.StartCoroutine(Grow(0));
		// NeighbourCount();
		sequestration = ForestController.Instance.StartCoroutine(Sequestre(4, initialDelay : 4 - index));
	}

	public override string ToString() => pos.ToString();

	~ForestTree() => ForestController.Instance.StopCoroutine(sequestration);

	IEnumerator Sequestre(float interval = 2, float initialDelay = 0) {
		yield return new WaitForSeconds(initialDelay);
		yield return new WaitForSeconds(interval);
		float effect = index >= 3 ? index * (1 - NeighbourCount() / 8f) : index == 2 ? 0 : -index;
		//ForestController.Instance.damage = Mathf.Max(ForestController.Instance.damage - effect / 2, 0);
		sequestration = ForestController.Instance.StartCoroutine(Sequestre(interval));
	}

	IEnumerator Grow(float time) {
		yield return new WaitForSeconds(time);
		if (index == ForestGrid.trees.Length - 1)
			tile = ForestGrid.dead;
		else {
			// float awaitTime = (ForestGrid.growthTime + (Random.value - .25f) * 2) * (1 + .5f * NeighbourCount() / 4f);
			float awaitTime = ForestGrid.growthTime * (1 + NeighbourCount() / 4f) + (Random.value - .5f);
			if (index == 4) {
				ForestController.Instance.activeTrees.Add(pos);
				awaitTime *= 1.5f;
			}
			tile = ForestGrid.trees[index + 1];
			ForestController.Instance.StartCoroutine(Grow(awaitTime));
		}
	}

	static bool IsTree(TileBase tile) => tile != null && tile != ForestGrid.dead && tile != ForestGrid.stump && tile != ForestGrid.empty;
	// TODO: maybe store this array somewhere and not instantiate every time 
	float NeighbourCount() => (new [] { Vector3Int.up, Vector3Int.left, Vector3Int.right, Vector3Int.down }).Count(dir => IsTree(ForestGrid.map.GetTile(pos + dir)));
}
