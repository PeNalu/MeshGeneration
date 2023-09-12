using ApexInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    [SerializeField]
    [Array]
    private List<Transform> trackedObjects;

    [SerializeField]
    [Range(0, 100)]
    private float minDistance = 2f;

    #region [Properties]
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
    private List<Vector2Int> pointsToDisable;
    #endregion

    private void Update()
    {
        if (filled)
        {
            filter.mesh = GenerateMeshWithHoles();
        }
    }

    /// <summary>
    /// Adjusts all the necessary parameters.
    /// </summary>
    /// <param name="meshFilter"></param>
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

        pointsToDisable = new List<Vector2Int>();
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

    /// <summary>
    /// Recalculate the holes in the mesh.
    /// </summary>
    private Mesh GenerateMeshWithHoles()
    {
        // Clearing information about past holes.
        BaseGridGenerator.Instance.SetWolkable(pointsToDisable, true);
        pointsToDisable.Clear();

        // Calculate hole.
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
                    Vector2Int twoDPos = new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.z));
                    pointsToDisable.Add(twoDPos);
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
        RemoveAllSpecifiedIndicesFromArray(ref triangles, trianglesDisabled);

        // Change mesh.
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        for (int i = 0; i < trianglesDisabled.Length; ++i)
        {
            trianglesDisabled[i] = false;
        }

        // Setting non walkable points.
        BaseGridGenerator.Instance.SetWolkable(pointsToDisable, false);

        return mesh;
    }

    /// <summary>
    /// Creates a hole, but does not rebuild navigation.
    /// </summary>
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
}