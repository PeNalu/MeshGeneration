using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjPlaser : MonoBehaviour
{
    [SerializeField]
    private Transform startPoint;

    [SerializeField]
    private Transform endPoint;

    [SerializeField]
    private int copyNumber;

    [SerializeField]
    private float y;

    [SerializeField]
    private GameObject obj;

    //Stored required components.
    [SerializeField]
    private List<GameObject> copies;

    //[ContextMenu("Plase")]
    public void Plase()
    {
        Clear();
        float length = Vector3.Distance(startPoint.position, endPoint.position);
        float segmentLength = length / (copyNumber - 1);
        Vector3 copyPos = startPoint.position;

        for (int i = 0; i < copyNumber; i++)
        {
            if(i != 0)
            {
                copyPos = startPoint.position + startPoint.forward * (segmentLength * i);
            }
            GameObject objCopy = Instantiate(obj, copyPos, Quaternion.Euler(0, y, 0));
            copies.Add(objCopy);
        }
    }

    //[ContextMenu("Clear")]
    public void Clear()
    {
        for (int i = copies.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(copies[i].gameObject);
        }

        copies.Clear();
    }

    //[ContextMenu("LClear")]
    public void LClear()
    {
        copies.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPoint.position, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPoint.position, 0.1f);
    }

    //[MenuItem("Tools/Print Length")]
    public static void PlintCount()
    {
        print(Selection.objects.Length);
    }
}
