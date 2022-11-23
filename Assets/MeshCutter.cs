using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ApexInspector;

public class MeshCutter : MonoBehaviour
{
    [SerializeField]
    [Array]
    private List<Transform> trackedObjects;

    [SerializeField]
    [Range(0, 100)]
    private float minDistance = 2f;

    // Stored required properties.
    private Mesh mesh;
    private MeshFilter filter;
    private bool filled = false;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uvs;
    private Vector3[] origvertices;
    private Vector3[] orignormals;
    private Vector2[] origuvs;
    private int[] triangles;
    private int[] origtriangles;
    private bool[] trianglesDisabled;
    private List<int>[] trisWithVertex;
    private List<MeshObstacle> meshObstacles;

    private void Update()
    {
        if (filled)
        {
            Remesh();
        }
    }

    public void Initialize(MeshFilter meshFilter)
    {
        meshObstacles = FindObjectsOfType<MeshObstacle>().ToList();
        filter = meshFilter;
        mesh = new Mesh();
        orignormals = filter.mesh.normals;
        origvertices = filter.mesh.vertices;
        origuvs = filter.mesh.uv;
        origtriangles = filter.mesh.triangles;

        vertices = new Vector3[origvertices.Length];
        normals = new Vector3[orignormals.Length];
        uvs = new Vector2[origuvs.Length];
        triangles = new int[origtriangles.Length];
        trianglesDisabled = new bool[origtriangles.Length];

        orignormals.CopyTo(normals, 0);
        origvertices.CopyTo(vertices, 0);
        origtriangles.CopyTo(triangles, 0);
        origuvs.CopyTo(uvs, 0);

        trisWithVertex = new List<int>[origvertices.Length];
        for (int i = 0; i < origvertices.Length; ++i)
        {
            trisWithVertex[i] = origtriangles.IndexOf(i);

        }
        filled = true;
    }

    public void Remesh()
    {
        filter.mesh = GenerateMeshWithHoles();
    }

    private Mesh GenerateMeshWithHoles()
    {
        foreach (MeshObstacle obstacle in meshObstacles)
        {
            Vector3 trackPos = obstacle.transform.position;
            for (int i = 0; i < origvertices.Length; ++i)
            {
                Vector3 v = new Vector3(origvertices[i].x * transform.localScale.x, origvertices[i].y * transform.localScale.y, origvertices[i].z * transform.localScale.z);
                if ((v + transform.position - trackPos).magnitude < obstacle.GetDistance())
                {
                    for (int j = 0; j < trisWithVertex[i].Count; ++j)
                    {
                        int value = trisWithVertex[i][j];
                        int remainder = value % 3;
                        trianglesDisabled[value - remainder] = true;
                        trianglesDisabled[value - remainder + 1] = true;
                        trianglesDisabled[value - remainder + 2] = true;
                    }
                }
            }
        }
        triangles = origtriangles;
        triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled).ToArray();

        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        for (int i = 0; i < trianglesDisabled.Length; ++i)
            trianglesDisabled[i] = false;
        return mesh;
    }
    private Mesh GenerateMeshWithFakeHoles()
    {
        Vector3 trackPos = trackedObjects[0].position;
        for (int i = 0; i < origvertices.Length; ++i)
        {
            if ((origvertices[i] + transform.position - trackPos).magnitude < minDistance)
            {
                normals[i] = -orignormals[i];
            }
            else
            {
                normals[i] = orignormals[i];
            }
        }
        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        return mesh;
    }
}