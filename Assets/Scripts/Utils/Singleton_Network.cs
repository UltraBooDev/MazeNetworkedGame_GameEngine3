using UnityEngine;
using Unity.Netcode;

public class Singleton_Network<T> : NetworkBehaviour where T : NetworkBehaviour
{
    static bool appIsQuitting = false;

    static object _lock = new object();

    static T instance;
    public static T Instance
    {
        get
        {
            if (appIsQuitting)
            {
                Debug.LogWarning($"{typeof(T)} Instance: App is quitting. Returning Null.");
                return null;
            }

            lock (_lock)
            {
                if (instance != null) { return instance; }

                instance = FindObjectOfType<T>();

                if (FindObjectsOfType(typeof(T)).Length > 1)
                {
                    Debug.LogWarning($"{typeof(T)} Instance: More than one instance exists.");
                    return instance;
                }

                if (instance == null)
                {
                    GameObject singleton = new GameObject();
                    instance = singleton.AddComponent<T>();
                    singleton.name = typeof(T).ToString();
                }

                return instance;
            }
        }
    }


    public override void OnDestroy()
    {
        if (this == instance) appIsQuitting = true;
        base.OnDestroy();
    }

    void Start()
    {
        if (Instance && this != instance)
        {
            Destroy(gameObject);
        }
    }
}

