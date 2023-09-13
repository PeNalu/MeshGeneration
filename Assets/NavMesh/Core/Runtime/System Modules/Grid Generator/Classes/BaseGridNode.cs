using System.Collections.Generic;
using UnityEngine;

public class BaseGridNode
{
    #region [Properties]
    public float G;
    public float H;
    public float F => G + H + cost;
    public bool Walkable = true;

    private int cost;
    private int maxSlope = 50;
    private Vector3 pos;
    private Vector2Int matrixPos;

    private List<BaseGridNode> neighbors;
    private BaseGridNode connection;

    private bool hashing = true;
    #endregion

    public BaseGridNode(int cost, Vector3 pos, Vector2Int matrixPos)
    {
        this.cost = cost;
        this.pos = pos;
        this.matrixPos = matrixPos;
    }

    public virtual void CalculateNeighbors()
    {
        neighbors = new List<BaseGridNode>();
        BaseGridNode[,] matrix = BaseGridGenerator.Instance.GetMatrix();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (!(x == 0 && z == 0) && matrixPos.x + x > 0 && matrixPos.x + x < matrix.GetLength(0) &&
                    matrixPos.y + z > 0 && matrixPos.y + z < matrix.GetLength(1))
                {
                    if(matrix[matrixPos.x + x, matrixPos.y + z] != null) 
                    {
                        neighbors.Add(matrix[matrixPos.x + x, matrixPos.y + z]);
                    }
                }
            }
        }
    }

    public virtual float GetDistance(Vector2Int otherPos)
    {
        Vector2Int pos = GetTwoDPosition();
        return Vector2Int.Distance(pos, otherPos);
    }

    #region [Getter / Setter]
    public List<BaseGridNode> GetNeighbors()
    {
        if(neighbors == null || hashing)
        {
            neighbors = new List<BaseGridNode>();
            BaseGridNode[,] matrix = BaseGridGenerator.Instance.GetMatrix();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (!(x == 0 && z == 0) && matrixPos.x + x > 0 && matrixPos.x + x < matrix.GetLength(0) &&
                        matrixPos.y + z > 0 && matrixPos.y + z < matrix.GetLength(1))
                    {
                        if (matrix[matrixPos.x + x, matrixPos.y + z] != null)
                        {
                            Vector3 pos = matrix[matrixPos.x + x, matrixPos.y + z].GetPosition();
                            if (!Physics.Linecast(GetPosition(), pos))
                            {
                                Vector3 targetDirection = pos - GetPosition();
                                Vector3 forwardDirection = targetDirection;
                                forwardDirection.y = 0;
                                float angle = Vector3.SignedAngle(targetDirection, forwardDirection, Vector3.up);

                                if (Mathf.Abs(angle) < maxSlope)
                                {
                                    neighbors.Add(matrix[matrixPos.x + x, matrixPos.y + z]);
                                }
                            }
                        }
                    }
                }
            }
        }

        return neighbors;
    }

    public Vector2Int GetMatrixPosition()
    {
        return matrixPos;
    }

    public float GetCost()
    {
        return G + H + cost;
    }

    public void SetConnection(BaseGridNode node)
    {
        connection = node;
    }

    public BaseGridNode GetConnection()
    {
        return connection;
    }

    public Vector3 GetPosition()
    {
        return pos;
    }

    public Vector2Int GetTwoDPosition()
    {
        return new Vector2Int((int)pos.x, (int)pos.z);
    }
    #endregion
}
