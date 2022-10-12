using ApexInspector;
using System.Collections.Generic;
using UnityEngine;

[HideScriptField]
public class ArraysExample : MonoBehaviour
{
    [Array]
    public string[] names;

    [ReorderableList]
    public List<Transform> points;
}
