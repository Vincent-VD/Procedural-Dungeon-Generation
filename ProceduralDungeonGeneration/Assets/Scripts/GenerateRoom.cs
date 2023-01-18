using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
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

	public bool IsPointInRoom(Vector2 point)
	{
		Vector2 pos = transform.position;
		Vector2 bottomLeft = pos;
		Vector2 topRight = new Vector2(pos.x + Height * _cellSize, pos.y + Width * _cellSize);
		if (point.x >= bottomLeft.y && point.y >= bottomLeft.y && point.x < topRight.x && point.y < topRight.y)
		{
			return true;
		}
		return false;
	}

	public bool CheckIfRoomIntersects(Connection connection)
	{
		Vector2 pos1 = connection.p1;
		Vector2 pos2 = connection.p2;

		Vector2 pos = transform.position;
		Vector2 bottomLeft = pos;
		Vector2 topLeft = new Vector2(pos.x + Height * _cellSize, pos.y);
		Vector2 bottomRight = new Vector2(pos.x, pos.y + Width * _cellSize);
		Vector2 topRight = new Vector2(pos.x + Height * _cellSize, pos.y + Width * _cellSize);


		// check if the line has hit any of the rectangle's sides
		// uses the Line/Line function below
		bool left = LineLineIntersect(pos1.x, pos1.y, pos2.x, pos2.y, bottomLeft.x, bottomLeft.y, topLeft.x, topLeft.y);
		bool right = LineLineIntersect(pos1.x, pos1.y, pos2.x, pos2.y, bottomRight.x, bottomRight.y, topRight.x, topRight.y);
		bool top = LineLineIntersect(pos1.x, pos1.y, pos2.x, pos2.y, topLeft.x, topLeft.y, topRight.x, topRight.y);
		bool bottom = LineLineIntersect(pos1.x, pos1.y, pos2.x, pos2.y, bottomLeft.x, bottomLeft.y, bottomRight.x, bottomRight.y);

		return left || right || top || bottom;
	}


	private bool LineLineIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
	{
		// calculate the direction of the lines
		float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
		float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

		// if uA and uB are between 0-1, lines are colliding
		if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
		{

			// optionally, draw a circle where the lines meet
			float intersectionX = x1 + (uA * (x2 - x1));
			float intersectionY = y1 + (uA * (y2 - y1));

			return true;
		}
		return false;
	}

}
