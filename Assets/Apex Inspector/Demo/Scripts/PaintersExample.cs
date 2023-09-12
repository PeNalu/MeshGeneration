using ApexInspector;
using UnityEngine;

[HideScriptField]
public class PaintersExample : MonoBehaviour
{
    [Message("Some text here...", MessageStyle.Warning)]
    [PropertySpace(10)]
    public float value;

    [Prefix("(Experimental)", true)]
    public string text;

    [Suffix("m/s")]
    public float speed;

    [Space(10)]
    [Indent(0)]
    public int level0;

    [Indent(1)]
    public int level1;

    [Indent(2)]
    public int level2;

    [Indent(3)]
    public int level3;
}
