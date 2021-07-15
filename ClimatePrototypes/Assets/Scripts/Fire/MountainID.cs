using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainID : Tile
{
	
		[SerializeField] Sprite mountain = default;
		[HideInInspector] public int alt = 0;
		

		void Start()
		{
			alt = Random.Range(0, 2);
			UpdateTile();
		}

		protected override void UpdateTile()
		{
			if (idManager.id == IdentityManager.Identity.Green) {
			sr.sprite = mountain;
		}
		}
}
