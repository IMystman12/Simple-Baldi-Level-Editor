using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : EntityInstance
{
    [SerializeField] private float run = 2.4f;
    public override void GameUpdate()
    {
        Move(Vector2.ClampMagnitude(Vector3.up * InputManager.Instance.MovementY + Vector3.right * InputManager.Instance.MovementX, 1) * run * BaldiManager.Instance.playerTimeScale);
    }
}
