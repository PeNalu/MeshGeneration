using ApexInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HideScriptField]
public class AIAgent : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    [MinValue(0.1f)]
    private float speed = 3f;

    //Stored requred properties.
    private List<BaseGridNode> path;

    public void SetDestination(Transform targetTransform)
    {
        target = targetTransform;
        path = PathBuilder.FindPath(transform.position, target.position);
        StartCoroutine(MoveTo());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetDestination(target);
        }
    }

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
