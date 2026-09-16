using UnityEngine;

public class Guide : MonoBehaviour
{
    public SpriteRenderer bg, cursorCrossX, cursorCrossY;
    Vector3 vector;
    void Update()
    {
        vector = Camera.main.transform.position;
        bg.transform.position = new Vector3(Mathf.Floor(vector.x / 10) * 10 - 0.5f, Mathf.Floor(vector.y / 10) * 10 - 0.5f, 10);
        if (!EditorPad.Instance.pause && EditorPad.Instance.inArea)
        {
            vector = (Vector3)IntVector2.ToVector2(EditorPad.Instance.cursorGridPos) + Vector3.forward * 10;
            cursorCrossX.transform.position = vector;
            cursorCrossY.transform.position = vector;
        }
    }
}
