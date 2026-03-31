// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.

using UnityEngine;

public class CreateCheckerBoardFlooring : MonoBehaviour {
	[SerializeField] private GameObject parentObject;
	[SerializeField] private Vector2 startingPoint = Vector2.zero;
	[Header("Grid: number of tiles on each axis")]
	[SerializeField] private Vector2Int gridSize = new(8, 8);
	[Header("Size of each tile in Unity world units (meters)")]
	[SerializeField] private float tileSizeUnits = 1f;
	[SerializeField] private Color color1 = Color.white;
	[SerializeField] private Color color2 = Color.black;

	public void CreateFlooring() {
		DeletePreviousFlooring();
		Create2DFlooring();
	}

	private void Create2DFlooring() {
		Texture2D tex = new Texture2D(1, 1);
		tex.SetPixel(0, 0, Color.white);
		tex.Apply();

		Sprite whiteSquare = Sprite.Create(
			tex,
			new Rect(0, 0, 1, 1),
			new Vector2(0.5f, 0.5f),
			1f
		);

		for (int x = 0; x < gridSize.x; x++) {
			for (int y = 0; y < gridSize.y; y++) {
				GameObject tileObj = new($"Tile_{x}_{y}");
				tileObj.transform.parent = parentObject.transform;

				tileObj.transform.localPosition = new Vector3(
					startingPoint.x + x * tileSizeUnits,
					startingPoint.y + y * tileSizeUnits,
					0f
				);

				tileObj.transform.localScale = new Vector3(tileSizeUnits, tileSizeUnits, 1f);

				SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
				sr.sprite = whiteSquare;
				sr.color = (x + y) % 2 == 0 ? color1 : color2;
			}
		}
	}

	private void DeletePreviousFlooring() {
		for (int i = parentObject.transform.childCount - 1; i >= 0; i--)
			DestroyImmediate(parentObject.transform.GetChild(i).gameObject);
	}
}