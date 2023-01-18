using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Mathematics;
using UnityEditor.SearchService;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GenerateDungeon : MonoBehaviour
{
	[SerializeField] private GameObject _roomTemplate;
	[SerializeField] private GameObject _corridorTemplate;
	[SerializeField] [MinAttribute(0)] private int _nrOfRooms = 2;
	[SerializeField] [MinAttribute(0)] private int _minDimensions = 3;
	[SerializeField] [MinAttribute (0)] private int _maxDimensions = 8;
	[SerializeField] [Range (0,1)] private float _ellipseHeight = 1.0f;
	[SerializeField] private float _mainRoomSelectionThreshold = 1.0f;

	private GameObject[] _rooms;
	private List<GameObject> _mainRooms = new List<GameObject>();
	private List<GameObject> _otherRooms = new List<GameObject>();
	private float _meanWidth = 0.0f;
	private float _meanHeight = 0.0f;
	private List<Vector2> _mainRoomCenters = new List<Vector2>();
	private List<Vector2> _hullVerts = new List<Vector2>();
	private List<Triangle> _triangles = new List<Triangle>();
	private List<Connection> _connections = new List<Connection>();
	private List<Connection> _connections2 = new List<Connection>();
	private List<Vector2> _dungeon = new List<Vector2>();

	// Start is called before the first frame update
	void Start()
	{
		_rooms = new GameObject[_nrOfRooms];
		for (int iter = 0; iter < _nrOfRooms; ++iter)
		{
			Vector3 pos = Random.insideUnitCircle;
			pos.y *= _ellipseHeight;
			GameObject room = Instantiate(_roomTemplate);
			int width = NormalizedRandom(_minDimensions, _maxDimensions);
			int height = NormalizedRandom(_minDimensions, _maxDimensions);
			room.GetComponent<GenerateRoom>().InitializeRoom(width, height);
			//Debug.Log(width.ToString() + "  " + height.ToString());
			room.transform.position = pos;
			room.transform.SetParent(this.transform);
			_rooms[iter] = room;
			_meanWidth += (float) width;
			_meanHeight += (float) height;
		}

		foreach (var room in _rooms)
		{
			room.GetComponent<GenerateRoom>().Simulate();
		}

		_meanWidth /= _nrOfRooms;
		_meanHeight /= _nrOfRooms;

		//Select main rooms
		foreach (var room in _rooms)
		{
			GenerateRoom roomGnr = room.GetComponent<GenerateRoom>();
			if (roomGnr.Width >= _mainRoomSelectionThreshold * _meanWidth &&
				roomGnr.Height >= _mainRoomSelectionThreshold * _meanHeight)
			{
				_mainRooms.Add(room);
				roomGnr.SetMainRoom();
			}
			else
			{
				_otherRooms.Add(room);
			}
		}

		foreach (var room in _mainRooms)
		{
			GenerateRoom roomGnr = room.GetComponent<GenerateRoom>();
			_mainRoomCenters.Add((Vector2)room.transform.position + roomGnr.Offset);
		}

		//_hullVerts = DelaunayTriangulation.GetConvexHull(_mainRoomCenters);
		//_triangles = DelaunayTriangulation.TriangulateConvexPolygon(_hullVerts);

		//for (int iter = 0; iter < _mainRoomCenters.Count; ++iter)
		//{
		//	Vector2 currPoint = _mainRoomCenters[iter];

		//	for (int inner = 0; inner < +_triangles.Count; ++inner)
		//	{
		//		Triangle triangle = _triangles[inner];

		//		if (DelaunayTriangulation.IsPointInTriangle(currPoint, triangle))
		//		{
		//			Vector3 p1 = triangle.v1.position;
		//			Vector3 p2 = triangle.v2.position;
		//			Vector3 p3 = triangle.v3.position;

		//			Triangle t1 = new Triangle(p1, p2, currPoint);
		//			Triangle t2 = new Triangle(p1, p3, currPoint);
		//			Triangle t3 = new Triangle(p2, p3, currPoint);

		//			_triangles.Remove(triangle);

		//			_triangles.Add(t1);
		//			_triangles.Add(t2);
		//			_triangles.Add(t3);
		//			break;
		//		}
		//	}
		//}

		_triangles = DelaunayTriangulation.TriangulateByFlippingEdges(_mainRoomCenters);

		_connections = DelaunayTriangulation.GenerateMST(_triangles, _mainRoomCenters);

		foreach (var connection in _connections)
		{
			int roomIter1 = _mainRoomCenters.FindIndex(v => v.Equals(connection.p1));
			int roomIter2 = _mainRoomCenters.FindIndex(v => v.Equals(connection.p2));

			GameObject room1 = _mainRooms[roomIter1];
			GameObject room2 = _mainRooms[roomIter2];

			GameObject corridor = Instantiate(_corridorTemplate);
			GenerateCorridor generateCorridor = corridor.GetComponent<GenerateCorridor>();
			generateCorridor.Room1 = room1;
			generateCorridor.Room2 = room2;
			generateCorridor.Initialize();
			
		}

		for (int iter = 0; iter < _rooms.Length; ++iter)
		{
			if (!_mainRooms.Contains(_rooms[iter]))
			{
				Destroy(_rooms[iter]);
			}
		}

	}

	// Update is called once per frame
	void Update()
	{
		//foreach (var triangle in _triangles)
		//{
		//	Vector3 p1 = triangle.v1.position;
		//	Vector3 p2 = triangle.v2.position;
		//	Vector3 p3 = triangle.v3.position;

		//	Debug.DrawLine(p1, p2, Color.red);
		//	Debug.DrawLine(p1, p3, Color.red);
		//	Debug.DrawLine(p2, p3, Color.red);
		//}

		foreach (var curr in _connections)
		{
			Debug.DrawLine(curr.p1, curr.p2, Color.red);
		}
	}

	int NormalizedRandom(int minVal, int maxVal)
	{
		float mean = (minVal + maxVal) / 2.0f;
		float stdDev = (maxVal - mean) / 3.0f;
		return (int)NextGaussianDouble(mean, stdDev);
	}

	//Marsaglia polar method
	public static float NextGaussianDouble(float mean, float stdDev)
	{
		float u, v, S;

		do
		{
			u = 2.0f * Random.value - 1.0f;
			v = 2.0f * Random.value - 1.0f;
			S = u * u + v * v;
		}
		while (S >= 1.0);

		float fac = (float)Math.Sqrt(-2.0 * Math.Log(S) / S);
		return mean + stdDev * u * fac;
	}

	enum RelativeLocation
	{
		BottomLeft,
		BottomRight,
		TopLeft,
		TopRight
	}

	private void ConnectRooms(GameObject room1, GameObject room2)
	{
		GenerateRoom room1Gnr = room1.GetComponent<GenerateRoom>();
		GenerateRoom room2Gnr = room2.GetComponent<GenerateRoom>();
		Vector2 room1Center = (Vector2) room1.transform.position + room1Gnr.Offset;
		Vector2 room2Center = (Vector2) room2.transform.position + room2Gnr.Offset;

		Vector2 corner = new Vector2(room2Center.x, room1Center.y);

		Connection con1 = new Connection(room1Center, corner);
		Connection con2 = new Connection(room2Center, corner);

		_connections2.Add(con1);
		_connections2.Add(con2);

		//switch (CheckRelativeLocation(room1Center, room2Center))
		//{
		//	case RelativeLocation.BottomLeft:
		//	{
		//		corner = new Vector2(room2Center.x, room1Center.y);
		//		break;
		//	};
		//	case RelativeLocation.BottomRight:
		//	{
		//		corner = new Vector2(room2Center.x, room1Center.y);
		//		break;
		//	};
		//	case RelativeLocation.TopLeft:
		//	{
		//		corner = new Vector2(room2Center.x, room1Center.y);
		//		break;
		//	};
		//	case RelativeLocation.TopRight:
		//	{
		//		corner = new Vector2(room2Center.x, room1Center.y);
		//		break;
		//	};
		//}

	}

	private RelativeLocation CheckRelativeLocation(Vector2 room1, Vector2 room2)
	{
		RelativeLocation res = RelativeLocation.BottomLeft;
		if (room2.x < room1.x)
		{
			if (room2.y < room1.y)
			{
				res =  RelativeLocation.BottomLeft;
			}
			else res = RelativeLocation.TopLeft;
		}

		if (room2.x > room1.x)
		{
			if (room2.y < room1.y)
			{
				res = RelativeLocation.BottomRight;
			}
			else res = RelativeLocation.TopRight;
		}

		return res;
	}

}
