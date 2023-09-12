using ApexInspector;
using UnityEngine;

[HideScriptField]
public class GridSurface : MonoBehaviour
{
    [SerializeField]
    [Label("Radius")]
    private int radius;

    [SerializeField]
    private bool debugMode;

    [SerializeField]
    [VisibleIf("debugMode")]
    private Color gizmosColor;

    private void Awake()
    {
        BaseGridGenerator.Instance.BuildGrid(transform, new Vector2Int(radius, radius));
    }

    private void OnDrawGizmosSelected()
    {
        if (debugMode)
        {
            Gizmos.color = gizmosColor;
            Gizmos.DrawCube(transform.position, new Vector3(radius * 2, (transform.position.y + 10) * 2, radius * 2));
        }
    }
}
