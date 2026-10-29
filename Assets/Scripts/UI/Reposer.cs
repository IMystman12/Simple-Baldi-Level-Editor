using UnityEngine;

public class Reposer : MonoBehaviour
{
    bool val;
    public Vector3 on, off;
    public void Switch()
    {
        val = !val;
        transform.localPosition = val ? on : off;
    }
}
