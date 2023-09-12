using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ApexInspector;
using System.Diagnostics;
using System;

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
            List<int> res = new List<int>();
            for (int j = 0; j < origtriangles.Length; j++)
            {
                if (origtriangles[j] == i)
                {
                    res.Add(j);
                }
            }
            trisWithVertex[i] = res;
        }
        filled = true;
    }

    public void PrintArr(List<int> arr)
    {
        string res = "";
        for (int i = 0; i < arr.Count; i++)
        {
            res += $" {arr[i]}";
        }
        print(res);
    }

    public void Remesh()
    {
        filter.mesh = GenerateMeshWithHoles();
    }

    private Mesh GenerateMeshWithHoles()
    {
        Transform mYTransform = transform;
        foreach (MeshObstacle obstacle in meshObstacles)
        {
            Vector3 trackPos = obstacle.transform.position;
            float dist = obstacle.GetDistance() * obstacle.GetDistance();
            for (int i = 0; i < origvertices.Length; ++i)
            {
                Vector3 v = Vector3.Scale(origvertices[i], mYTransform.localScale);
                if ((v + mYTransform.position - trackPos).sqrMagnitude < dist)
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
        triangles = origtriangles.Clone() as int[];
        //triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled);
        RemoveAllSpecifiedIndicesFromArray(ref triangles, trianglesDisabled);
        print(trianglesDisabled.Length);

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        for (int i = 0; i < trianglesDisabled.Length; ++i)
            trianglesDisabled[i] = false;
        return mesh;
    }

    public void RemoveAllSpecifiedIndicesFromArray(ref int[] a, bool[] indicesToRemove)
    {
        int i = 0;
        for (int j = 0; j < a.Length; j++)
        {
            if (!indicesToRemove[j])
            {
                a[i++] = a[j];
            }
        }

        a = a[0..i];
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