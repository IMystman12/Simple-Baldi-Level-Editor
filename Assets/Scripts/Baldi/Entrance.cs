using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entrance : BalPosable
{
    [SerializeField] private CircleCollider2D circleCollider;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && BaldiManager.Instance.GetEntrance(this))
        {
            circleCollider.enabled = false;
        }
    }
    public void ResetState()
    {
        circleCollider.enabled = true;
    }
}
