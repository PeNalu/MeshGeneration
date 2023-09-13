using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthSearchPF : PathBuilder
{
    protected override List<BaseGridNode> Searching(BaseGridNode startNode, BaseGridNode targetNode)
    {
        Queue<BaseGridNode> frontier = new Queue<BaseGridNode>();
        frontier.Enqueue(startNode);

        Dictionary<BaseGridNode, BaseGridNode> came_frome = new Dictionary<BaseGridNode, BaseGridNode>();
        came_frome.Add(startNode, null);

        BaseGridNode currentNode = null;

        while (frontier.Count > 0)
        {
            currentNode = frontier.Dequeue();
            
            if (currentNode == targetNode) break;

            foreach (BaseGridNode neighbor in currentNode.GetNeighbors())
            {
                if (!neighbor.Walkable) continue;

                if (!came_frome.ContainsKey(neighbor))
                {
                    frontier.Enqueue(neighbor);
                    came_frome.Add(neighbor, currentNode);
                }
            }
        }

        List<BaseGridNode> path = new List<BaseGridNode>();
        currentNode = targetNode;
        while (currentNode != startNode)
        {
            currentNode = came_frome[currentNode];
            path.Add(currentNode);
        }

        path.Add(startNode);
        path.Reverse();
        return path;
    }
}
