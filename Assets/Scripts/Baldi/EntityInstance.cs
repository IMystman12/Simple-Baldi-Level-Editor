using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EntityInstance : BalPosable
{
    public EnvironmentController ec => EditorPad.Instance.ec;
    public Collider2D collider;
    public Rigidbody2D rb;
    public virtual void GameStart()
    {
        foreach (var a in BaldiManager.Instance.entities)
        {
            Physics2D.IgnoreCollision(collider, a.collider);
        }
    }

    public virtual void GameUpdate()
    {

    }
    const float radtodeg = 180 / Mathf.PI;
    protected void Move(Vector2 val)
    {
        rb.velocity = val;
        if (val.sqrMagnitude != 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(val.y, val.x) * radtodeg) - 90);
        }
    }

    public static void DrawGridPosition(Coordinate coordinate)
    {
        Debug.DrawRay(Coordinate.ConvertToMapCoordinate(coordinate) - Vector2.one * 0.5f, Vector2.one, Color.green);
        Debug.DrawRay(Coordinate.ConvertToMapCoordinate(coordinate) - new Vector2(-0.5f, 0.5f), new Vector2(-1, 1), Color.green);
    }
}
