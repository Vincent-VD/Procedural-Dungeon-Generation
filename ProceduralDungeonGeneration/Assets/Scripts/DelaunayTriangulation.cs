using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Xml;
using TreeEditor;
using UnityEngine;
using static UnityEditor.Progress;
using Random = UnityEngine.Random;

public class Vertex
{
	public Vector3 position;

	//The outgoing halfedge (a halfedge that starts at this vertex)
	//Doesnt matter which edge we connect to it
	public HalfEdge halfEdge;

	//Which triangle is this vertex a part of?
	public Triangle triangle;

	//The previous and next vertex this vertex is attached to
	public Vertex prevVertex;
	public Vertex nextVertex;

	//Properties this vertex may have
	//Reflex is concave
	public bool isReflex;
	public bool isConvex;
	public bool isEar;

	public Vertex(Vector3 position)
	{
		this.position = position;
	}

	//Get 2d pos of this vertex
	public Vector2 GetPos2D_XZ()
	{
		Vector2 pos_2d_xz = new Vector2(position.x, position.z);

		return pos_2d_xz;
	}
}

public class HalfEdge
{
	//The vertex the edge points to
	public Vertex v;

	//The face this edge is a part of
	public Triangle t;

	//The next edge
	public HalfEdge nextEdge;
	//The previous
	public HalfEdge prevEdge;
	//The edge going in the opposite direction
	public HalfEdge oppositeEdge;

	//This structure assumes we have a vertex class with a reference to a half edge going from that vertex
	//and a face (triangle) class with a reference to a half edge which is a part of this face 
	public HalfEdge(Vertex v)
	{
		this.v = v;
	}
}

public class Triangle
{
	//Corners
	public Vertex v1;
	public Vertex v2;
	public Vertex v3;

	//If we are using the half edge mesh structure, we just need one half edge
	public HalfEdge halfEdge;

	public bool _toErase = false;

	public Triangle(Vertex v1, Vertex v2, Vertex v3)
	{
		this.v1 = v1;
		this.v2 = v2;
		this.v3 = v3;
	}

	public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		this.v1 = new Vertex(v1);
		this.v2 = new Vertex(v2);
		this.v3 = new Vertex(v3);
	}

	public Triangle(HalfEdge halfEdge)
	{
		this.halfEdge = halfEdge;
	}

	//Change orientation of triangle from cw -> ccw or ccw -> cw
	public void ChangeOrientation()
	{
		Vertex temp = this.v1;

	this.v1 = this.v2;

	this.v2 = temp;
	}
}

public class Edge
{
	public Vertex v1;
	public Vertex v2;

	//Is this edge intersecting with another edge?
	public bool isIntersecting = false;

	public Edge(Vertex v1, Vertex v2)
	{
		this.v1 = v1;
		this.v2 = v2;
	}

	public Edge(Vector3 v1, Vector3 v2)
	{
		this.v1 = new Vertex(v1);
		this.v2 = new Vertex(v2);
	}

	//Get vertex in 2d space (assuming x, z)
	public Vector2 GetVertex2D(Vertex v)
	{
		return new Vector2(v.position.x, v.position.z);
	}

	//Flip edge
	public void FlipEdge()
	{
		Vertex temp = v1;

		v1 = v2;

		v2 = temp;
	}
}

public class Plane
{
	public Vector3 pos;

	public Vector3 normal;

	public Plane(Vector3 pos, Vector3 normal)
	{
		this.pos = pos;

		this.normal = normal;
	}
}

public class Connection
{
	public Vector2 p1;

	public Vector2 p2;

	public Connection(Vector2 from, Vector2 to)
	{
		p1 = from;
		p2 = to;
	}

	public bool Equals(Connection other)
	{
		if ((p1.Equals(other.p1) || (p1.Equals(other.p2))) && (p2.Equals(other.p2)) || p2.Equals(other.p1))
		{
			return true;
		}

		return false;
	}
}

//https://en.wikipedia.org/wiki/Cycle_(graph_theory)
// Declares the class for the vertices of the graph
public class Node
{
	public Vector2 pos;
	public GameObject room;
	public HashSet<Node> adjacentNodes = new HashSet<Node>(); // Set of neighbour vertices

	public bool Equals(Node other)
	{
		if (pos.Equals(other.pos))
		{
			return true;
		}
		return false;
	}
}

// Declares the class for the undirected graph
public class UndirectedGraph
{
	public HashSet<Node> nodes = new HashSet<Node>();

	// This method connects node1 and node2 with each other
	public void ConnectNodes(Node node1, Node node2)
	{
		node1.adjacentNodes.Add(node2);
		node2.adjacentNodes.Add(node1);
	}
}


//https://www.habrador.com/tutorials/math/11-delaunay/
public static class DelaunayTriangulation
{
	private static float IsAPointLeftOfVectorOrOnTheLine(Vector2 a, Vector2 b, Vector2 p)
	{
		float determinant = (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);

		return determinant;
	}

	//https://stackoverflow.com/questions/10020949/gift-wrapping-algorithm
	public static List<Vector2> GetConvexHull(List<Vector2> points)
	{
		if (points.Count < 3)
		{
			Debug.LogError("At least 3 points required");
		}

		List<Vector2> hull = new List<Vector2>();

		// get leftmost point
		Vector2 vPointOnHull = points.Where(p => p.x == (points.Min(y => y.x))).First();

		Vector2 vEndpoint;
		while (true)
		{
			hull.Add(vPointOnHull);
			vEndpoint = points[0];

			for (int i = 1; i < points.Count; i++)
			{
				if ((vPointOnHull == vEndpoint)
				    || (Orientation(vPointOnHull, vEndpoint, points[i]) < 0))
				{
					vEndpoint = points[i];
				}
			}

			vPointOnHull = vEndpoint;
			//Break condition -- if we've looped back around then we've made a convex hull!
			if (vEndpoint == hull[0])
				break;
		}
		//while (vEndpoint != hull[0]);
		hull.Add(hull[0]);

		return hull;
	}

	//https://www.habrador.com/tutorials/math/10-triangulation/
	public static List<Triangle> TriangulateConvexPolygon(List<Vector2> convexHullpoints)
	{
		List<Triangle> triangles = new List<Triangle>();

		for (int i = 2; i < convexHullpoints.Count; i++)
		{
			Vector2 a = convexHullpoints[0];
			Vector2 b = convexHullpoints[i - 1];
			Vector2 c = convexHullpoints[i];

			triangles.Add(new Triangle(a, b, c));
		}

		return triangles;
	}

	public static bool IsPointInTriangle(Vector2 point, Triangle triangle)
	{
		Vector3 p1 = triangle.v1.position;
		Vector3 p2 = triangle.v2.position;
		Vector3 p3 = triangle.v3.position;

		if (point == (Vector2)p1 || point == (Vector2)p2 || point == (Vector2)p3)
		{
			return false;
		}

		//Based on Barycentric coordinates
		float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

		float a = ((p2.y - p3.y) * (point.x - p3.x) + (p3.x - p2.x) * (point.y - p3.y)) / denominator;
		float b = ((p3.y - p1.y) * (point.x - p3.x) + (p1.x - p3.x) * (point.y - p3.y)) / denominator;
		float c = 1 - a - b;

		//The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
		//if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
		//{
		//    isWithinTriangle = true;
		//}

		//The point is within the triangle
		if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
		{
			return true;
		}

		return false;
	}

	//Orient triangles so they have the correct orientation
	public static void OrientTrianglesClockwise(List<Triangle> triangles)
	{
		for (int i = 0; i < triangles.Count; i++)
		{
			Triangle tri = triangles[i];

			Vector2 v1 = new Vector2(tri.v1.position.x, tri.v1.position.z);
			Vector2 v2 = new Vector2(tri.v2.position.x, tri.v2.position.z);
			Vector2 v3 = new Vector2(tri.v3.position.x, tri.v3.position.z);

			if (!IsTriangleOrientedClockwise(v1, v2, v3))
			{
				tri.ChangeOrientation();
			}
		}
	}

	//Is a triangle in 2d space oriented clockwise or counter-clockwise
	//https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
	//https://en.wikipedia.org/wiki/Curve_orientation
	public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		bool isClockWise = true;

		float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

		if (determinant > 0f)
		{
			isClockWise = false;
		}

		return isClockWise;
	}

	//From triangle where each triangle has one vertex to half edge
	public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles)
	{
		//Make sure the triangles have the same orientation
		OrientTrianglesClockwise(triangles);

		//First create a list with all possible half-edges
		List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

		for (int i = 0; i < triangles.Count; i++)
		{
			Triangle t = triangles[i];

			HalfEdge he1 = new HalfEdge(t.v1);
			HalfEdge he2 = new HalfEdge(t.v2);
			HalfEdge he3 = new HalfEdge(t.v3);

			he1.nextEdge = he2;
			he2.nextEdge = he3;
			he3.nextEdge = he1;

			he1.prevEdge = he3;
			he2.prevEdge = he1;
			he3.prevEdge = he2;

			//The vertex needs to know of an edge going from it
			he1.v.halfEdge = he2;
			he2.v.halfEdge = he3;
			he3.v.halfEdge = he1;

			//The face the half-edge is connected to
			t.halfEdge = he1;

			he1.t = t;
			he2.t = t;
			he3.t = t;

			//Add the half-edges to the list
			halfEdges.Add(he1);
			halfEdges.Add(he2);
			halfEdges.Add(he3);
		}

		//Find the half-edges going in the opposite direction
		for (int i = 0; i < halfEdges.Count; i++)
		{
			HalfEdge he = halfEdges[i];

			Vertex goingToVertex = he.v;
			Vertex goingFromVertex = he.prevEdge.v;

			for (int j = 0; j < halfEdges.Count; j++)
			{
				//Dont compare with itself
				if (i == j)
				{
					continue;
				}

				HalfEdge heOpposite = halfEdges[j];

				//Is this edge going between the vertices in the opposite direction
				if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position)
				{
					he.oppositeEdge = heOpposite;

					break;
				}
			}
		}


		return halfEdges;
	}

	//Is a point d inside, outside or on the same circle as a, b, c
	//https://gamedev.stackexchange.com/questions/71328/how-can-i-add-and-subtract-convex-polygons
	//Returns positive if inside, negative if outside, and 0 if on the circle
	public static float IsPointInsideOutsideOrOnCircle(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec)
	{
		//This first part will simplify how we calculate the determinant
		float a = aVec.x - dVec.x;
		float d = bVec.x - dVec.x;
		float g = cVec.x - dVec.x;

		float b = aVec.y - dVec.y;
		float e = bVec.y - dVec.y;
		float h = cVec.y - dVec.y;

		float c = a * a + b * b;
		float f = d * d + e * e;
		float i = g * g + h * h;

		float determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);

		return determinant;
	}

	//Is a quadrilateral convex? Assume no 3 points are colinear and the shape doesnt look like an hourglass
	public static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		bool isConvex = false;

		bool abc = IsTriangleOrientedClockwise(a, b, c);
		bool abd = IsTriangleOrientedClockwise(a, b, d);
		bool bcd = IsTriangleOrientedClockwise(b, c, d);
		bool cad = IsTriangleOrientedClockwise(c, a, d);

		if (abc && abd && bcd & !cad)
		{
			isConvex = true;
		}
		else if (abc && abd && !bcd & cad)
		{
			isConvex = true;
		}
		else if (abc && !abd && bcd & cad)
		{
			isConvex = true;
		}
		//The opposite sign, which makes everything inverted
		else if (!abc && !abd && !bcd & cad)
		{
			isConvex = true;
		}
		else if (!abc && !abd && bcd & !cad)
		{
			isConvex = true;
		}
		else if (!abc && abd && !bcd & !cad)
		{
			isConvex = true;
		}


		return isConvex;
	}

	public static List<Triangle> TriangulateByFlippingEdges(List<Vector2> roomVerts)
	{
		List<Vector2> hullVerts = DelaunayTriangulation.GetConvexHull(roomVerts);
		List<Triangle> triangles = DelaunayTriangulation.TriangulateConvexPolygon(hullVerts);

		for (int iter = 0; iter < roomVerts.Count; ++iter)
		{
			Vector2 currPoint = roomVerts[iter];

			for (int inner = 0; inner < +triangles.Count; ++inner)
			{
				Triangle triangle = triangles[inner];

				if (DelaunayTriangulation.IsPointInTriangle(currPoint, triangle))
				{
					Vector3 p1 = triangle.v1.position;
					Vector3 p2 = triangle.v2.position;
					Vector3 p3 = triangle.v3.position;

					Triangle t1 = new Triangle(p1, p2, currPoint);
					Triangle t2 = new Triangle(p1, p3, currPoint);
					Triangle t3 = new Triangle(p2, p3, currPoint);

					triangles.Remove(triangle);

					triangles.Add(t1);
					triangles.Add(t2);
					triangles.Add(t3);
					break;
				}
			}
		}

		List<HalfEdge> halfEdges = TransformFromTriangleToHalfEdge(triangles);

		int safety = 0;

		int flippedEdges = 0;

		while (true)
		{
			safety += 1;

			if (safety > 100000)
			{
				Debug.Log("Stuck in endless loop");

				break;
			}

			bool hasFlippedEdge = false;

			//Search through all edges to see if we can flip an edge
			for (int i = 0; i < halfEdges.Count; i++)
			{
				HalfEdge thisEdge = halfEdges[i];

				//Is this edge sharing an edge, otherwise its a border, and then we cant flip the edge
				if (thisEdge.oppositeEdge == null)
				{
					continue;
				}

				//The vertices belonging to the two triangles, c-a are the edge vertices, b belongs to this triangle
				Vertex a = thisEdge.v;
				Vertex b = thisEdge.nextEdge.v;
				Vertex c = thisEdge.prevEdge.v;
				Vertex d = thisEdge.oppositeEdge.nextEdge.v;

				Vector2 aPos = a.GetPos2D_XZ();
				Vector2 bPos = b.GetPos2D_XZ();
				Vector2 cPos = c.GetPos2D_XZ();
				Vector2 dPos = d.GetPos2D_XZ();

				//Use the circle test to test if we need to flip this edge
				if (IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
				{
					//Are these the two triangles that share this edge forming a convex quadrilateral?
					//Otherwise the edge cant be flipped
					if (IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
					{
						//If the new triangle after a flip is not better, then dont flip
						//This will also stop the algoritm from ending up in an endless loop
						if (IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
						{
							continue;
						}

						//Flip the edge
						flippedEdges += 1;

						hasFlippedEdge = true;

						FlipEdge(thisEdge);
					}
				}
			}

			//We have searched through all edges and havent found an edge to flip, so we have a Delaunay triangulation!
			if (!hasFlippedEdge)
			{
				//Debug.Log("Found a delaunay triangulation");

				break;
			}
		}

		//Debug.Log("Flipped edges: " + flippedEdges);

		//Dont have to convert from half edge to triangle because the algorithm will modify the objects, which belongs to the 
		//original triangles, so the triangles have the data we need

		return triangles;

	}

	//Flip an edge
	private static void FlipEdge(HalfEdge one)
	{
		//The data we need
		//This edge's triangle
		HalfEdge two = one.nextEdge;
		HalfEdge three = one.prevEdge;
		//The opposite edge's triangle
		HalfEdge four = one.oppositeEdge;
		HalfEdge five = one.oppositeEdge.nextEdge;
		HalfEdge six = one.oppositeEdge.prevEdge;
		//The vertices
		Vertex a = one.v;
		Vertex b = one.nextEdge.v;
		Vertex c = one.prevEdge.v;
		Vertex d = one.oppositeEdge.nextEdge.v;
		//Flip

		//Change vertex
		a.halfEdge = one.nextEdge;
		c.halfEdge = one.oppositeEdge.nextEdge;

		//Change half-edge
		//Half-edge - half-edge connections
		one.nextEdge = three;
		one.prevEdge = five;

		two.nextEdge = four;
		two.prevEdge = six;

		three.nextEdge = five;
		three.prevEdge = one;

		four.nextEdge = six;
		four.prevEdge = two;

		five.nextEdge = one;
		five.prevEdge = three;

		six.nextEdge = two;
		six.prevEdge = four;

		//Half-edge - vertex connection
		one.v = b;
		two.v = b;
		three.v = c;
		four.v = d;
		five.v = d;
		six.v = a;

		//Half-edge - triangle connection
		Triangle t1 = one.t;
		Triangle t2 = four.t;

		one.t = t1;
		three.t = t1;
		five.t = t1;

		two.t = t2;
		four.t = t2;
		six.t = t2;

		//Opposite-edges are not changing!

		//Triangle connection
		t1.v1 = b;
		t1.v2 = c;
		t1.v3 = d;

		t2.v1 = b;
		t2.v2 = d;
		t2.v3 = a;

		t1.halfEdge = three;
		t2.halfEdge = four;
	}

	private static float Orientation(Vector2 p1, Vector2 p2, Vector2 p)
	{
		//Determinant
		return (p2.x - p1.x) * (p.y - p1.y) - (p.x - p1.x) * (p2.y - p1.y);
	}

	public static List<Connection> GenerateMST(List<Triangle> triangles, List<Vector2> vertices)
	{
		List<Connection> mst = new List<Connection>();

		Triangle t = triangles[0];
		Connection conn1 = new Connection(t.v1.position, t.v2.position);
		Connection conn2 = new Connection(t.v1.position, t.v3.position);
		Connection conn3 = new Connection(t.v2.position, t.v3.position);
		mst.Add(conn1);
		mst.Add(conn2);
		mst.Add(conn3);

		//Create connections
		for (int iter = 1; iter < triangles.Count; iter++)
		{
			Triangle triangle = triangles[iter];
			Connection c1 = new Connection(triangle.v1.position, triangle.v2.position);
			Connection c2 = new Connection(triangle.v1.position, triangle.v3.position);
			Connection c3 = new Connection(triangle.v2.position, triangle.v3.position);

			bool[] markForAdd = {true, true, true };

			for (int inner = 0; inner < mst.Count; inner++)
			{
				Connection connection = mst[inner];
				if (connection.Equals(c1))
				{
					markForAdd[0] = false;
				}
				if (connection.Equals(c2))
				{
					markForAdd[1] = false;
				}
				if (connection.Equals(c3))
				{
					markForAdd[2] = false;
				}
			}

			if (markForAdd[0])
			{
				mst.Add(c1);
			}

			if (markForAdd[1])
			{
				mst.Add(c2);
			}

			if (markForAdd[2])
			{
				mst.Add(c3);
			}
		}
		UndirectedGraph fullGraph = new UndirectedGraph();
		UndirectedGraph res = new UndirectedGraph();
		UndirectedGraph part = new UndirectedGraph();
		foreach (var vert in vertices)
		{
			Node newNode = new Node();
			newNode.pos = vert;
			fullGraph.nodes.Add(newNode);
			res.nodes.Add(newNode);
			part.nodes.Add(newNode);
		}

		foreach (var connection in mst)
		{
			Vector2 pos1 = connection.p1;
			Vector2 pos2 = connection.p2;

			Node from = new Node();
			Node to = new Node();

			foreach (var node in fullGraph.nodes)
			{
				if (node.pos.Equals(pos1))
				{
					from = node;
				}

				if (node.pos.Equals(pos2))
				{
					to = node;
				}
			}

			if (!from.pos.Equals(Vector2.zero) && !to.pos.Equals(Vector2.zero))
			{
				fullGraph.ConnectNodes(from, to);
			}
		}

		foreach (var from in fullGraph.nodes)
		{
			foreach (var to in from.adjacentNodes)
			{
				part.ConnectNodes(from, to);
				if (CheckForLoop(part))
				{
					//res.ConnectNodes(from, to);
					part = res; //commit
				}
				else
				{
					//res.ConnectNodes(from, to);
					res = part; //reset
				}
			}
		}

		List<Connection> mstRes = new List<Connection>();

		foreach (var from in res.nodes)
		{
			foreach (var to in from.adjacentNodes)
			{
				Connection con = new Connection(from.pos, to.pos);
				mstRes.Add(con);
			}
		}

		return mstRes;
	}

	//https://en.wikipedia.org/wiki/Cycle_(graph_theory)
	private static bool CheckForLoop(UndirectedGraph graph)
	{
		HashSet<Node> newNodes = new HashSet<Node>(graph.nodes); // Set of new vertices to iterate
		HashSet<List<Node>> paths = new HashSet<List<Node>>(); // Set of current paths
		for (int i = 0; i < graph.nodes.Count; i++) // for-loop, iterating all vertices of the graph
		{
			Node node = graph.nodes.ElementAt(i);
			newNodes.Add(node); // Add the vertex to the set of new vertices to iterate
			List<Node> path = new List<Node>();
			path.Add(node);
			paths.Add(path); // Adds a path for each node as a starting vertex
		}
		HashSet<List<Node>> shortestCycles = new HashSet<List<Node>>(); // Set of shortest cycles
		int lengthOfCycles = 0; // Length of shortest cycles
		bool cyclesAreFound = false; // Whether or not cycles were found at all
		while (!cyclesAreFound && newNodes.Count > 0) // As long as we still had vertices to iterate
		{
			newNodes.Clear(); // Empties the set of nodes to iterate
			HashSet<List<Node>> newPaths = new HashSet<List<Node>>(); // Set of newly found paths
			foreach (List<Node> path in paths) // foreach-loop, iterating all current paths
			{
				Node lastNode = path[path.Count - 1];
				newNodes.Add(lastNode); // Adds the final vertex of the path to the list of vertices to iterate
				foreach (Node nextNode in lastNode.adjacentNodes) // foreach-loop, iterating all neighbours of the previous node
				{
					if (path.Count >= 3 && path[0] == nextNode) // If a cycle with length greater or equal 3 was found
					{
						cyclesAreFound = true;
						shortestCycles.Add(path); // Adds the path to the set of cycles
						lengthOfCycles = path.Count;
					}
					if (!path.Contains(nextNode)) // If the path doesn't contain the neighbour
					{
						newNodes.Add(nextNode); // Adds the neighbour to the set of vertices to iterate
												// Creates a new path
						List<Node> newPath = new List<Node>();
						newPath.AddRange(path); // Adds the current path's vertex to the new path in the correct order
						newPath.Add(nextNode); // Adds the neighbour to the new path
						newPaths.Add(newPath); // Adds the path to the set of newly found paths
					}
				}
			}
			paths = newPaths; // Updates the set of current paths
		}

		return cyclesAreFound;
	}
}
