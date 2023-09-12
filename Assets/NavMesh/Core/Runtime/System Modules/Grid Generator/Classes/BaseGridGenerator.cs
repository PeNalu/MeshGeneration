using ApexInspector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[HideScriptField]
public class BaseGridGenerator : Singleton<BaseGridGenerator>
{
    [SerializeField]
    [Foldout("Advanced Settings")]
    private bool usePoints = true;

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

    public void BuildGrid(Transform startPosition, Vector2Int gridSize)
    {
        points = new HashSet<Vector3>();
        size = gridSize;
        twoDpoints = new Dictionary<Vector2Int, BaseGridNode>();
        matrix = new BaseGridNode[gridSize.x * 2, gridSize.y * 2];
        vecMatrix = new Vector3[gridSize.x * 2, gridSize.y * 2];

        float rHeight = startPosition.position.y + 10;

        if (usePoints)
        {
            ByPoints(startPosition.position, rHeight);
        }
        else
        {
            ByMesh(startPosition.position, rHeight);
        }
    }

    private void ByPoints(Vector3 startPosition, float rHeight)
    {
        var sw = new Stopwatch();
        sw.Start();
        for (int x = -size.x; x < size.x; x++)
        {
            for (int z = -size.y; z < size.y; z++)
            {
                TryAddPoint(startPosition + new Vector3(x, rHeight, z), new Vector2Int(x + size.x, z + size.y));
            }
        }

/*        int a = size.y * 2 / (int)cellSize.x;
        int b = size.x * 2 / (int)cellSize.y;

        for (int z = 0; z < a; z++)
        {
            for (int x = 0; x < b; x++)
            {
                Vector3 sPosition = new Vector3(startPosition.x - b * z, startPosition.y, startPosition.z - a * x);

                RaycastHit hit;
                Vector3 pos = sPosition + new Vector3(x + Random.Range(0, 0.15f), rHeight, z + Random.Range(0, 0.15f));
                if (Physics.Raycast(pos, Vector3.down, out hit))
                {
                    Vector3 nodePos = new Vector3(hit.point.x, hit.point.y + pointHeightOffset, hit.point.z);
                    points.Add(nodePos);
                    vecMatrix[x, z] = nodePos;
                }
            }
        }*/


        for (int z = 0; z < size.y * 2; z++)
        {
            for (int x = 0; x < size.x * 2; x++)
            {
                Vector3 sPosition = new Vector3(startPosition.x - size.x, startPosition.y, startPosition.z - size.y);

                RaycastHit hit;
                Vector3 pos = sPosition + new Vector3(x + Random.Range(0, 0.15f), rHeight, z + Random.Range(0, 0.15f));
                if (Physics.Raycast(pos, Vector3.down, out hit, float.MaxValue, cullingMask))
                {
                    Vector3 nodePos = new Vector3(hit.point.x, hit.point.y + pointHeightOffset, hit.point.z);
                    points.Add(nodePos);
                    vecMatrix[x, z] = nodePos;
                }
            }
        }

        meshGenerator.Initialize(new Vector2Int(size.x * 2, size.y * 2), vecMatrix, maxSlope);
        sw.Stop();
        print(sw.ElapsedMilliseconds);
    }


    private void ByMesh(Vector3 startPosition, float rHeight)
    {
        for (int z = 0; z < size.y * 2; z++)
        {
            for (int x = 0; x < size.x * 2; x++)
            {
                Vector3 sPosition = new Vector3(startPosition.x - size.x, startPosition.y, startPosition.z - size.y);

                RaycastHit hit;
                Vector3 pos = sPosition + new Vector3(x + Random.Range(0, 0.15f), rHeight, z + Random.Range(0, 0.15f));
                if (Physics.Raycast(pos, Vector3.down, out hit))
                {
                    Vector3 nodePos = new Vector3(hit.point.x, hit.point.y + pointHeightOffset, hit.point.z);
                    points.Add(nodePos);
                }

                sPosition = new Vector3(startPosition.x - size.x, startPosition.y, startPosition.z - size.y);

                pos = sPosition + new Vector3(((x + Random.Range(0, 0.15f)) - 0.5f), rHeight, ((z + Random.Range(0, 0.15f)) - 0.5f));
                if (Physics.Raycast(pos, Vector3.down, out hit))
                {
                    Vector3 nodePos = new Vector3(hit.point.x, hit.point.y + pointHeightOffset, hit.point.z);
                    points.Add(nodePos);
                }
            }
        }

        meshGenerator.Initialize(MakeClusters());
    }

    private List<Cluster> MakeClusters()
    {
        List<Cluster> clusters = new List<Cluster>();
        while (points.Count > 0)
        {
            Cluster cluster = MakeCluster();
            clusters.Add(cluster);
        }

        return clusters;
    }

    private Cluster MakeCluster()
    {
        HashSet<Vector3> clusterPoint = new HashSet<Vector3>();
        HashSet<Vector3> processPoints = new HashSet<Vector3>();
        Vector3 initPoint = points.First();
        clusterPoint.Add(initPoint);
        points.Remove(initPoint);

        processPoints = GetNearestPoints(initPoint);

        while (processPoints.Count > 0)
        {
            HashSet<Vector3> pointToAdd = new HashSet<Vector3>();
            foreach (Vector3 item in processPoints)
            {
                HashSet<Vector3> vectors = GetNearestPoints(item);
                foreach (Vector3 vec in vectors)
                {
                    pointToAdd.Add(vec);
                }
            }

            foreach (Vector3 item in processPoints)
            {
                clusterPoint.Add(item);
            }
            processPoints.Clear();

            foreach (Vector3 item in pointToAdd)
            {
                processPoints.Add(item);
            }
        }

        return new Cluster(clusterPoint);
    }

    public HashSet<Vector3> GetNearestPoints(Vector3 point)
    {
        HashSet<Vector3> result = new HashSet<Vector3>();

        foreach (Vector3 item in points)
        {
            if(Vector3.Distance(point, item) <= offset)
            {
                result.Add(item);
            }
        }

        foreach (Vector3 item in result)
        {
            points.Remove(item);
        }

        return result;
    }

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

    private void OnDrawGizmos()
    {
       
    }

    public void SetWolkable(List<Vector2Int> points, bool value)
    {
        for (int i = 0; i < points.Count; i++)
        {
            twoDpoints[points[i]].Walkable = value;
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
