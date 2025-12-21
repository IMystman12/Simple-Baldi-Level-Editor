using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mark : MonoBehaviour
{
    public void OnEnter()
    {
        transform.localScale = Vector3.one * 3;
    }
    public void OnExit()
    {
        transform.localScale = Vector3.one;
    }
}
