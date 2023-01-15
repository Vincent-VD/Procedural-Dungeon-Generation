using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateDungeon : MonoBehaviour
{
	[SerializeField] private GameObject _roomTemplate;
	[SerializeField] private int _nrOfRooms = 2;
	[SerializeField] private int _minDimensions = 3;
	[SerializeField] private int _maxDimensions = 8;

	struct Room
	{
		public Vector3 _pos;
		public int _width;
		public int _height;
		public GameObject _room;
	}

	// Start is called before the first frame update
    void Start()
    {
	    for (int iter = 0; iter < _nrOfRooms; ++iter)
	    {
		    Vector3 pos = Random.insideUnitCircle;
		    GameObject room = Instantiate(_roomTemplate);
		    int width = NormalizedRandom(_minDimensions, _maxDimensions);
		    int height = NormalizedRandom(_minDimensions, _maxDimensions);
		    room.GetComponent<GenerateRoom>().InitializeRoom(width, height);
			Debug.Log(width.ToString() + "  " + height.ToString());
			room.transform.position = pos;
			room.transform.SetParent(this.transform);
	    }
	}

    // Update is called once per frame
    void Update()
    {
        
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
}
