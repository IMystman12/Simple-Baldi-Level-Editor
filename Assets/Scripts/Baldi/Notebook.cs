using UnityEngine;

public class Notebook : BalPosable
{
    [SerializeField] private CircleCollider2D circleCollider;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            circleCollider.enabled = false;
            spriteRenderer.enabled = false;
            BaldiManager.Instance.GetNotebook();
        }
    }
    public void ResetState()
    {
        circleCollider.enabled = true;
        spriteRenderer.enabled = true;
    }
}
