using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//using Random = Unity.Mathematics.Random;

public class GenerateRoom : MonoBehaviour
{

	[SerializeField] private GameObject _roomTile;
	[SerializeField] private Grid _grid;
	[SerializeField] private int _roomWidth;
	[SerializeField] private int _roomHeight;
	[SerializeField] private float _cellSize;

	GenerateRoom(int roomWidth, int roomHeight)
	{
		_roomWidth = roomWidth;
		_roomHeight = roomHeight;
	}

	// Start is called before the first frame update
	void Start()
	{
		Color roomColor = new Color(Random.value, Random.value, Random.value);
		_grid.cellSize = new Vector3(_cellSize, _cellSize, 1);
		for (int width = 0; width < _roomWidth; width++)
		{
			for (int height = 0; height <= _roomHeight; height++)
			{
				GameObject room = Instantiate(_roomTile);
				room.transform.SetParent(_grid.gameObject.transform, false);
				room.transform.localPosition = new Vector3(width * _grid.cellSize.x, height * _grid.cellSize.y, 0);
				room.transform.localScale = new Vector3(_cellSize, _cellSize, 1);
				room.GetComponent<SpriteRenderer>().color = roomColor;
			}
		}
	}
}
