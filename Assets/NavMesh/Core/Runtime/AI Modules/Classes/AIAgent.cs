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
    private bool endRefind = true;
    private Vector3 storedPos;


    private void Start()
    {
        if (target != null)
        {
            SetDestination(target);
        }
    }

    public void SetDestination(Transform targetTransform)
    {
        target = targetTransform;
        //path = PathBuilder.FindPath(transform.position, target.position);
        StartCoroutine(FindPath());
    }

    private void Update()
    {
        /*if (endRefind)
        {
            if (storedPos != target.position)
            {
                path = PathBuilder.FindPath(transform.position, target.position);
                if (path != null)
                {
                    storedPos = target.position;
                    path.Reverse();
                }
            }
        }*/

        if (Input.GetKeyDown(KeyCode.E))
        {
            endRefind = false;
            StartCoroutine(MoveTo());
        }
    }

    private IEnumerator FindPath()
    {
        while (true)
        {
            if (endRefind)
            {
                if (storedPos != target.position)
                {
                    path = PathBuilder.FindPath(transform.position, target.position);
                    if (path != null)
                    {
                        storedPos = target.position;
                        path.Reverse();
                    }
                    else
                    {
                        yield return new WaitForSeconds(1);
                    }
                }
            }

            yield return null;
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
