using System;
using System.Collections.Generic;

public class DijkstraSearchPF : PathBuilder
{
    protected override List<BaseGridNode> Searching(BaseGridNode startNode, BaseGridNode targetNode)
    {
        PriorityQueue<BaseGridNode> frontier = new PriorityQueue<BaseGridNode>();
        frontier.Add(0, startNode);

        Dictionary<BaseGridNode, BaseGridNode> came_frome = new Dictionary<BaseGridNode, BaseGridNode>();
        came_frome.Add(startNode, null);

        Dictionary<BaseGridNode, int> cost_so_far = new Dictionary<BaseGridNode, int>();
        cost_so_far.Add(startNode, 0);
        BaseGridNode currentNode = null;

        while (frontier.Count > 0)
        {
            currentNode = frontier.RemoveMin();

            if (currentNode == targetNode) break;

            foreach (BaseGridNode neighbor in currentNode.GetNeighbors())
            {
                int newCost = cost_so_far[currentNode] + (int)currentNode.GetDistance(neighbor.GetTwoDPosition());

                if (!neighbor.Walkable) continue;

                if(!cost_so_far.ContainsKey(neighbor) || newCost < cost_so_far[neighbor])
                {
                    if (came_frome.ContainsKey(neighbor))
                    {
                        cost_so_far[neighbor] = newCost;
                        frontier.Add(newCost, neighbor);
                        came_frome[neighbor] = currentNode;
                    }
                    else
                    {
                        cost_so_far.Add(neighbor, newCost);
                        frontier.Add(newCost, neighbor);
                        came_frome.Add(neighbor, currentNode);
                    }
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

    class MinHeap<T> where T : IComparable<T>
    {
        private List<T> array = new List<T>();

        public void Add(T element)
        {
            array.Add(element);
            int c = array.Count - 1;
            while (c > 0 && array[c].CompareTo(array[c / 2]) == -1)
            {
                T tmp = array[c];
                array[c] = array[c / 2];
                array[c / 2] = tmp;
                c = c / 2;
            }
        }

        public T RemoveMin()
        {
            T ret = array[0];
            array[0] = array[array.Count - 1];
            array.RemoveAt(array.Count - 1);

            int c = 0;
            while (c < array.Count)
            {
                int min = c;
                if (2 * c + 1 < array.Count && array[2 * c + 1].CompareTo(array[min]) == -1)
                    min = 2 * c + 1;
                if (2 * c + 2 < array.Count && array[2 * c + 2].CompareTo(array[min]) == -1)
                    min = 2 * c + 2;

                if (min == c)
                    break;
                else
                {
                    T tmp = array[c];
                    array[c] = array[min];
                    array[min] = tmp;
                    c = min;
                }
            }

            return ret;
        }

        public T Peek()
        {
            return array[0];
        }

        public int Count
        {
            get
            {
                return array.Count;
            }
        }
    }

    class PriorityQueue<T>
    {
        internal class Node : IComparable<Node>
        {
            public int Priority;
            public T O;
            public int CompareTo(Node other)
            {
                return Priority.CompareTo(other.Priority);
            }
        }

        private MinHeap<Node> minHeap = new MinHeap<Node>();

        public void Add(int priority, T element)
        {
            minHeap.Add(new Node() { Priority = priority, O = element });
        }

        public T RemoveMin()
        {
            return minHeap.RemoveMin().O;
        }

        public T Peek()
        {
            return minHeap.Peek().O;
        }

        public int Count
        {
            get
            {
                return minHeap.Count;
            }
        }
    }
}
