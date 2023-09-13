using ApexInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HideScriptField]
public class AIAgent : MonoBehaviour
{
    [SerializeField]
    [MinValue(0.1f)]
    private float speed = 3f;

    //Stored requred properties.
    private List<BaseGridNode> path;

    /// <summary>
    /// Sets the movement goal for the agent.
    /// </summary>
    /// <param name="targetTransform"></param>
    public void SetDestination(Transform targetTransform)
    {
        path = PathBuilder.FindPath(transform.position, targetTransform.position);
        StartCoroutine(MoveTo());
    }

    /// <summary>
    /// Responsible for the logic of movement from one point to another along the found route.
    /// </summary>
    private IEnumerator MoveTo()
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 star = path[i].GetPosition();
            Vector3 end = path[i + 1].GetPosition();

            float time = 0;
            while(time < 1f)
            {
                time += speed * Time.deltaTime;
                transform.position = Vector3.Lerp(star, end, time);
                yield return null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            foreach (var node in path)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(node.GetPosition(), 0.1f);
            }
        }
    }
}
