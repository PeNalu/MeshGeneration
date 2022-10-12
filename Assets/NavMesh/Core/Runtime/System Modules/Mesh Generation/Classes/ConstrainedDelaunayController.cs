using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstrainedDelaunayController : MonoBehaviour
{
    [SerializeField]
    private List<Transform> transforms;

    [SerializeField]
    private List<Transform> constrainedTransforms;

    [SerializeField]
    private GameObject gmObj;

    [SerializeField]
    private bool optimize = false;

    public int seed = 0;
    public int numberOfPoints = 20;
    public float radius = 10f;

    private void OnDrawGizmos()
    {
        //Generate the random sites
        //List<Vector3> points = transforms.Select(x => x.position).ToList();
        List<Vector3> constrainedPoints = constrainedTransforms.Select(x => x.position).ToList();
        List<Vector3> points = new List<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = radius;
        float min = -radius;

        /*        for (int i = 0; i < numberOfPoints; i++)
                {
                    float randomX = Random.Range(min, max);
                    float randomZ = Random.Range(min, max);

                    points.Add(new Vector3(randomX, transform.position.y, randomZ));
                }*/

        for (int i = 0; i < radius; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                points.Add(new Vector3(i + Random.Range(0, 0.15f), 0, j + Random.Range(0, 0.15f)));
            }
        }

        Gizmos.color = Color.red;
        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }

        List<Triangle> triangles = new List<Triangle>();
        if (optimize)
        {
            List<Vertex> vertices = new List<Vertex>();
            foreach (Vector3 pos in points)
            {
                vertices.Add(new Vertex(pos));
            }
            points = JarvisMarchAlgorithm.GetConvexHull(vertices).Select(x => x.position).ToList();
        }

        triangles = Triangulate.TriangulateByFlippingEdges(points);
        //triangles = ConstrainedDelaunay.GenerateTriangulation(points, constrainedPoints);

        //List<Triangle> triangles = Triangulate.TriangulateByFlippingEdges(points);
        List<TriangleTriangleIntersection.Triangle> tri = new List<TriangleTriangleIntersection.Triangle>();
        foreach (var item in triangles)
        {
            TriangleTriangleIntersection.Triangle triangle = new TriangleTriangleIntersection.Triangle();
            triangle.p1 = item.v1.position;
            triangle.p2 = item.v2.position;
            triangle.p3 = item.v3.position;

            tri.Add(triangle);
        }

        TriangleTriangleIntersection.CreateTriangleMesh(tri, gmObj);
    }
}
