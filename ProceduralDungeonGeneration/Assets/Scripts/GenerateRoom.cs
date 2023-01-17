using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

//using Random = Unity.Mathematics.Random;

public class GenerateRoom : MonoBehaviour
{

	[SerializeField] private GameObject _roomTile;
	[SerializeField] private BoxCollider2D _roomCollider;
	[SerializeField] private float _cellSize;
	[SerializeField] [MinAttribute(1.0f)] private float _cellSpacing = 1.0f;

	public int Width { get; private set; }
	public int Height { get; private set; }
	public Vector2 Offset { get; private set; }

	// Start is called before the first frame update
	void Start()
	{
		
	}

	public void InitializeRoom(int roomWidth, int roomHeight)
	{
		Width = roomWidth;
		Height = roomHeight;
		Color roomColor = new Color(Random.value, Random.value, Random.value);
		//_grid.cellSize = new Vector3(_cellSize, _cellSize, 1);
		for (int width = 0; width < roomHeight; width++)
		{
			for (int height = 0; height <= roomWidth; height++)
			{
				GameObject room = Instantiate(_roomTile);
				room.transform.SetParent(gameObject.transform, false);
				room.transform.localPosition = new Vector3(width * _cellSize, height * _cellSize, 0);
				room.transform.localScale = new Vector3(_cellSize, _cellSize, 1);
				room.GetComponent<SpriteRenderer>().color = roomColor;
			}
		}
		_roomCollider.size = new Vector2(_cellSize * (roomHeight * _cellSpacing), _cellSize * (roomWidth * _cellSpacing));
		_roomCollider.offset = new Vector2(((_cellSize * (roomHeight)) / 2) - 0.1f, ((_cellSize * (roomWidth + 1)) / 2) - 0.1f);
		
		Offset = new Vector2(((_cellSize * (roomHeight)) / 2) - 0.1f, ((_cellSize * (roomWidth + 1)) / 2) - 0.1f);

	}

	public void SetMainRoom()
	{
		foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
		{
			spriteRenderer.color = new Color(0, 0, 0);
		}
	}

	public void Simulate()
	{
		Physics2D.simulationMode = SimulationMode2D.Script;
		for (int iter = 0; iter < 30; iter++)
		{
			Physics2D.Simulate(Time.fixedDeltaTime); //Separate rooms
		}
	}

}
