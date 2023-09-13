using ApexInspector;
using System.Collections.Generic;
using UnityEngine;

[HideScriptField]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField]
    private bool debugMode = false;

    [SerializeField]
    [NotNull]
    private Material material;

    //Stored required properties.
    private Vector3[] vertices;
    private Mesh mesh;

    private void OnEnable()
    {
        mesh = new Mesh
        {
            name = "Procedural Mesh"
        };
    }

    /// <summary>
    /// Creates a mesh from the specified grid.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    /// <param name="vecMatrix">Grid points.</param>
    /// <param name="slopAngle">Maximum angle of inclination.</param>
    public void CreateMesh(Vector2Int size, Vector3[,] vecMatrix, float slopAngle)
    {
        vertices = GetVertices(size, vecMatrix);

        mesh.vertices = vertices;
        mesh.triangles = GetTriangles(size, slopAngle); 
        GetComponent<MeshFilter>().mesh = mesh;
        MeshCutter meshCutter = gameObject.AddComponent<MeshCutter>();
        meshCutter.Initialize(GetComponent<MeshFilter>());
    }

    /// <summary>
    /// Forms triangles from grid points.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    /// <param name="slopAngle">Maximum angle of inclination.</param>
    private int[] GetTriangles(Vector2Int size, float slopAngle)
    {
        List<int> triangles = new List<int>();
        for (int ti = 0, vi = 0, y = 0; y < size.y - 1; y++, vi++)
        {
            for (int x = 0; x < size.x - 1; x++, ti += 6, vi++)
            {
                Vector3 normal2 = Vector3.Cross(vertices[vi + 1] - vertices[vi + size.x], vertices[vi + size.x] - vertices[vi + size.x + 1]);
                Vector3 normal1 = Vector3.Cross(vertices[vi] - vertices[vi + 1], vertices[vi + size.x] - vertices[vi]);
                int angle1 = (int)Vector3.Angle(Vector3.up, normal1);
                int angle2 = (int)Vector3.Angle(Vector3.up, normal2);

                if (angle1 <= slopAngle)
                {
                    triangles.AddRange(new int[3] { 0, 0, 0 });
                    triangles[(triangles.Count - 3)] = vi;
                    triangles[(triangles.Count - 3) + 1] = vi + size.x;
                    triangles[(triangles.Count - 3) + 2] = vi + 1;
                }

                if (angle2 <= slopAngle)
                {
                    triangles.AddRange(new int[3] { 0, 0, 0 });
                    triangles[(triangles.Count - 3) + 1] = vi + size.x;
                    triangles[(triangles.Count - 3)] = vi + 1;
                    triangles[(triangles.Count - 3) + 2] = vi + size.x + 1;
                }
            }
        }

        return triangles.ToArray();
    }

    /// <summary>
    /// Converts a point cloud into a sequential set of points.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    /// <param name="vecMatrix">Grid points.</param>
    private Vector3[] GetVertices(Vector2Int size, Vector3[,] vecMatrix)
    {
        Vector3[] vertices = new Vector3[(size.x) * (size.y)];
        for (int z = 0, i = 0; z < size.y; z++)
        {
            for (int x = 0; x < size.x; x++, i++)
            {
                vertices[i] = vecMatrix[x, z];
            }
        }

        return vertices;
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
