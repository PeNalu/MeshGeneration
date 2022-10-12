using System.Collections.Generic;
using UnityEngine;

public class PathBuilder : Singleton<PathBuilder>
{
    public static List<BaseGridNode> FindPath(Vector3 pos, Vector3 targetPos)
    {
        BaseGridNode startNode = BaseGridGenerator.Instance.GetNodeByPosition(new Vector2Int((int)pos.x, (int)pos.z));
        BaseGridNode endNode = BaseGridGenerator.Instance.GetNodeByPosition(new Vector2Int((int)targetPos.x, (int)targetPos.z));

        return Instance.Searching(startNode, endNode);
    }

    protected virtual List<BaseGridNode> Searching(BaseGridNode startNode, BaseGridNode targetNode) { return null; }
}
