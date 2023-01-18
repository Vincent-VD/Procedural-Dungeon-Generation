using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

public class GenerateCorridor : MonoBehaviour
{

	[SerializeField] private GameObject _corridorTiles;
	[SerializeField] private float _cellSize;

	public GameObject Room1 { get; set; }
	public GameObject Room2 { get; set; }

	private enum RelativeLocation
	{
		BottomLeft,
		BottomRight,
		TopLeft,
		TopRight
	}

	public void Initialize()
	{
		GenerateRoom room1Gnr = Room1.GetComponent<GenerateRoom>();
		GenerateRoom room2Gnr = Room2.GetComponent<GenerateRoom>();
		Vector2 room1Center = (Vector2)Room1.transform.position + room1Gnr.Offset;
		Vector2 room2Center = (Vector2)Room2.transform.position + room2Gnr.Offset;

		Vector2 corner = new Vector2(room2Center.x, room1Center.y);

		Vector2 direction = (corner - room1Center).normalized;

		Vector2 currPos = room1Center;

		float dist = (corner - room1Center).magnitude;

		int maxIter = (int)Math.Ceiling((dist / _cellSize));

		for (int iter = 0; iter < maxIter; iter++)
		{
			if(room2Gnr.IsPointInRoom(currPos))
			{
				break;
			}
			GameObject tile = Instantiate(_corridorTiles);
			tile.transform.position = currPos;
			tile.transform.localScale = new Vector3(_cellSize, _cellSize, 1);
			currPos += (direction * _cellSize);
		}

		direction = (corner - room2Center).normalized;
		currPos = room2Center;
		dist = (corner - room2Center).magnitude;
		maxIter = (int) Math.Ceiling((dist / _cellSize));

		for (int iter = 0; iter < maxIter; iter++)
		{
			if (room2Gnr.IsPointInRoom(currPos))
			{
				break;
			}
			GameObject tile = Instantiate(_corridorTiles);
			tile.transform.position = currPos;
			tile.transform.localScale = new Vector3(_cellSize, _cellSize, 1);
			currPos += (direction * _cellSize);
		}

	}
}
