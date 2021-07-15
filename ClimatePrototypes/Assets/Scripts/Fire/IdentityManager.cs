/*
 * BIG TODO HERE: refactor so this is on a main manager obj instead of on every tile
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>  Controls the identity of the cell </summary>
public class IdentityManager : MonoBehaviour { // TODO: definitely rename to TileManager
	public enum Identity { Fire, Water, Green, Tree, Mountain }
	/// <summary> Controls the chance of it being ignited </summary>
	public enum Moisture { Moist, /* not likely that it could be ignited */ Normal, Dry }

	Identity _id = Identity.Green;
	public Identity id { get => _id; set { _id = value; UpdateTile(); } }
	public Moisture moisture = Moisture.Normal;
	public int fireVariance = 0; // 0 for green, 1 for tree. keep track of the nature of the cell before fire

	void UpdateTile() => GetComponents<Tile>().ToList().ForEach((o, i) => o.enabled = i == (int) id);

	/// <summary> sets current tile </summary>
	void OnMouseDown() => SubtropicsController.Instance.player.target = transform.position;
}

/// <summary> Parent class for Subtropics tiles </summary>
public abstract class Tile : MonoBehaviour {
	protected IdentityManager idManager;
	protected SpriteRenderer sr;
	void Awake() {
		sr = GetComponent<SpriteRenderer>();
		idManager = GetComponent<IdentityManager>();
	}

	void OnEnable() => UpdateTile();
	protected abstract void UpdateTile();
}
