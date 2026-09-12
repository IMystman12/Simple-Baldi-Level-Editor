using UnityEngine;

public class Grid : MonoBehaviour
{
    public SpriteRenderer bg, cursorCrossX, cursorCrossY;
    Vector3 vector;
    void Update()
    {
        vector = Camera.main.transform.position;
        bg.transform.position = new Vector3(Mathf.Floor(vector.x / 10) * 10 - 0.5f, Mathf.Floor(vector.y / 10) * 10 - 0.5f, 0);
        if (EditorPad.Instance.inArea)
        {
            vector = (Vector3)IntVector2.ToVector2(EditorPad.Instance.cursorGridPos) + Vector3.forward;
            vector.x = Mathf.Clamp(vector.x, 0, EditorPad.Instance.ec.realSize.x);
            vector.y = Mathf.Clamp(vector.y, 0, EditorPad.Instance.ec.realSize.z);
            cursorCrossX.transform.position = vector;
            cursorCrossY.transform.position = vector;
        }
    }
}
