using ApexInspector;
using System.Collections.Generic;
using UnityEngine;

[HideScriptField]
public class BaseGridGenerator : Singleton<BaseGridGenerator>
{
    #region [Properties]
    [SerializeField]
    [Foldout("Advanced Settings")]
    [NotNull]
    private MeshGenerator meshGenerator;

    [SerializeField]
    [Foldout("Advanced Settings")]
    private LayerMask cullingMask;

    [SerializeField]
    [Foldout("Advanced Settings")]
    [Range(0, 90)]
    private int angularCoefficient;

    [SerializeField]
    [Foldout("Advanced Settings")]
    [Range(0, 1000)]
    private int angularMultiplier;

    [SerializeField]
    [Foldout("Advanced Settings")]
    [Range(0.0f, 2.0f)]
    private float pointHeightOffset;

    [SerializeField]
    [Foldout("Advanced Settings")]
    [Range(0, 90)]
    private int maxSlope = 50;

    [SerializeField]
    [Foldout("Advanced Settings")]
    [Range(0.0f, 2.0f)]
    private float offset = 1f;

    [SerializeField]
    [Foldout("Debug Settings")]
    private bool debugMode;

    [SerializeField]
    [Foldout("Debug Settings")]
    [VisibleIf("debugMode")]
    private bool debugOnlyNonWalkable;

    //Stored required properties.
    private Dictionary<Vector2Int, BaseGridNode> twoDpoints;
    private HashSet<Vector3> points;
    private BaseGridNode[,] matrix;
    private Vector3[,] vecMatrix;
    private Vector2Int size;
    #endregion

    public void BuildGrid(Transform startPosition, Vector2Int gridSize)
    {
        points = new HashSet<Vector3>();
        size = gridSize;
        twoDpoints = new Dictionary<Vector2Int, BaseGridNode>();
        matrix = new BaseGridNode[gridSize.x * 2, gridSize.y * 2];
        vecMatrix = new Vector3[gridSize.x * 2, gridSize.y * 2];

        float rHeight = startPosition.position.y + 10;
        CalculatePoints(startPosition.position, rHeight);
    }

    /// <summary>
    /// Builds a point cloud.
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="rHeight"></param>
    private void CalculatePoints(Vector3 startPosition, float rHeight)
    {
        for (int x = -size.x; x < size.x; x++)
        {
            for (int z = -size.y; z < size.y; z++)
            {
                TryAddPoint(startPosition + new Vector3(x, rHeight, z), new Vector2Int(x + size.x, z + size.y));
            }
        }

        for (int z = 0; z < size.y * 2; z++)
        {
            for (int x = 0; x < size.x * 2; x++)
            {
                Vector3 sPosition = new Vector3(startPosition.x - size.x, startPosition.y, startPosition.z - size.y);

                RaycastHit hit;
                Vector3 pos = sPosition + new Vector3(x, rHeight, z);
                if (Physics.Raycast(pos, Vector3.down, out hit, float.MaxValue, cullingMask))
                {
                    Vector3 nodePos = new Vector3(hit.point.x, hit.point.y + pointHeightOffset, hit.point.z);
                    points.Add(nodePos);
                    vecMatrix[x, z] = nodePos;
                }
            }
        }

        meshGenerator.CreateMesh(new Vector2Int(size.x * 2, size.y * 2), vecMatrix, maxSlope);
    }

    /// <summary>
    /// Checks whether a point can be created at the specified position.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="matrixPos"></param>
    private void TryAddPoint(Vector3 pos, Vector2Int matrixPos)
    {
        RaycastHit hit;

        if (Physics.Raycast(pos, Vector3.down, out hit))
        {
            Vector3 nodePos = new Vector3(hit.point.x, hit.point.y + pointHeightOffset, hit.point.z);
            Vector2Int twoDPos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));

            int cost = CalculateIncrementalCost(hit);

            BaseGridNode baseGridNode = new BaseGridNode(cost, nodePos, matrixPos);

            if (Vector3.Angle(hit.normal, Vector3.up) >= maxSlope || hit.transform.tag == "Obstacle" || Physics.OverlapSphere(nodePos, 0.5f).Length > 0)
            {
                baseGridNode.Walkable = false;
            }

            matrix[matrixPos.x, matrixPos.y] = baseGridNode;
            twoDpoints.Add(twoDPos, baseGridNode);
        }
    }

    /// <summary>
    /// Responsible for calculating the added value of the landfill.
    /// </summary>
    /// <param name="hit"></param>
    protected virtual int CalculateIncrementalCost(RaycastHit hit)
    {
        int cost = 0;
        float angle = Vector3.Angle(hit.normal, Vector3.up);
        if (angle > angularCoefficient)
        {
            cost = angularMultiplier * (int)(angle / angularCoefficient);
        }
        return cost;
    }

    /// <summary>
    /// Sets the passability flag for the list of points.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="value"></param>
    public void SetWolkable(List<Vector2Int> points, bool value)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int point = points[i];
            if (twoDpoints.ContainsKey(point))
            {
                twoDpoints[point].Walkable = value;
            }
        }
    }

    #region [Getter / Setter]
    public BaseGridNode GetNodeByPosition(Vector2Int pos)
    {
        if (twoDpoints.ContainsKey(pos))
        {
            return twoDpoints[pos];
        }

        return null;
    }

    public float GetPointHeightOffset()
    {
        return pointHeightOffset;
    }

    public BaseGridNode[,] GetMatrix()
    {
        return matrix;
    }
    #endregion
}
