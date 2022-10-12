using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField]
    private bool debugMode = false;

    //Stored required properties.
    private Vector2Int size;
    private Vector3[] vertices;
    private Mesh mesh;

    private void OnEnable()
    {
        mesh = new Mesh
        {
            name = "Procedural Mesh"
        };
    }

    public void Initialize(List<Vector3> points)
    {
        List<Triangle> triangles = Triangulate.TriangulateConcavePolygon(points);

        List<TriangleTriangleIntersection.Triangle> tri = new List<TriangleTriangleIntersection.Triangle>();
        foreach (Triangle item in triangles)
        {
            TriangleTriangleIntersection.Triangle triangle = new TriangleTriangleIntersection.Triangle();
            triangle.p1 = item.v1.position;
            triangle.p2 = item.v2.position;
            triangle.p3 = item.v3.position;

            tri.Add(triangle);
        }

        TriangleTriangleIntersection.CreateTriangleMesh(tri, gameObject);
    }

    public void Initialize(Vector2Int size, Vector3[,] vecMatrix)
    {
        this.size = size;
        vertices = new Vector3[(size.x) * (size.y)];

        for (int z = 0, i = 0; z < size.y; z++)
        {
            for (int x = 0; x < size.x; x++, i++)
            {
                Vector3 vec = vecMatrix[x, z];
                vertices[i] = vec;
            }
        }

        int[] triangles = new int[((size.x - 1) * (size.y - 1)) * 6];

        for (int ti = 0, vi = 0, y = 0; y < size.y - 1; y++, vi++)
        {
            for (int x = 0; x < size.x - 1; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = triangles[ti + 4] = vi + size.x;
                triangles[ti + 2] = triangles[ti + 3] = vi + 1;
                triangles[ti + 5] = vi + size.x + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        if(vertices != null && debugMode)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(vertices[i], 0.2f);
            }
        }
    }
}
