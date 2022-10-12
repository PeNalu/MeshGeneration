using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangulate : MonoBehaviour
{
	public static List<Triangle> TriangulateConvexPolygon(List<Vertex> convexHullpoints)
	{
		List<Triangle> triangles = new List<Triangle>();

		for (int i = 2; i < convexHullpoints.Count; i++)
		{
			Vertex a = convexHullpoints[0];
			Vertex b = convexHullpoints[i - 1];
			Vertex c = convexHullpoints[i];

			triangles.Add(new Triangle(a, b, c));
		}

		return triangles;
	}

	//This assumes that we have a polygon and now we want to triangulate it
	//The points on the polygon should be ordered counter-clockwise
	//This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
	//One can maybe do it in O(n) time but no such version is known
	//Assumes we have at least 3 points
	public static List<Triangle> TriangulateConcavePolygon(List<Vector3> points)
	{
		//The list with triangles the method returns
		List<Triangle> triangles = new List<Triangle>();

		//If we just have three points, then we dont have to do all calculations
		if (points.Count == 3)
		{
			triangles.Add(new Triangle(points[0], points[1], points[2]));

			return triangles;
		}



		//Step 1. Store the vertices in a list and we also need to know the next and prev vertex
		List<Vertex> vertices = new List<Vertex>();

		for (int i = 0; i < points.Count; i++)
		{
			vertices.Add(new Vertex(points[i]));
		}

		//Find the next and previous vertex
		for (int i = 0; i < vertices.Count; i++)
		{
			int nextPos = Geometry.ClampListIndex(i + 1, vertices.Count);

			int prevPos = Geometry.ClampListIndex(i - 1, vertices.Count);

			vertices[i].prevVertex = vertices[prevPos];

			vertices[i].nextVertex = vertices[nextPos];
		}



		//Step 2. Find the reflex (concave) and convex vertices, and ear vertices
		for (int i = 0; i < vertices.Count; i++)
		{
			CheckIfReflexOrConvex(vertices[i]);
		}

		//Have to find the ears after we have found if the vertex is reflex or convex
		List<Vertex> earVertices = new List<Vertex>();

		for (int i = 0; i < vertices.Count; i++)
		{
			IsVertexEar(vertices[i], vertices, earVertices);
		}



		//Step 3. Triangulate!
		while (true)
		{
			//This means we have just one triangle left
			if (vertices.Count == 3)
			{
				//The final triangle
				triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

				break;
			}

			//Make a triangle of the first ear
			Vertex earVertex = earVertices[0];

			Vertex earVertexPrev = earVertex.prevVertex;
			Vertex earVertexNext = earVertex.nextVertex;

			Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

			triangles.Add(newTriangle);

			//Remove the vertex from the lists
			earVertices.Remove(earVertex);

			vertices.Remove(earVertex);

			//Update the previous vertex and next vertex
			earVertexPrev.nextVertex = earVertexNext;
			earVertexNext.prevVertex = earVertexPrev;

			//...see if we have found a new ear by investigating the two vertices that was part of the ear
			CheckIfReflexOrConvex(earVertexPrev);
			CheckIfReflexOrConvex(earVertexNext);

			earVertices.Remove(earVertexPrev);
			earVertices.Remove(earVertexNext);

			IsVertexEar(earVertexPrev, vertices, earVertices);
			IsVertexEar(earVertexNext, vertices, earVertices);
		}

		//Debug.Log(triangles.Count);

		return triangles;
	}



	//Check if a vertex if reflex or convex, and add to appropriate list
	private static void CheckIfReflexOrConvex(Vertex v)
	{
		v.isReflex = false;
		v.isConvex = false;

		//This is a reflex vertex if its triangle is oriented clockwise
		Vector2 a = v.prevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.nextVertex.GetPos2D_XZ();

		if (Triangle.IsTriangleOrientedClockwise(a, b, c))
		{
			v.isReflex = true;
		}
		else
		{
			v.isConvex = true;
		}
	}



	//Check if a vertex is an ear
	private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
	{
		//A reflex vertex cant be an ear!
		if (v.isReflex)
		{
			return;
		}

		//This triangle to check point in triangle
		Vector2 a = v.prevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.nextVertex.GetPos2D_XZ();

		bool hasPointInside = false;

		for (int i = 0; i < vertices.Count; i++)
		{
			//We only need to check if a reflex vertex is inside of the triangle
			if (vertices[i].isReflex)
			{
				Vector2 p = vertices[i].GetPos2D_XZ();

				//This means inside and not on the hull
				if (TriangleTriangleIntersection.IsPointInTriangle(a, b, c, p))
				{
					hasPointInside = true;

					break;
				}
			}
		}

		if (!hasPointInside)
		{
			earVertices.Add(v);
		}
	}


	public static List<Triangle> SplittingTriangulatePoints(List<Vertex> points)
	{
		//Generate the convex hull - will also remove the points from points list which are not on the hull
		List<Vertex> pointsOnConvexHull = JarvisMarchAlgorithm.GetConvexHull(points);

		//Triangulate the convex hull
		List<Triangle> triangles = TriangulateConvexPolygon(pointsOnConvexHull);

		//Add the remaining points and split the triangles
		for (int i = 0; i < points.Count; i++)
		{
			Vertex currentPoint = points[i];

			//2d space
			Vector2 p = new Vector2(currentPoint.position.x, currentPoint.position.z);

			//Which triangle is this point in?
			for (int j = 0; j < triangles.Count; j++)
			{
				Triangle t = triangles[j];

				Vector2 p1 = new Vector2(t.v1.position.x, t.v1.position.z);
				Vector2 p2 = new Vector2(t.v2.position.x, t.v2.position.z);
				Vector2 p3 = new Vector2(t.v3.position.x, t.v3.position.z);

				if (TriangleTriangleIntersection.IsPointInTriangle(p1, p2, p3, p))
				{
					//Create 3 new triangles
					Triangle t1 = new Triangle(t.v1, t.v2, currentPoint);
					Triangle t2 = new Triangle(t.v2, t.v3, currentPoint);
					Triangle t3 = new Triangle(t.v3, t.v1, currentPoint);

					//Remove the old triangle
					triangles.Remove(t);

					//Add the new triangles
					triangles.Add(t1);
					triangles.Add(t2);
					triangles.Add(t3);

					break;
				}
			}
		}

		return triangles;
	}

	public static List<Triangle> IncrementalTriangulatePoints(List<Vertex> points)
	{
		List<Triangle> triangles = new List<Triangle>();

		//Sort the points along x-axis
		//OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
		points = points.OrderBy(n => n.position.x).ToList();

		//The first 3 vertices are always forming a triangle
		Triangle newTriangle = new Triangle(points[0].position, points[1].position, points[2].position);

		triangles.Add(newTriangle);

		//All edges that form the triangles, so we have something to test against
		List<Edge> edges = new List<Edge>();

		edges.Add(new Edge(newTriangle.v1, newTriangle.v2));
		edges.Add(new Edge(newTriangle.v2, newTriangle.v3));
		edges.Add(new Edge(newTriangle.v3, newTriangle.v1));

		//Add the other triangles one by one
		//Starts at 3 because we have already added 0,1,2
		for (int i = 3; i < points.Count; i++)
		{
			Vector3 currentPoint = points[i].position;

			//The edges we add this loop or we will get stuck in an endless loop
			List<Edge> newEdges = new List<Edge>();

			//Is this edge visible? We only need to check if the midpoint of the edge is visible 
			for (int j = 0; j < edges.Count; j++)
			{
				Edge currentEdge = edges[j];

				Vector3 midPoint = (currentEdge.v1.position + currentEdge.v2.position) / 2f;

				Edge edgeToMidpoint = new Edge(currentPoint, midPoint);

				//Check if this line is intersecting
				bool canSeeEdge = true;

				for (int k = 0; k < edges.Count; k++)
				{
					//Dont compare the edge with itself
					if (k == j)
					{
						continue;
					}

					if (AreEdgesIntersecting(edgeToMidpoint, edges[k]))
					{
						canSeeEdge = false;

						break;
					}
				}

				//This is a valid triangle
				if (canSeeEdge)
				{
					Edge edgeToPoint1 = new Edge(currentEdge.v1, new Vertex(currentPoint));
					Edge edgeToPoint2 = new Edge(currentEdge.v2, new Vertex(currentPoint));

					newEdges.Add(edgeToPoint1);
					newEdges.Add(edgeToPoint2);

					Triangle newTri = new Triangle(edgeToPoint1.v1, edgeToPoint1.v2, edgeToPoint2.v1);

					triangles.Add(newTri);
				}
			}


			for (int j = 0; j < newEdges.Count; j++)
			{
				edges.Add(newEdges[j]);
			}
		}


		return triangles;
	}

	//Alternative 1. Triangulate with some algorithm - then flip edges until we have a delaunay triangulation
	public static List<Triangle> TriangulateByFlippingEdges(List<Vector3> sites)
	{
		//Step 1. Triangulate the points with some algorithm
		//Vector3 to vertex
		List<Vertex> vertices = new List<Vertex>();

		for (int i = 0; i < sites.Count; i++)
		{
			vertices.Add(new Vertex(sites[i]));
		}

		//Triangulate the convex hull of the sites
		List<Triangle> triangles = IncrementalTriangulatePoints(vertices);
		//List triangles = TriangulatePoints.TriangleSplitting(vertices);

		//Step 2. Change the structure from triangle to half-edge to make it faster to flip edges
		List<HalfEdge> halfEdges = TransformFromTriangleToHalfEdge(triangles);

		//Step 3. Flip edges until we have a delaunay triangulation
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
				if (Triangle.IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
				{
					//Are these the two triangles that share this edge forming a convex quadrilateral?
					//Otherwise the edge cant be flipped
					if (Triangle.IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
					{
						//If the new triangle after a flip is not better, then dont flip
						//This will also stop the algoritm from ending up in an endless loop
						if (Triangle.IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
						{
							continue;
						}

						//Flip the edge
						flippedEdges += 1;

						hasFlippedEdge = true;

						HalfEdge.FlipEdge(thisEdge);
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



	private static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
	{
		Vector2 l1_p1 = new Vector2(edge1.v1.position.x, edge1.v1.position.z);
		Vector2 l1_p2 = new Vector2(edge1.v2.position.x, edge1.v2.position.z);

		Vector2 l2_p1 = new Vector2(edge2.v1.position.x, edge2.v1.position.z);
		Vector2 l2_p2 = new Vector2(edge2.v2.position.x, edge2.v2.position.z);

		bool isIntersecting = LineLineIntersection.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

		return isIntersecting;
	}

	//From triangle where each triangle has one vertex to half edge
	public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles)
	{
		//Make sure the triangles have the same orientation
		Triangle.OrientTrianglesClockwise(triangles);

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
}
