using ApexInspector;
using System.Collections;
using UnityEngine;

[HideScriptField]
[DisallowMultipleComponent]
public class Target : MonoBehaviour
{
    [SerializeField]
    [MinValue(1f)]
    private float timeToCall = 10f;

    [SerializeField]
    [NotNull]
    private AIAgent agent;

    //Stored required properties.
    private WaitForSeconds wait;
    private Coroutine coroutine;

    private void Awake()
    {
        wait = new WaitForSeconds(timeToCall);
    }

    private void OnEnable()
    {
        coroutine = StartCoroutine(Timer());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
    }

    private IEnumerator Timer()
    {
        while (true)
        {
            yield return wait;
            agent.SetDestination(transform);
            yield return null;
        }
    }
}
