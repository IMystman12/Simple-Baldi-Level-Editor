using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entrance : Posable
{
    [SerializeField] private CircleCollider2D circleCollider;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            circleCollider.enabled = false;
            BaldiManager.Instance.GetEntrance(this);
        }
    }
    public void ResetState()
    {
        circleCollider.enabled = true;
    }
}
