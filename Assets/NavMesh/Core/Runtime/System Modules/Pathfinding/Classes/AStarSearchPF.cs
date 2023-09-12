using System.Collections.Generic;
using System.Linq;

public class AStarSearchPF : PathBuilder
{
    protected override List<BaseGridNode> Searching(BaseGridNode startNode, BaseGridNode targetNode)
    {
        if (targetNode == null) return null;

        List<BaseGridNode> toSearch = new List<BaseGridNode>() { startNode };
        List<BaseGridNode> processed = new List<BaseGridNode>();

        while (toSearch.Count > 0)
        {
            BaseGridNode current = toSearch[0];

            foreach (BaseGridNode t in toSearch)
            {
                if (CompareCost(current, t) > 0)
                {
                    current = t;
                }
            }

            processed.Add(current);
            toSearch.Remove(current);

            if (current == targetNode)
            {
                BaseGridNode currentPathTile = targetNode;
                List<BaseGridNode> path = new List<BaseGridNode>();
                while (currentPathTile != startNode)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.GetConnection();
                }

                path.Reverse();
                return path;
            }

            foreach (BaseGridNode neighbor in current.GetNeighbors().Where(t => t.Walkable && !processed.Contains(t)))
            {
                bool inSearch = toSearch.Contains(neighbor);

                float costToNeighbor = current.G + current.GetDistance(neighbor.GetTwoDPosition());

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.G = costToNeighbor;
                    neighbor.H = neighbor.GetDistance(targetNode.GetTwoDPosition());
                    neighbor.SetConnection(current);

                    if (!inSearch)
                    {
                        toSearch.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private int CompareCost(BaseGridNode a, BaseGridNode b)
    {
        int compare = a.F.CompareTo(b.F);

        if (compare == 0)
        {
            compare = a.H.CompareTo(b.H);
        }

        return compare;
    }
}
