using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    [SerializeField]
    private HashSet<Vector3> points;

    //Stored required properties.
    private Vector3 center;

    public Cluster(HashSet<Vector3> points)
    {
        this.points = points;
        center = CalculateCenter();
    }

    private Vector3 CalculateCenter()
    {
        Bounds bounds = new Bounds();
        foreach (Vector3 point in points)
        {
            bounds.Encapsulate(point);
        }

        return bounds.center;
    }

    #region [Getter/ Setter]
    public Vector3 GetCenter()
    {
        return center;
    }

    public HashSet<Vector3> GetPoints()
    {
        return points;
    }
    #endregion
}
