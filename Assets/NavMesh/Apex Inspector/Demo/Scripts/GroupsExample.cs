using ApexInspector;
using UnityEngine;

[HideScriptField]
public class GroupsExample : MonoBehaviour
{
    [Group("Main Group")]
    public float a = 1;

    [Group("Main Group")]
    [Foldout("Foldout Group")]
    public float b = 2;

    [Group("Main Group")]
    [TabGroup("Tab Group", "Section 1")]
    public float c = 3;

    [Group("Main Group")]
    [TabGroup("Tab Group", "Section 2")]
    public float d = 4;
}
