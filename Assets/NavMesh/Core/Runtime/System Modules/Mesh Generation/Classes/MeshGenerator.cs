using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField]
    private bool debugMode = false;

    [SerializeField]
    private bool optimize = true;

    [SerializeField]
    private Material material;

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
        List<Vertex> vertices = new List<Vertex>();
        foreach (Vector3 pos in points)
        {
            vertices.Add(new Vertex(pos));
        }
        points = JarvisMarchAlgorithm.GetConvexHull(vertices).Select(x => x.position).ToList();

        List<Triangle> triangles = Triangulate.TriangulateByFlippingEdges(points);

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

    public void Initialize(Dictionary<float, List<Vector3>> clusters)
    {
        int i = 0;
        foreach (KeyValuePair<float, List<Vector3>> item in clusters)
        {
            GameObject gO = new GameObject($"Cluster {i}");
            gO.AddComponent<MeshFilter>();
            gO.AddComponent<MeshRenderer>();

            List<Vertex> vertices = new List<Vertex>();
            List<Vector3> points = item.Value;
            foreach (Vector3 pos in item.Value)
            {
                vertices.Add(new Vertex(pos));
            }
            points = JarvisMarchAlgorithm.GetConvexHull(vertices).Select(x => x.position).ToList();

            List<Triangle> triangles = Triangulate.TriangulateByFlippingEdges(points);

            List<TriangleTriangleIntersection.Triangle> tri = new List<TriangleTriangleIntersection.Triangle>();
            foreach (Triangle tr in triangles)
            {
                TriangleTriangleIntersection.Triangle triangle = new TriangleTriangleIntersection.Triangle();
                triangle.p1 = tr.v1.position;
                triangle.p2 = tr.v2.position;
                triangle.p3 = tr.v3.position;

                tri.Add(triangle);
            }

            TriangleTriangleIntersection.CreateTriangleMesh(tri, gO);

            i++;
        }
    }

    public void Initialize(List<Cluster> clusters)
    {
        int i = 0;
        foreach (Cluster item in clusters)
        {
            GameObject gO = new GameObject($"Cluster {i}");
            gO.AddComponent<MeshFilter>();
            MeshRenderer mR = gO.AddComponent<MeshRenderer>();
            mR.materials = new Material[] { material };

            List<Vertex> vertices = new List<Vertex>();
            HashSet<Vector3> points = item.GetPoints();
            foreach (Vector3 pos in points)
            {
                vertices.Add(new Vertex(pos));
            }

            if (optimize)
            {
                points = JarvisMarchAlgorithm.GetConvexHull(vertices).Select(x => x.position).ToHashSet();
            }

            List<Triangle> triangles = Triangulate.TriangulateByFlippingEdges(points.ToList());

            List<TriangleTriangleIntersection.Triangle> tri = new List<TriangleTriangleIntersection.Triangle>();
            foreach (Triangle tr in triangles)
            {
                TriangleTriangleIntersection.Triangle triangle = new TriangleTriangleIntersection.Triangle();
                triangle.p1 = tr.v1.position;
                triangle.p2 = tr.v2.position;
                triangle.p3 = tr.v3.position;

                tri.Add(triangle);
            }

            TriangleTriangleIntersection.CreateTriangleMesh(tri, gO);
            MeshCutter meshCutter = gO.AddComponent<MeshCutter>();
            meshCutter.Initialize(gO.GetComponent<MeshFilter>());
            i++;
        }
    }

    public void Initialize(Vector2Int size, Vector3[,] vecMatrix, float slopAngle)
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

        //int[] triangles = new int[((size.x - 1) * (size.y - 1)) * 6];
        List<int> vs = new List<int>();

        for (int ti = 0, vi = 0, y = 0; y < size.y - 1; y++, vi++)
        {
            for (int x = 0; x < size.x - 1; x++, ti += 6, vi++)
            {
                /*                triangles[ti] = vi;
                                triangles[ti + 1] = triangles[ti + 4] = vi + size.x;
                                triangles[ti + 2] = triangles[ti + 3] = vi + 1;
                                triangles[ti + 5] = vi + size.x + 1;*/
                //vs.AddRange(new int[6] { 0, 0, 0, 0, 0, 0 }) ;

                Vector3 normal2 = Vector3.Cross(vertices[vi + 1] - vertices[vi + size.x], vertices[vi + size.x] - vertices[vi + size.x + 1]);
                Vector3 normal1 = Vector3.Cross(vertices[vi] - vertices[vi + 1], vertices[vi + size.x] - vertices[vi]);
                int angle1 = (int)Vector3.Angle(Vector3.up, normal1);
                int angle2 = (int)Vector3.Angle(Vector3.up, normal2);

                if(angle1 <= slopAngle)
                {
                    vs.AddRange(new int[3] { 0, 0, 0 });
                    vs[(vs.Count - 3)] = vi;
                    vs[(vs.Count - 3) + 1] = vi + size.x;
                    vs[(vs.Count - 3) + 2] = vi + 1;
                }

                if (angle2 <= slopAngle)
                {
                    vs.AddRange(new int[3] { 0, 0, 0 });
                    vs[(vs.Count - 3) + 1] = vi + size.x;
                    vs[(vs.Count - 3)] = vi + 1;
                    vs[(vs.Count - 3) + 2] = vi + size.x + 1;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = vs.ToArray();
        GetComponent<MeshFilter>().mesh = mesh;
        MeshCutter meshCutter = gameObject.AddComponent<MeshCutter>();
        meshCutter.Initialize(GetComponent<MeshFilter>());
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
