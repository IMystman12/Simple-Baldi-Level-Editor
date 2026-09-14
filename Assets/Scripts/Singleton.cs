using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Object
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
            }
            return instance;
        }
    }
}
