using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//Triangle-triangle intersection in 2D
public class TriangleTriangleIntersection : MonoBehaviour
{
    [SerializeField]
    private Transform point;
    //Triangle corners
    public Transform[] triangle1Corners;
    public Transform[] triangle2Corners;
    //Triangle objects to which we will attach meshes showing the triangles
    public GameObject triangle1Obj;
    public GameObject triangle2Obj;
    //Boxes for debugging of bounding box around triangle
    public GameObject box1Obj;
    public GameObject box2Obj;

    void Update()
    {
        //Get the positions of the triangles corners
        Vector3 t1_p1 = triangle1Corners[0].position;
        Vector3 t1_p2 = triangle1Corners[1].position;
        Vector3 t1_p3 = triangle1Corners[2].position;

        Vector3 t2_p1 = triangle2Corners[0].position;
        Vector3 t2_p2 = triangle2Corners[1].position;
        Vector3 t2_p3 = triangle2Corners[2].position;

        //Create new triangles
        Triangle triangle1 = new Triangle(t1_p1, t1_p2, t1_p3);
        Triangle triangle2 = new Triangle(t2_p1, t2_p2, t2_p3);

        //Build a triangle mesh between the corners
        CreateTriangleMesh(triangle1, triangle1Obj);
        CreateTriangleMesh(triangle2, triangle2Obj);

        if (IsPointInTriangle(point.position, t1_p1, t1_p2, t1_p3))
        {
            triangle1Obj.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            triangle1Obj.GetComponent<MeshRenderer>().material.color = Color.green;
        }

        /*//Check for intersection
        if (IsTriangleTriangleIntersecting(triangle1, triangle2))
        {
            triangle1Obj.GetComponent<MeshRenderer>().material.color = Color.red;
            triangle2Obj.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            triangle1Obj.GetComponent<MeshRenderer>().material.color = Color.blue;
            triangle2Obj.GetComponent<MeshRenderer>().material.color = Color.blue;
        }*/
    }

    //The triangle-triangle intersection in 2D algorithm
    public static bool IsTriangleTriangleIntersecting(Triangle triangle1, Triangle triangle2)
    {
        bool isIntersecting = false;

        //Step 1. AABB intersection
        if (IsIntersectingAABB(triangle1, triangle2))
        {
            //Step 2. Line segment - triangle intersection
            if (AreAnyLineSegmentsIntersecting3D(triangle1, triangle2))
            {
                isIntersecting = true;
            }
            //Step 3. Point in triangle intersection - if one of the triangles is inside the other
            else if (AreCornersIntersecting(triangle1, triangle2))
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;
    }

    //
    // Intersection: Point in triangle
    //

    //There's a possibility that one of the triangles is smaller than the other
    //So we have to check if any of the triangle's corners is inside the other triangle
    public static bool AreCornersIntersecting(Triangle t1, Triangle t2)
    {
        bool isIntersecting = false;

        //We only have to test one corner from each triangle
        //Triangle 1 in triangle 2
        if (IsPointInTriangle(t1.p1.XZ(), t2.p1.XZ(), t2.p2.XZ(), t2.p3.XZ()))
        {
            isIntersecting = true;
        }
        //Triangle 2 in triangle 1
        else if (IsPointInTriangle(t2.p1.XZ(), t1.p1.XZ(), t1.p2.XZ(), t1.p3.XZ()))
        {
            isIntersecting = true;
        }

        return isIntersecting;
    }



    //Is a point p inside a triangle p1-p2-p3?
    //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
    public static bool IsPointInTriangle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        bool isWithinTriangle = false;

        float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

        float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        float c = 1 - a - b;

        //The point is within the triangle if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }

    //Is a point p inside a triangle p1-p2-p3?
    //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
    public static bool IsPointInTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return SameSide(p1, p2, p3, p) &&
                SameSide(p3, p2, p1, p) &&
                SameSide(p3, p1, p2, p);
    }

    public static bool SameSide(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        Vector3 AB = B - A;
        Vector3 AC = C - A;
        Vector3 AP = P - A;

        Vector3 v1 = Vector3.Cross(AB, AC);
        Vector3 v2 = Vector3.Cross(AB, AP);

        // v1 and v2 should point to the same direction
        return Vector3.Dot(v1, v2) >= 0;
    }

    //
    // Intersection: Line segment - triangle
    //

    //Check if any of the edges that make up one of the triangles is intersecting with any of
    //the edges of the other triangle
    public static bool AreAnyLineSegmentsIntersecting(Triangle t1, Triangle t2)
    {
        bool isIntersecting = false;

        //Loop through all edges
        for (int i = 0; i < t1.lineSegments.Length; i++)
        {
            for (int j = 0; j < t2.lineSegments.Length; j++)
            {
                //The start/end coordinates of the current line segments
                Vector3 t1_p1 = t1.lineSegments[i].p1;
                Vector3 t1_p2 = t1.lineSegments[i].p2;
                Vector3 t2_p1 = t2.lineSegments[j].p1;
                Vector3 t2_p2 = t2.lineSegments[j].p2;

                //Are they intersecting?
                if (AreLineSegmentsIntersecting2D(t1_p1, t1_p2, t2_p1, t2_p2))
                {
                    isIntersecting = true;

                    //To stop the outer for loop
                    i = int.MaxValue - 1;

                    break;
                }
            }
        }

        return isIntersecting;
    }

    public static bool AreAnyLineSegmentsIntersecting3D(Triangle t1, Triangle t2)
    {
        bool isIntersecting = false;

        //Loop through all edges
        for (int i = 0; i < t1.lineSegments.Length; i++)
        {
            Vector3 t1_p1 = t1.lineSegments[i].p1;
            Vector3 t1_p2 = t1.lineSegments[i].p2;

            //Are they intersecting?
            if (AreLineSegmentsIntersecting3D(t1_p1, t1_p2, t2))
            {
                isIntersecting = true;

                //To stop the outer for loop
                i = int.MaxValue - 1;

                break;
            }
        }

        for (int j = 0; j < t2.lineSegments.Length; j++)
        {

            Vector3 t2_p1 = t2.lineSegments[j].p1;
            Vector3 t2_p2 = t2.lineSegments[j].p2;

            //Are they intersecting?
            if (AreLineSegmentsIntersecting3D(t2_p1, t2_p2, t1))
            {
                isIntersecting = true;

                //To stop the outer for loop
                j = int.MaxValue - 1;

                break;
            }
        }

        return isIntersecting;
    }

    //Check if 2 line segments are intersecting in 2d space
    //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
    //p1 and p2 belong to line 1, p3 and p4 belong to line 2
    public static bool AreLineSegmentsIntersecting2D(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        bool isIntersecting = false;

        float denominator = (p4.z - p3.z) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.z - p1.z);

        //Make sure the denominator is != 0, if 0 the lines are parallel
        if (denominator != 0)
        {
            float u_a = ((p4.x - p3.x) * (p1.z - p3.z) - (p4.z - p3.z) * (p1.x - p3.x)) / denominator;
            float u_b = ((p2.x - p1.x) * (p1.z - p3.z) - (p2.z - p1.z) * (p1.x - p3.x)) / denominator;

            //Is intersecting if u_a and u_b are between 0 and 1
            if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;
    }

    public static bool AreLineSegmentsIntersecting3D(Vector3 p1, Vector3 p2, Triangle triangle)
    {
        Vector3 dir = p2 - p1;
        Vector3 normal = Vector3.Cross(triangle.p2 - triangle.p1, triangle.p3 - triangle.p1).normalized;

        float t = normal.x * (triangle.p1.x + normal.y * triangle.p1.y + normal.z * triangle.p1.z) / ((normal.x * dir.x * p1.x) + (normal.y * dir.y * p1.y) + (normal.z * dir.z * p1.z));

        float x = dir.x * t * p1.x;
        float y = dir.y * t * p1.y;
        float z = dir.z * t * p1.z;

        Vector3 intersectionPoint = new Vector3(x, y, z);

        return IsPointInTriangle(intersectionPoint, triangle.p1, triangle.p2, triangle.p3);
    }


    //
    // Other stuff
    //

    //Create a triangle mesh with 2 sides from 3 coordinates
    public static void CreateTriangleMesh(Triangle triangle, GameObject triangleObj)
    {
        //The vertices
        Vector3[] newVertices = new Vector3[3];

        //Add the vertices
        newVertices[0] = triangle.p1;
        newVertices[1] = triangle.p2;
        newVertices[2] = triangle.p3;

        //The triangles (6 because we need a bottom so we can move the corners the way we want)
        int[] newTriangles = new int[6];

        newTriangles[0] = 2;
        newTriangles[1] = 1;
        newTriangles[2] = 0;

        newTriangles[3] = 0;
        newTriangles[4] = 1;
        newTriangles[5] = 2;

        //Add everything to a mesh
        Mesh newMesh = new Mesh();

        newMesh.vertices = newVertices;
        newMesh.triangles = newTriangles;

        //Add the mesh to the object
        MeshFilter meshFilter = triangleObj.GetComponent<MeshFilter>();
        meshFilter.mesh = newMesh;
    }

    public static void CreateTriangleMesh(List<Triangle> triangles, GameObject triangleObj)
    {
        Vector3[] newVertices = new Vector3[3 * triangles.Count];
        int[] newTriangles = new int[6 * triangles.Count];

        for (int i = 0; i < triangles.Count; i++)
        {
            newVertices[(3 * i)] = triangles[i].p1;
            newVertices[1 + (3 * i)] = triangles[i].p2;
            newVertices[2 + (3 * i)] = triangles[i].p3;

            newTriangles[0 + (6 * i)] = 2 + (3 * i);
            newTriangles[1 + (6 * i)] = 1 + (3 * i);
            newTriangles[2 + (6 * i)] = 0 + (3 * i);

            newTriangles[3 + (6 * i)] = 0 + (3 * i);
            newTriangles[4 + (6 * i)] = 1 + (3 * i);
            newTriangles[5 + (6 * i)] = 2 + (3 * i);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = newVertices;
        newMesh.triangles = newTriangles;

        MeshFilter meshFilter = triangleObj.GetComponent<MeshFilter>();
        meshFilter.mesh = newMesh;
    }

    //
    // Intersection: AABB
    //

    //Approximate the triangles with rectangles and see if they intersect with the AABB intersection algorithm
    public static bool IsIntersectingAABB(Triangle t1, Triangle t2)
    {
        //Find the size of the bounding box

        //Triangle 1
        float t1_minX = Mathf.Min(t1.p1.x, Mathf.Min(t1.p2.x, t1.p3.x));
        float t1_maxX = Mathf.Max(t1.p1.x, Mathf.Max(t1.p2.x, t1.p3.x));
        float t1_minZ = Mathf.Min(t1.p1.z, Mathf.Min(t1.p2.z, t1.p3.z));
        float t1_maxZ = Mathf.Max(t1.p1.z, Mathf.Max(t1.p2.z, t1.p3.z));

        //Triangle 2
        float t2_minX = Mathf.Min(t2.p1.x, Mathf.Min(t2.p2.x, t2.p3.x));
        float t2_maxX = Mathf.Max(t2.p1.x, Mathf.Max(t2.p2.x, t2.p3.x));
        float t2_minZ = Mathf.Min(t2.p1.z, Mathf.Min(t2.p2.z, t2.p3.z));
        float t2_maxZ = Mathf.Max(t2.p1.z, Mathf.Max(t2.p2.z, t2.p3.z));


        //Are the rectangles intersecting?
        //If the min of one box in one dimension is greater than the max of another box then the boxes are not intersecting
        //They have to intersect in 2 dimensions. We have to test if box 1 is to the left or box 2 and vice versa
        bool isIntersecting = true;

        //X axis
        if (t1_minX > t2_maxX)
        {
            isIntersecting = false;
        }
        else if (t2_minX > t1_maxX)
        {
            isIntersecting = false;
        }
        //Z axis
        else if (t1_minZ > t2_maxZ)
        {
            isIntersecting = false;
        }
        else if (t2_minZ > t1_maxZ)
        {
            isIntersecting = false;
        }

        return isIntersecting;
    }

    //To create a line segment
    public struct LineSegment
    {
        //Start/end coordinates
        public Vector3 p1, p2;

        public LineSegment(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }

    //To store triangle data to get cleaner code
    public struct Triangle
    {
        //Corners of the triangle
        public Vector3 p1, p2, p3;
        //The 3 line segments that make up this triangle
        public LineSegment[] lineSegments;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            lineSegments = new LineSegment[3];

            lineSegments[0] = new LineSegment(p1, p2);
            lineSegments[1] = new LineSegment(p2, p3);
            lineSegments[2] = new LineSegment(p3, p1);
        }
    }
}