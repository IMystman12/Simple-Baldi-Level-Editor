using UnityEngine;
using UnityEngine.Events;

public class AbleEvents : MonoBehaviour
{
    public UnityEvent onEn, onDis;
    private void OnEnable()
    {
        onEn?.Invoke();
    }
    void OnDisable()
    {
        onDis?.Invoke();
    }
}
