using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager>
{
    public GameObject controls;
    public float zoomMobile { get; set; }
    public float movementXMobile { get; set; }
    public float movementYMobile { get; set; }

    //Point
    public static PointerEventData pointerEvent;
    //down in Input.GetXXX means click!
    public static bool SubmitClick, Submit, SubmitUp;
    //0 none 1 hold 2 up 3 click
    public ButtonPhase phase;
    public enum ButtonPhase
    {
        None,
        Hold,
        Release,
        Click
    }
    static Vector3 _pointPosition;

    //Control
    public float Zoom => Priority(Input.GetAxis("Zoom"), zoomMobile) * Time.unscaledDeltaTime * 0.5f;
    public float MovementX => Priority(Input.GetAxis("Horizontal"), movementXMobile) * Time.unscaledDeltaTime * 0.5f;
    public float MovementY => Priority(Input.GetAxis("Vertical"), movementYMobile) * Time.unscaledDeltaTime * 0.5f;

    Camera cam;
    private void Awake()
    {
        SubmitClick = false;
        Submit = false;
        SubmitUp = false;
        cam = Camera.main;
    }
    void Update()
    {
        if (pointerEvent != null)
        {
            _pointPosition = pointerEvent.position;
        }
        EditorPad.worldPoint = cam.ScreenToWorldPoint(_pointPosition);
        EditorPad.cursorGridPos = Coordinate.ConvertToGridCoordinate(EditorPad.worldPoint);

        switch (phase)
        {
            case ButtonPhase.None:
                SubmitClick = false;
                break;
            case ButtonPhase.Hold:
                Submit = true;
                break;
            case ButtonPhase.Release:
                Submit = false;
                SubmitUp = true;
                phase = ButtonPhase.Click;
                break;
            case ButtonPhase.Click:
                SubmitUp = false;
                SubmitClick = true;
                phase = ButtonPhase.None;
                break;
        }
    }
    float Priority(float a, float b) => Mathf.Abs(b) > Mathf.Abs(a) ? b : a;
    public void SetMobileControl(bool val) => controls.SetActive(val && Input.touchSupported);
}
