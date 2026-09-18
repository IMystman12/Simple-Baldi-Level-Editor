using UnityEngine;

public class Reposer : MonoBehaviour
{
    bool val;
    public Vector3 on, off;
    void Start() => Switch();
    public void Switch()
    {
        val = !val;
        transform.localPosition = val ? on : off;
    }
}
