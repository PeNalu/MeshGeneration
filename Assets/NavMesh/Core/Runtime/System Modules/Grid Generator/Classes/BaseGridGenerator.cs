using System.Collections.Generic;
using UnityEngine;
using ApexInspector;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Linq;

[HideScriptField]
public class BaseGridGenerator : Singleton<BaseGridGenerator>
{
    [SerializeField]
    private MeshGenerator meshGenerator;

    [SerializeField]
    private int angularCoefficient;

    [SerializeField]
    private int angularMultiplier;

    [SerializeField]
    private float pointHeightOffset;

    [SerializeField]
    private int maxSlope = 50;

    [SerializeField]
    private bool debugMode;

    [SerializeField]
    [VisibleIf("debugMode")]
    private bool debugOnlyNonWalkable;

    private Dictionary<Vector2Int, BaseGridNode> twoDpoints;
    public BaseGridNode[,] matrix;
    public List<Vector3> points;

    private List<Vertex> vertToPrint;

    public Vector3[,] vecMatrix;
    public bool flag = false;
    public Vector2Int size;

    public void BuildGrid(Transform startPosition, Vector2Int gridSize)
    {
        print(11111);
        size = gridSize;
        twoDpoints = new Dictionary<Vector2Int, BaseGridNode>();
        matrix = new BaseGridNode[gridSize.x * 2, gridSize.y * 2];
        vecMatrix = new Vector3[gridSize.x * 2, gridSize.y * 2];

        float rHeight = startPosition.position.y + 10;

/*        for (int x = -gridSize.x; x < gridSize.x; x++)
        {
            for (int z = -gridSize.y; z < gridSize.y; z++)
            {
                TryAddPoint(startPosition.position + new Vector3(x, rHeight, z), new Vector2Int(x + gridSize.x, z + gridSize.y));
            }
        }*/

        for (int z = 0; z < gridSize.y * 2; z++)
        {
            for (int x = 0; x < gridSize.x * 2; x++)
            {
                Vector3 sPosition = new Vector3(startPosition.position.x - gridSize.x, startPosition.position.y, startPosition.position.z - gridSize.y);

                RaycastHit hit;
                Vector3 pos = sPosition + new Vector3(x, rHeight, z);
                if (Physics.Raycast(pos, Vector3.down, out hit))
                {
                    Vector3 nodePos = new Vector3(hit.point.x, hit.point.y + pointHeightOffset, hit.point.z);
                    points.Add(nodePos);
                    //print($"{nodePos.x} {nodePos.z}");
                    vecMatrix[x, z] = nodePos;
                }
            }
        }

        List<Vertex> vert = new List<Vertex>();

        for (int z = 0; z < gridSize.y * 2; z++)
        {
            for (int x = 0; x < gridSize.x * 2; x++)
            {
                Vertex vertex = new Vertex(vecMatrix[x, z]);
                vert.Add(vertex);
            }
        }

        //vertToPrint = JarvisMarchAlgorithm.GetConvexHull(vert);

        meshGenerator.Initialize(points);
        //meshGenerator.Initialize(new Vector2Int(gridSize.x * 2, gridSize.y * 2), vecMatrix);

        flag = true;
    }

    private void BuildMesh()
    {

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
        if(vertToPrint != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < vertToPrint.Count - 1; i++)
            {
                Gizmos.DrawLine(vertToPrint[i].position, vertToPrint[i + 1].position);
            }
        }

        Gizmos.color = Color.red;
        foreach (Vector3 item in points)
        {
            Gizmos.DrawSphere(item, 0.1f);
        }

        /*if (Application.isPlaying && twoDpoints != null && debugMode)
        {
            foreach (var point in twoDpoints)
            {
                if (point.Value.Walkable)
                {
                    if (!debugOnlyNonWalkable)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(point.Value.GetPosition(), 0.1f);
                    }
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(point.Value.GetPosition(), 0.1f);
                }
            }
        }*/
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
    #endregion
}
