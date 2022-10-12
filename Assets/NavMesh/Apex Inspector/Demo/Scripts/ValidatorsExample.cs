using ApexInspector;
using UnityEngine;

[HideScriptField]
public class ValidatorsExample : MonoBehaviour
{
    [NotNull]
    public GameObject someObject;

    [MinValue(0)]
    public float minValue = 0.0f;

    [MaxValue(10)]
    public float maxValue = 10.0f;

    [MinValue("minValue")]
    [MaxValue("maxValue")]
    public float dynamicValue = 7.0f;

    [AssetOnly]
    public Object asset;

    [SceneObjectOnly]
    public GameObject sceneObject;
}
