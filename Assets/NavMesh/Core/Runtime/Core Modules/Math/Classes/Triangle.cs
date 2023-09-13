using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    //Corners
    public Vertex v1;
    public Vertex v2;
    public Vertex v3;

    //If we are using the half edge mesh structure, we just need one half edge
    public HalfEdge halfEdge;

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
}