/*
 * BIG TODO HERE: actually use the tile map
 * with a tile map, you can condense all the Green/Fire/Tree/Water ID classes
 * into one class that handles all functionality 
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SubtropicsWorld : MonoBehaviour {

	[HideInInspector] public SubtropicsWorld world;


	public GameObject cloudPrefab, cellPrefab, waterPrefab;
	[HideInInspector] public GameObject[] reservoirs = new GameObject[2];
	public Sprite[] reservoirSprites;

	[SerializeField] int reservoirTotalSize;
	[SerializeField] int spreadability = 3;
	[Range(0, 100)] public int treeDensity;

	GridLayout gridLayout;

	// TODO: this ↓ is bad, use the bounds object of the tilemap
	public Vector3Int topLeftCell = new Vector3Int(-36, 16, 0);
	public Vector3Int topRightCell = new Vector3Int(32, 16, 0);
	public Vector3Int bottomLeftCell = new Vector3Int(-36, -20, 0);
	public Vector3Int bottomRightCell = new Vector3Int(32, -20, 0);

	Tilemap tilemap;

	[SerializeField] int width, height;
	public GameObject[, ] cellArray; // central cell data structure
	// could maybe use a List<GameObject> instead and store pos in each instance
	List<GameObject> clouds = new List<GameObject>();

	void Start() {
		world = GetComponentInChildren<SubtropicsWorld>();
		tilemap = transform.GetComponent<Tilemap>();
		gridLayout = this.GetComponentInParent<GridLayout>();
		PopulateVanillaWorld();
		PopulateWater();
		PopulateTree();
		PopulateCloud();
		PopulateMountain();
		StartCoroutine(WaitForFire(0)); // first fire mutation
	}

	void PopulateCloud() {
		GameObject cloudParent = new GameObject("Clouds");
		var bottomBound = gridLayout.CellToWorld(bottomLeftCell);
		var topBound = gridLayout.CellToWorld(topRightCell);
		var centre = (bottomBound + topBound) / 2;
		var dist = (topBound - bottomBound) / 2;

		for (int i = 0; i < 10; i++)
			clouds.Add(Instantiate(cloudPrefab, new Vector3(Random.Range(-dist.x, dist.x), Random.Range(-dist.y, dist.y), 0) + centre, Quaternion.identity, cloudParent.transform));
	}

	void PopulateVanillaWorld() {
		GameObject cellParent = new GameObject("Cells");

		// get corner positions of the world 
		width = topRightCell.x - topLeftCell.x + 1;
		height = topLeftCell.y - bottomLeftCell.y + 1;
		cellArray = new GameObject[width + 1, height + 1];

		for (var i = 0; i <= width; i++)
			for (var j = 0; j <= height; j++) {
				Vector3Int posInt = new Vector3Int(topLeftCell.x + i, bottomLeftCell.y + j, 0);
				Vector3 pos = tilemap.GetCellCenterWorld(posInt);

				// instantiate and construct 2d array
				GameObject go = Instantiate(cellPrefab, pos, Quaternion.identity);
				go.transform.parent = cellParent.transform;
				go.GetComponent<IdentityManager>().id = IdentityManager.Identity.Green; // default to green
				cellArray[i, j] = go;
				go.GetComponent<AudioSource>().enabled = false;
			}
	}

	/// <summary> Create water cells </summary>
	void PopulateWater() {
		// TODO: water size according to precipitation
		reservoirTotalSize = 0;
		//int reservoirGen = Random.Range(0,4);
		int reservoirGen = 0;

		// TODO: don't hard code starting from here
		for (int r = 0; r < 2; r++) {
			var(waterHeight, waterWidth) = new [] {
				(1, 1), (4, 3), (3, 2), (1, 2), (2, 5)
			}[reservoirGen];

			int waterX = 0, waterY = 0;
			if (r == 1) // this is the last reservoir being generated
				(waterX, waterY) = (6, 5);
			else
				(waterX, waterY) = (15, 8);

			// modulate water based on difficulty 
			waterHeight = waterHeight * (6 - SubtropicsController.Instance.difficulty); 
			waterWidth = waterWidth * (6 - SubtropicsController.Instance.difficulty); 
			

			// TODO: don't hard code, ending here

			for (int i = waterX; i < waterX + waterWidth; i++)
				for (int j = waterY; j < waterY + waterHeight; j++)
					cellArray[i, j].GetComponent<IdentityManager>().id = IdentityManager.Identity.Water;

			reservoirTotalSize += waterHeight * waterWidth;

			// TODO: do this better
			Vector3 pos1 = tilemap.GetCellCenterWorld(GetVector3IntFromCellArray(waterX, waterY));
			Vector3 pos2 = tilemap.GetCellCenterWorld(GetVector3IntFromCellArray(waterX + waterWidth, waterY + waterHeight));
			Vector3 reservoirPos = (pos1 + pos2) / 2 + new Vector3(-0.25f, -0.25f, 0); // TODO: don't hard code
			GameObject water = Instantiate(waterPrefab, reservoirPos, Quaternion.identity);
			water.SetActive(true);

			water.GetComponent<SpriteRenderer>().sprite = reservoirSprites[reservoirGen];
			water.GetComponent<SpriteRenderer>().size = new Vector2(waterWidth / 4, waterHeight / 4);
			water.transform.localScale = new Vector3(0.4f, 0.4f, 1) * (6 - SubtropicsController.Instance.difficulty); // TODO: don't hard code

			reservoirs[r] = water;
			SubtropicsController.Instance.player.GetComponentsInChildren<WaterArrow>() [r].waterPosition = water.transform.position;
		}
	}

	void PopulateTree() { // TODO: procedurally generate trees
		// hand authored three regions of mountain areas filled with trees
		// Region1   left top
		int width1 = Random.Range(width / 4, width / 3); // right
		int height1 = Random.Range(height / 3, height / 2); // bottom

		// Region2    right
		int width2_0 = Random.Range(width / 3 + 4, width / 2); // left
		int width2_1 = width - 6; // right
		int height2_0 = Random.Range(height / 6, height / 5); // bottom
		int height2_1 = height * 2 / 3; // top 

		// Region3     bottom
		int width3_0 = Random.Range(width / 9, width / 6); // left
		int width3_1 = Random.Range(width * 8 / 9, width * 9 / 10); // right
		int height3_0 = Random.Range(height * 2 / 3 + 4, height * 4 / 5); // top
		int height3_1 = height - 6; // bottom

		GenerateTreeInRegion(6, width1, 6, height1);
		GenerateTreeInRegion(width2_0, width2_1, height2_0, height2_1);
		GenerateTreeInRegion(width3_0, width3_1, height3_0, height3_1);
	}

	void PopulateMountain() {
		int width1 = topRightCell.x - topLeftCell.x + 1;
		int height1 = height / 8;
		GenerateMountainInRegion(0, width1, 0, height1);
		int width2 = width / 11;
		int height2 = topLeftCell.y - bottomLeftCell.y + 1;
		GenerateMountainInRegion(0, width2, 0, height2);
		int width3 = width * 10 / 11;
		GenerateMountainInRegion(width3, width1, 0, height2);
		int height3 = height * 7 / 8;
		GenerateMountainInRegion(0, width1, height3, height2);
	}

	void GenerateTreeInRegion(int w0, int w1, int h0, int h1) {
		for (var i = w0; i <= w1; i++)
			for (var j = h0; j <= h1; j++) {
				IdentityManager I = cellArray[i, j].GetComponent<IdentityManager>();
				if (I.id == IdentityManager.Identity.Green && I.id != IdentityManager.Identity.Mountain) {
					if (Random.value * 100 < treeDensity) {
						I.id = IdentityManager.Identity.Tree;
						I.fireVariance = 1; // if fire happens, set variance type to tree fire 
					}
				}
			}
	}

	void GenerateMountainInRegion(int w0, int w1, int h0, int h1)
	{
		for (var i = w0; i <= w1; i++)
			for (var j = h0; j <= h1; j++)
			{
				IdentityManager I = cellArray[i, j].GetComponent<IdentityManager>();
				I.id = IdentityManager.Identity.Mountain;

			}
	}

	/// <summary> Mutates grass to fire </summary>
	void MutateToFire() {
		// pick a random cell
		// if green, mutates to fire
		GameObject cell = cellArray[Random.Range(0, width), Random.Range(0, height)];
		IdentityManager I = cell.GetComponent<IdentityManager>();
		//if (I.id is IdentityManager.Identity.Green || I.id is IdentityManager.Identity.Tree )  // can mutate green or tree
		if (I.id is IdentityManager.Identity.Tree)
		{
			I.id = IdentityManager.Identity.Fire;
			cell.GetComponent<AudioSource>().enabled = true;
		}

        // Spread the fire to random neighbors 
        // find all cells that are fire 
        List<GameObject> fireCells = new List<GameObject>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (cellArray[i, j].GetComponent<IdentityManager>().id is IdentityManager.Identity.Fire)
                {
                    fireCells.Add(cellArray[i, j]);
                }
            }
        }

        foreach (GameObject fireCell in fireCells)
        {
            List<GameObject> fire_neighbors = GetNeighbors(fireCell);

            foreach (GameObject neighbor in fire_neighbors)
            {
                IdentityManager I_neighbor = neighbor.GetComponent<IdentityManager>();

                if (I_neighbor.id is IdentityManager.Identity.Tree)
                {
                    I_neighbor.id = IdentityManager.Identity.Fire;
                    neighbor.GetComponent<AudioSource>().enabled = true;
                }
            }
        }
    }

	IEnumerator WaitForFire(float s) {
		yield return new WaitForSeconds(s);
		MutateToFire();
		StartCoroutine(WaitForFire(8 - SubtropicsController.Instance.difficulty));
	}

	/// <summary> Returns cell neighbors - up to 4 dir</summary>
	public List<GameObject> GetNeighbors(GameObject cell)
	{
		Vector3Int cellPosition = gridLayout.WorldToCell(cell.transform.position);

		int x = cellPosition.x - topLeftCell.x; // convert pos vec3int to correct index in array
		int y = cellPosition.y - bottomLeftCell.y;

		string windDirection = SubtropicsController.Instance.wind.dir.ToString();

		List<GameObject> neighbors = new List<GameObject>();

		if (windDirection == "NE")
		{
			neighbors.Add(cellArray[x, y + 1]);
			neighbors.Add(cellArray[x + 1, y]);
		}
		else if (windDirection == "E")
		{
			neighbors.Add(cellArray[x + 1, y]);
		}
		else if (windDirection == "SE")
		{
			neighbors.Add(cellArray[x, y - 1]);
			neighbors.Add(cellArray[x + 1, y]);
		}
		else if (windDirection == "S")
		{
			neighbors.Add(cellArray[x, y - 1]);
		}
		else if (windDirection == "SW")
		{
			neighbors.Add(cellArray[x, y - 1]);
			neighbors.Add(cellArray[x - 1, y]);

		}
		else if (windDirection == "W")
		{
			neighbors.Add(cellArray[x - 1, y]);
		}
		else if (windDirection == "NW")
		{
			neighbors.Add(cellArray[x, y + 1]);
			neighbors.Add(cellArray[x - 1, y]);
		}
		else if (windDirection == "N")
		{
			neighbors.Add(cellArray[x, y + 1]);
		}
		return neighbors;
	}

	/// <summary> Returns cell radius - outwards 2+ </summary>
	public List<IdentityManager> GetRadius(Vector3 pos) {
		Vector3Int cellPosition = gridLayout.WorldToCell(pos);

		int x = cellPosition.x - topLeftCell.x; // convert pos vec3int to correct index in array
		int y = cellPosition.y - bottomLeftCell.y;

		int r = 1; // radius

		List<IdentityManager> area = new List<IdentityManager>();

		for (int a = -r; a <= r; a++)
			for (int b = -r; b <= r; b++)
				if (x + a >= 0 && x + a <= width && y + b <= height && y + b >= 0)
					area.Add(cellArray[x + a, y + b].GetComponent<IdentityManager>());
		return area;
	}

	public GameObject GetCell(Vector3 worldPos) { // TODO: could be a Vector3 indexer function
		Vector3Int cellLoc = gridLayout.WorldToCell(worldPos);
		int x = cellLoc.x - topLeftCell.x; // convert pos vec3int to correct index in array
		int y = cellLoc.y - bottomLeftCell.y;
		return cellArray[x, y];
	}

	/// <summary> Returns cell vec3int value given cell array 2d index</summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <returns></returns>
	public Vector3Int GetVector3IntFromCellArray(int i, int j) { // TODO: fix with with tilemap
		return new Vector3Int(topLeftCell.x + i, bottomLeftCell.y + j, 0);
	}
}
