using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    protected virtual void Awake()
    {
        transform.position = Vector3.zero;

        instance = GetComponent<T>();
        if (transform == transform.root)
        {
            DontDestroyOnLoad(this);
        }
    }

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                T[] instances = FindObjectsOfType<T>();

                if (instances != null && instances.Length > 0)
                {
                    instance = instances[0];
                    if (instances.Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                                " - there should never be more than 1 singleton!" +
                                " Reopening the scene might fix it.");
                    }
                }

                if (instance == null)
                {
                    GameObject singleton = new GameObject($"[Singleton] {typeof(T).Name}");
                    instance = singleton.AddComponent<T>();
                }
            }

            return instance;
        }
    }
}
