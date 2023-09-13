using ApexInspector;
using UnityEngine;

[HideScriptField]
[DisallowMultipleComponent]
public class MeshObstacle : MonoBehaviour
{
    [SerializeField]
    private float distance = 1f;

    [SerializeField]
    private Color gizmosColor = Color.red;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmosColor;
        //Gizmos.DrawCube(transform.position, new Vector3(distance, distance, distance));
        Gizmos.DrawSphere(transform.position, distance);
    }

    #region [Getter // Setter]
    public float GetDistance()
    {
        return distance;
    }
    #endregion
}
