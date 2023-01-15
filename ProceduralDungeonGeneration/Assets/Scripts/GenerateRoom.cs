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
	[SerializeField] private Grid _grid;
	[SerializeField] private BoxCollider2D _roomCollider;
	[SerializeField] private float _cellSize;

	//GenerateRoom(int roomWidth, int roomHeight)
	//{
	//	_roomWidth = roomWidth;
	//	_roomHeight = roomHeight;
	//}

	private float timer = 2.0f;
	// Start is called before the first frame update

	void Start()
	{
		Physics2D.simulationMode = SimulationMode2D.Script;
		Physics2D.Simulate(Time.fixedDeltaTime); //Separate rooms
	}

	public void InitializeRoom(int roomWidth, int roomHeight)
	{
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
		_roomCollider.size = new Vector2(_cellSize * (roomHeight + 5), _cellSize * (roomWidth + 5));
		_roomCollider.offset = new Vector2(((_cellSize * (roomHeight)) / 2) - 0.1f, ((_cellSize * (roomWidth + 1)) / 2) - 0.1f);
	}
}
